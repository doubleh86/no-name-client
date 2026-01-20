using Resources.Scripts.Network;
using Resources.Scripts.World;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

public class MoveSender : MonoBehaviour
{
    public WorldPacketHandler handler;
    private float _acc;

    public float sendInterval = 0.1f;  // 100ms

    void Update()
    {
        if (handler == null)
            return;

        _acc += Time.deltaTime;
        if (_acc < sendInterval)
            return;

        _acc = 0f;

        // 위치/회전
        UnityEngine.Vector3 pos = transform.position;
        float yawDeg = transform.eulerAngles.y;

        // ✅ 여기를 형 패킷 함수로 바꿔서 호출하면 끝
        _SendMoveCommand(handler.GetNetworkSession(), yawDeg);
    }

    
    private void _SendMoveCommand(NetworkSession client, float rotation)
    {
        if(client == null || client.IsConnected() == false) 
            return;
        
        var position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        var moveCommand = new MoveCommand
                          {
                              Position = position,
                              Rotation = rotation
                          };
        
        client.SendGameCommand(new GameCommandRequest
                               {
                                   CommandId = (int)GameCommandId.MoveCommand,
                                   CommandData = MemoryPackHelper.Serialize(moveCommand)
                               });
    }
}