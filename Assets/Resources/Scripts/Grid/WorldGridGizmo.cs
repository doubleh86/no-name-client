using UnityEngine;

[ExecuteAlways]
public class WorldGridGizmo : MonoBehaviour
{
    [Header("Auto Sync with Terrain (Recommended)")]
    public Terrain terrain; // Assign your Terrain here
    public bool syncFromTerrain = true;

    [Header("Map Settings (Fallback if not syncing)")]
    public float sizeX = 2000f;
    public float sizeZ = 2000f;
    public float cellSize = 25f;

    [Header("Origin (A-mode: bottom-left = 0,0)")]
    public Vector3 origin = Vector3.zero;

    [Header("Gizmo")]
    public bool drawGrid = true;
    public bool drawBorder = true;

    [Header("Debug Target")]
    public Transform target;
    public bool drawTargetCell = true;

    private int MaxCellX => Mathf.RoundToInt(sizeX / cellSize);
    private int MaxCellZ => Mathf.RoundToInt(sizeZ / cellSize);

    private void OnValidate()
    {
        ApplySync();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            ApplySync();
    }

    private void ApplySync()
    {
        if (!syncFromTerrain || terrain == null || terrain.terrainData == null)
            return;

        // Terrain 좌하단이 곧 origin
        origin = terrain.GetPosition();

        // Terrain 실제 사이즈
        var sz = terrain.terrainData.size;
        sizeX = sz.x;
        sizeZ = sz.z;
    }

    private void OnDrawGizmos()
    {
        if (!drawGrid || cellSize <= 0) return;

        // 약간 띄워서 지형에 묻히는 느낌 방지
        float y = origin.y + 0.05f;

        // Border
        if (drawBorder)
        {
            Gizmos.color = Color.white;
            var p0 = new Vector3(origin.x, y, origin.z);
            var p1 = new Vector3(origin.x + sizeX, y, origin.z);
            var p2 = new Vector3(origin.x + sizeX, y, origin.z + sizeZ);
            var p3 = new Vector3(origin.x, y, origin.z + sizeZ);

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }

        // Grid
        Gizmos.color = new Color(1f, 1f, 1f, 0.25f);

        int maxX = MaxCellX;
        int maxZ = MaxCellZ;

        for (int x = 0; x <= maxX; x++)
        {
            float wx = origin.x + x * cellSize;
            var a = new Vector3(wx, y, origin.z);
            var b = new Vector3(wx, y, origin.z + sizeZ);
            Gizmos.DrawLine(a, b);
        }

        for (int z = 0; z <= maxZ; z++)
        {
            float wz = origin.z + z * cellSize;
            var a = new Vector3(origin.x, y, wz);
            var b = new Vector3(origin.x + sizeX, y, wz);
            Gizmos.DrawLine(a, b);
        }

        // Target Cell
        if (target != null && drawTargetCell)
        {
            if (TryGetCellCoord(target.position, out int cx, out int cz))
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
                var cellMin = new Vector3(origin.x + cx * cellSize, y, origin.z + cz * cellSize);
                var center = cellMin + new Vector3(cellSize * 0.5f, 0.05f, cellSize * 0.5f);
                var size = new Vector3(cellSize, 0.1f, cellSize);
                Gizmos.DrawCube(center, size);
            }
        }
    }

    public bool TryGetCellCoord(Vector3 worldPos, out int cx, out int cz)
    {
        float lx = worldPos.x - origin.x;
        float lz = worldPos.z - origin.z;

        if (lx < 0 || lz < 0 || lx >= sizeX || lz >= sizeZ)
        {
            cx = cz = -1;
            return false;
        }

        cx = Mathf.FloorToInt(lx / cellSize);
        cz = Mathf.FloorToInt(lz / cellSize);
        return true;
    }
}
