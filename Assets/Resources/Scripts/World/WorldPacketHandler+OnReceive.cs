using System.Collections.Generic;
using UnityEngine;

namespace Resources.Scripts.World
{
    public partial class WorldPacketHandler
    {
        public EnemyManager EnemyManager;
        private void _OnMonsterUpdateCommand(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<MonsterUpdateCommand>(data);

            foreach (var monster in packet.Monsters)
            {
                EnemyManager.Move(monster.Id, new Vector3(monster.Position.X, monster.Position.Y, monster.Position.Z), 0);
            }
        }

        private void _OnItemUseCommand(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<UseItemCommand>(data);
            Debug.Log($"Item Use {packet.ItemId}");
        }

        private void _OnSpawnGameObject(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<UpdateGameObjects>(data);
            if (packet == null)
                return;

            if (packet.IsSpawn == false)
            {
                _DespawnObject(packet.GameObjects);
                return;
            }
            
            foreach (var obj in packet.GameObjects)
            {
                if (obj.Type == GameObjectType.Monster)
                {
                    var pos = new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Y);
                    EnemyManager.Spawn(obj.Id, pos, 0f);
                }
            }
        }

        private void _DespawnObject(List<GameObjectBase> despawnObjects)
        {
            foreach (var obj in despawnObjects)
            {
                if (obj.Type == GameObjectType.Monster)
                {
                    EnemyManager.Despawn(obj.Id);
                }
            }
        }
    }
}