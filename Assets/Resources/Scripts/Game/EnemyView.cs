using System.Collections.Generic;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
    public long Id { get; private set; }

    // ✅ 0.12~0.2 사이 추천 (서버 10Hz면 0.15 전후가 보통 제일 자연스러움)
    public float interpolationBackTime = 0.15f;

    struct Snapshot
    {
        public float t;
        public Vector3 pos;
        public float yawDeg;
    }

    private readonly List<Snapshot> _buf = new(32);

    public void Initialize(long id, Vector3 pos, float yawDeg = 0f)
    {
        Id = id;
        _buf.Clear();
        var now = Time.time;
        _buf.Add(new Snapshot { t = now, pos = pos, yawDeg = yawDeg });

        transform.position = pos;
        transform.rotation = Quaternion.Euler(0, yawDeg, 0);
    }

    // 서버 패킷 받을 때마다 호출 (10Hz)
    public void PushServerState(Vector3 pos, float yawDeg)
    {
        float now = Time.time;
        _buf.Add(new Snapshot { t = now, pos = pos, yawDeg = yawDeg });

        // 너무 오래된 건 정리
        while (_buf.Count > 30) _buf.RemoveAt(0);
    }

    void Update()
    {
        if (_buf.Count < 2) return;

        float renderTime = Time.time - interpolationBackTime;

        // renderTime보다 오래된 스냅샷 정리
        while (_buf.Count >= 2 && _buf[1].t <= renderTime)
            _buf.RemoveAt(0);

        if (_buf.Count < 2) return;

        var a = _buf[0];
        var b = _buf[1];

        float len = b.t - a.t;
        float t = (len > 0.0001f) ? Mathf.Clamp01((renderTime - a.t) / len) : 1f;

        // 위치 보간
        transform.position = Vector3.Lerp(a.pos, b.pos, t);

        // 회전 보간
        var ra = Quaternion.Euler(0, a.yawDeg, 0);
        var rb = Quaternion.Euler(0, b.yawDeg, 0);
        transform.rotation = Quaternion.Slerp(ra, rb, t);
    }
}
