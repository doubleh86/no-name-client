using System;

namespace Resources.Scripts.Network
{
    public class NetworkSession : ITCPClient, IDisposable
    {
        private TCPNetworkWrapper _tcpNetwork;
        public bool IsConnected => _tcpNetwork.GetTcpSession().IsConnected();
        
        public TCPNetworkWrapper GetTcpNetwork() => _tcpNetwork;
    
        public void Connect(string ip, int port, TCPPacketHandler networkHandler)
        {
            _tcpNetwork = new TCPNetworkWrapper(ip, port, 0, networkHandler);
            _tcpNetwork.ConnectServer();
        }
    
        public void SendGameCommand(GameCommandRequest command)
        {
            var sendBuffer = TCPNetworkHelper.MakePackage((int)WorldServerKeys.GameCommandRequest, MemoryPackHelper.Serialize(command));
            _tcpNetwork.GetTcpSession().Send(sendBuffer);
        }

        public void SendWorldJoinCommand()
        {
            var package = new WorldJoinCommandRequest
                          {
                              Identifier = 100
                          };

            var sendBuffer = TCPNetworkHelper.MakePackage((int)WorldServerKeys.RequestWorldJoin, MemoryPackHelper.Serialize(package));
            _tcpNetwork.GetTcpSession().Send(sendBuffer);
        }
        
        

        public void Dispose()
        {
            _tcpNetwork?.Disconnect();
        }
    }
}