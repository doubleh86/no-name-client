using System;
using System.Collections.Generic;
using UnityEngine;

public class TCPPacketHandler : MonoBehaviour
{
    protected ITCPClient _client;
    protected readonly Dictionary<int, Action<NetworkPackage>> _handler = new();
    
    public virtual void Initialized()
    {
        _RegisterHandler();
    }

    protected virtual void _RegisterHandler()
    {
        _handler.Add(ConnectedCommand.ConnectedCommandId, OnConnected);
        _handler.Add(DisconnectedCommand.DisconnectedCommandId, OnDisconnected);
    }

    protected virtual Action<NetworkPackage> _GetHandler(int messageId)
    {
        return _handler.GetValueOrDefault(messageId);
    }

    protected virtual void OnConnected(NetworkPackage package)
    {
        
    }

    protected virtual void OnDisconnected(NetworkPackage package)
    {
        
    }
}
