using BmclNG.Thread;

namespace BmclNG
{
    public class BmclApiProxyServer
    {
        private System.Threading.Thread _serverThread;
        public BmclApiProxyServer()
        {
            _serverThread = new System.Threading.Thread(new Listener().Run);
        }
    }
}
