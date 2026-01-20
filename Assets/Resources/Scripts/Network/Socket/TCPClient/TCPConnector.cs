using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class TCPConnector : IDisposable
{
    public delegate void ConnectHandler(ITCPSession session);
        
    private Socket _socket;
    private TaskCompletionSource<bool> _connectCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ConnectHandler ConnectionCompleteHandler;
        
    public async Task<bool> Connect(string ip, int port, int timeout, ulong accountId)
    {
        if (false == IPAddress.TryParse(ip, out var address))
        {
            try
            {
                address = (await Dns.GetHostEntryAsync(ip)).AddressList[0];
            }
            catch
            {
                return false;
            }
        }

        _connectCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        var tcpSession = new TCPSessionV2(_socket);
        tcpSession.Identifier = accountId;

        _socket.BeginConnect(ip, port, ConnectComplete, tcpSession);
        if (timeout <= 0)
        {
            return await _connectCompletionSource.Task.ConfigureAwait(false);
        }
        
        var completeTask = await Task.WhenAny(_connectCompletionSource.Task, Task.Delay(timeout))
                                     .ConfigureAwait(false);

        if (completeTask != _connectCompletionSource.Task)
        {
            try
            {
                _socket?.Close();
            }
            catch (Exception e)
            {
                // ignored
            }

            return false;
        }
        
        return await _connectCompletionSource.Task.ConfigureAwait(false);
            
    }

    private void ConnectComplete(IAsyncResult ar)
    {
        var tcpSession = ar.AsyncState as ITCPSession;
        if (tcpSession == null)
        {
            _connectCompletionSource.TrySetResult(false);
            return;
        }
 
        try
        {
            tcpSession.GetSocket().EndConnect(ar);
            if (tcpSession.GetSocket().Connected == false)
            {
                throw new Exception("Socket is not connected");
            }
            
            ConnectionCompleteHandler?.Invoke(tcpSession);
            _connectCompletionSource.TrySetResult(true);
        }
        catch (SocketException e)
        {
            Console.WriteLine($"[ConnectComplete][{e.ErrorCode}][{e.Message}]");
            tcpSession.Disconnect(SessionCloseReason.ServerShutdown);
            tcpSession.Dispose();
            
            _connectCompletionSource.TrySetResult(false);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ConnectComplete][{e.Message}]");
            tcpSession.Disconnect(SessionCloseReason.Unknown);
            tcpSession.Dispose();
            
            _connectCompletionSource.TrySetResult(false);
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _connectCompletionSource.TrySetCanceled();
    }
}