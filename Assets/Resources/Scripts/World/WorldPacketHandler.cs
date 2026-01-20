using System;
using System.Collections.Generic;
using Resources.Scripts.Network;
using UnityEngine;

namespace Resources.Scripts.World
{
    public partial class WorldPacketHandler : TCPPacketHandler
    {
        private bool _isJoined = false;
        private readonly Dictionary<int, Action<byte[]>> _gameHandler = new();

        public NetworkSession GetNetworkSession() => _client as NetworkSession;
        public void Start()
        {
            DontDestroyOnLoad(gameObject);
            _client = new NetworkSession();
            
            GetNetworkSession().Connect("127.0.0.1", 28080, this);
        }
        
        protected override void _RegisterHandler()
        {
            base._RegisterHandler();
        
            _handler.Add((int)WorldServerKeys.ResponseWorldJoin, OnWorldJoin);
            _handler.Add((int)WorldServerKeys.GameCommandResponse, OnGameCommand);
        
            _gameHandler.Add((int)GameCommandId.MonsterUpdateCommand, _OnMonsterUpdateCommand);
            _gameHandler.Add((int)GameCommandId.UseItemCommand, _OnItemUseCommand);
            _gameHandler.Add((int)GameCommandId.SpawnGameObject, _OnSpawnGameObject);
        }

        public void Update()
        {
            var networkSession = GetNetworkSession();
            if(networkSession == null)
                return;
            
            var tcpNetwork = networkSession.GetTcpNetwork();
            if (tcpNetwork == null)
                return;
            
            while (tcpNetwork.TryDequeue(out var package))
            {
                var handler = _GetHandler(package.Key);
                if (handler == null)
                {
                    Debug.Log($"[NET] Not Register message Id : {package.Key}");
                    continue;
                }
                
                handler(package);
            }
        }

        protected override void OnConnected(NetworkPackage obj)
        {
            GetNetworkSession().SendWorldJoinCommand();
        }
        
        private void OnWorldJoin(NetworkPackage obj)
        {
            var receivedPackage = MemoryPackHelper.Deserialize<WorldJoinCommandResponse>(obj.Body);
            if(receivedPackage == null)
                return;
        
            _isJoined = true;
            UnityEngine.Debug.Log($"Received World Join Response: RoomId={receivedPackage.RoomId}");
        }
        
        private void OnGameCommand(NetworkPackage obj)
        {
            var receivedPackage = MemoryPackHelper.Deserialize<GameCommandResponse>(obj.Body);
            if(receivedPackage == null)
                return;

            if (_gameHandler.TryGetValue(receivedPackage.CommandId, out var handler) == false)
                return;
        
            handler(receivedPackage.CommandData);
        }

        public void OnDestroy()
        {
            if (GetNetworkSession() == null)
                return;

            GetNetworkSession().Dispose();
        }
    }
}