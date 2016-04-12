using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BmclNG.Thread
{
    public class Listener
    {
        private Socket server;
        public void Run()
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var port = FreePort.FindNextAvailableTcpPort(20000);
            var endPoint = new IPEndPoint(IPAddress.Any, port);
            server.Bind(endPoint);
            server.Listen(port);
            var accept = new SocketAsyncEventArgs();
            accept.Completed += Accept_Completed;
            accept.RemoteEndPoint = endPoint;
            accept.UserToken = server;
            server.AcceptAsync(accept);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Accept)
            {
                var proxy = new Proxy(e.AcceptSocket);
                var proxyThread = new System.Threading.Thread(proxy.Run);
            }
        }
    }
}