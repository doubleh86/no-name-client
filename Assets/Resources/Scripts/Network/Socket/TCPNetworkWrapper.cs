using System;
using System.Collections.Concurrent;
using System.Threading;

public class TCPNetworkWrapper
{
    private ITCPSession _tcpSession;
    private Thread _receiveThread;
    private CancellationTokenSource _cancellationTokenSource;

    private readonly ulong _accountId;
    private readonly TCPPacketHandler _packetReceiveHandler;
    private readonly string _serverIp;
    private readonly int _serverPort;

    private readonly ConcurrentQueue<NetworkPackage> _dispatchQueue = new();
    public bool TryDequeue(out NetworkPackage pkg) => _dispatchQueue.TryDequeue(out pkg);

    public ITCPSession GetTcpSession() => _tcpSession;
    
        
    public TCPNetworkWrapper(string serverIp, int serverPort, ulong accountId, TCPPacketHandler packetReceiveHandler)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _accountId = accountId;

        _packetReceiveHandler = packetReceiveHandler;
        _packetReceiveHandler.Initialized();
    }

    public void ConnectServer()
    {
        if (_tcpSession != null && _tcpSession.IsConnected() == true)
            return;

        if (_tcpSession != null)
        {
            _tcpSession.Disconnect(SessionCloseReason.Reconnecting);
            _tcpSession.Dispose();    
        }
        
        var tcpConnector = new TCPConnector();
        tcpConnector.ConnectionCompleteHandler = session =>
        {
            if (session.Identifier != _accountId)
            {
                Console.WriteLine($"Account Id is not same [{session.Identifier}][{_accountId}]");
                return;
            }
                
            _tcpSession = session;
            _tcpSession.Identifier = _accountId;
            _tcpSession.ConnectComplete();
        };
            
        _ = tcpConnector.Connect(_serverIp, _serverPort, 0, _accountId);

        _cancellationTokenSource = new CancellationTokenSource();
        
        _receiveThread = new Thread(_ReceiveThread);
        _receiveThread.Start();
            
        Console.WriteLine($"[{_receiveThread.ManagedThreadId}]");

    }

    private void _ReceiveThread()
    {
        try
        {
            while (_cancellationTokenSource.Token.IsCancellationRequested == false)
            {
                if (_tcpSession == null || _tcpSession.IsConnected() == false)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (_tcpSession.HasData() == false)
                {
                    Thread.Sleep(10);
                    continue;
                }
    
                var receiveDataList = _tcpSession.GetQueueData();
                foreach (var data in receiveDataList)
                {
                    if (data.Key == PongCommand.PongCommandId)
                    {
                        OnPong(data);
                        continue;
                    }
                
                    _dispatchQueue.Enqueue(data);
                }
                
                Thread.Sleep(1);
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the CancellationToken is cancelled during Disconnect
            UnityEngine.Debug.Log("Network receive thread cancelled gracefully.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Unexpected error in network thread: {ex.Message}");
        }
        
    }

    private void OnPong(NetworkPackage data)
    {
        _tcpSession.ReceivePong();
    }

    public void Disconnect()
    {
        _tcpSession?.Disconnect(SessionCloseReason.Disconnecting);
        
        _cancellationTokenSource.Cancel();
        _receiveThread?.Join();
    }
}