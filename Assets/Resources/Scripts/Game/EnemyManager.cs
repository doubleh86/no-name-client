using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public EnemyView EnemyPrefab; 
    private readonly Dictionary<long, EnemyView> _Enemies = new();

    public void Spawn(long id, Vector3 pos, float yawDeg)
    {
        if (_Enemies.ContainsKey(id) == true)
            return;
        
        var enemy = Instantiate(EnemyPrefab, pos, Quaternion.identity);
        enemy.name = $"Enemy_{id}";
        enemy.Initialize(id, pos, yawDeg);
        
        _Enemies.Add(id, enemy);
    }

    public void Move(long id, Vector3 pos, float yawDeg)
    {
        if(_Enemies.TryGetValue(id, out var enemy) == false)
        {
            Spawn(id, pos, yawDeg);
            return;
        }
        
        enemy.PushServerState(pos, yawDeg);
    }

    public void Despawn(long id)
    {
        if (_Enemies.TryGetValue(id, out var enemy) == false)
        {
            return;
        }
        
        Destroy(enemy.gameObject);
        _Enemies.Remove(id);
    }
}
