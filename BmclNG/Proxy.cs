using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;

namespace BmclNG
{
    public class Proxy
    {
        private Socket clientSocket;
        public Proxy(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        public void Run()
        {
            var recv = new SocketAsyncEventArgs();
            recv.Completed += Recv_Completed;
            recv.UserToken = clientSocket;
            clientSocket.ReceiveAsync(recv);
        }

        private void Recv_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                var buffer = e.Buffer;
                var str = Encoding.UTF8.GetString(buffer);
                var input = str.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}