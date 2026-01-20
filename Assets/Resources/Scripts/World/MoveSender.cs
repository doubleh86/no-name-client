using Resources.Scripts.Network;
using Resources.Scripts.World;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class MoveSender : MonoBehaviour
{
    public WorldPacketHandler handler;
    public Transform player;
    private float _acc;

    
    void Update()
    {
        if (handler == null || player == null) return;

        _acc += Time.deltaTime;
        if (_acc < 0.1f) return; // 10Hz
        _acc = 0f;

        _SendMoveCommand(handler.GetNetworkSession(), player.rotation.eulerAngles.y);
    }
    
    private void _SendMoveCommand(NetworkSession client, float rotation)
    {
        if(client == null) 
            return;
        
        client.SendGameCommand(new GameCommandRequest
                               {
                                   CommandId = (int)GameCommandId.MoveCommand,
                                   CommandData = MemoryPackHelper.Serialize(new MoveCommand()
                                                                            {
                                                                                Position = new Vector3(player.position.x, player.position.y, player.position.z),
                                                                                Rotation = rotation
                                                                            })
                               });
    }
}