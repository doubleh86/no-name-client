using UnityEngine;

namespace Resources.Scripts.World
{
    public partial class WorldPacketHandler
    {
        private void _OnMonsterUpdateCommand(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<MonsterUpdateCommand>(data);

            foreach (var monster in packet.Monsters)
            {
                UnityEngine.Debug.Log($"Monster Update : {monster.Id}|Position : {monster.Position}|{monster.State}");
            }
            // Console.WriteLine($"Monster Update : {packet.Monsters.Count}");
        }

        private void _OnItemUseCommand(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<UseItemCommand>(data);
            UnityEngine.Debug.Log($"Item Use {packet.ItemId}");
        }

        private void _OnSpawnGameObject(byte[] data)
        {
            var packet = MemoryPackHelper.Deserialize<UpdateGameObjects>(data);
            if (packet == null)
                return;

            if (packet.IsSpawn == false)
                return;

            if (packet.GameObjects == null)
                return;
            
            var list = packet.GameObjects.FindAll(x => x.Type == GameObjectType.Player);
            var list2 = packet.GameObjects.FindAll(x => x.Type == GameObjectType.Monster);
            UnityEngine.Debug.Log($"Spawn Monster {list2.Count} | Spawn Type : {packet.IsSpawn}");
            UnityEngine.Debug.Log($"Spawn Player {list.Count} | Spawn Type : {packet.IsSpawn}");

        }
    }
}