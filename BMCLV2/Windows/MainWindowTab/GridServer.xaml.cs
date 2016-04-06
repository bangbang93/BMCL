using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using BMCLV2.I18N;
using BMCLV2.util;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridServer.xaml 的交互逻辑
    /// </summary>
    public partial class GridServer
    {
        public GridServer()
        {
            InitializeComponent();
        }

        readonly DataTable _serverListDataTable = new DataTable();
        private serverlist.serverlist _sl;
        private void btnReflushServer_Click(object sender, RoutedEventArgs e)
        {
            _serverListDataTable.Clear();
            _serverListDataTable.Columns.Clear();
            _serverListDataTable.Rows.Clear();
            _serverListDataTable.Columns.Add("ServerName");
            _serverListDataTable.Columns.Add("HiddenAddress");
            _serverListDataTable.Columns.Add("ServerAddress");
            _serverListDataTable.Columns.Add("ServerMotd");
            _serverListDataTable.Columns.Add("ServerVer");
            _serverListDataTable.Columns.Add("ServerOnline");
            _serverListDataTable.Columns.Add("ServerDelay");
            this.listServer.DataContext = _serverListDataTable;
            this.btnReflushServer.IsEnabled = false;
            ThreadPool.QueueUserWorkItem(GetServerInfo);
        }

        private void GetServerInfo(object obj)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnReflushServer.Content = LangManager.GetLangFromResource("ServerListGetting"); }));
            if (File.Exists(@".minecraft\servers.dat"))
            {
                _sl = new serverlist.serverlist();
                foreach (serverlist.serverinfo info in _sl.info)
                {
                    var start = DateTime.Now;
                    var server = new object[7];
                    server[0] = info.Name;
                    server[1] = info.IsHide ? LangManager.GetLangFromResource("ServerListYes") : LangManager.GetLangFromResource("ServerListNo");
                    if (info.IsHide)
                        server[2] = LangManager.GetLangFromResource("btnMiniSize");//要的只是这两个字
                    else
                        server[2] = info.Address;
                    server[3] = " ";
                    server[4] = " ";
                    server[5] = " ";
                    try
                    {
                        var bytes = 512;
                        var recive = new byte[bytes];

                        using (var con = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000, SendTimeout = 3000 })
                        {
                            con.Connect(Dns.GetHostAddresses(info.Address.Split(':')[0] == "0" ? "127.0.0.1" : info.Address.Split(':')[0]),
                                info.Address.Split(':').Length == 1 ? 25565 : int.Parse(info.Address.Split(':')[1]));
                            con.Send(new byte[] { 254 });
                            con.Send(new byte[] { 1 });
                            recive = new byte[512];
                            bytes = con.Receive(recive);
                            con.Shutdown(SocketShutdown.Both);
                            con.Close();
                        }

                        if (recive[0] != 255)
                        {
                            throw new Exception(LangManager.GetLangFromResource("ServerListInvildReply"));
                        }

                        var unirecive = new System.Collections.Generic.List<byte>();
                        for (var index = 1; index < bytes; index += 2)
                        {
                            unirecive.Add(recive[index + 1]);
                            unirecive.Add(recive[index]);
                        }
                        var message = Encoding.Unicode.GetString(unirecive.ToArray());

                        var end = DateTime.Now;
                        //Logger.info(message);
                        var astring = message.Split('\u00a7');
                        if (astring.Length == 3)
                        {
                            server[3] = astring[0].IndexOf('\r') == 0 ? astring[0].Substring(1) : astring[0];
                            server[4] = LangManager.GetLangFromResource("Unknown");//未知
                            server[5] = astring[1] + "/" + astring[2];
                        }
                        if (astring.Length == 2)
                        {
                            astring = astring[1].Split('\0');
                            server[3] = astring[3];
                            server[4] = astring[2];
                            server[5] = astring[4] + "/" + astring[5];
                        }
                        server[6] = (end - start).Milliseconds + "ms";
                        server[3] = ((string)server[3]).Replace("  ", "").Replace(new String(new char[] { (char)0x1c }), "");
                    }
                    catch (SocketException ex)
                    {
                        server[6] = LangManager.GetLangFromResource("ServerListSocketException") + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        server[6] = LangManager.GetLangFromResource("ServerListUnknowServer") + ex.Message;
                    }
                    finally
                    {
                        var logger = new StringBuilder();
                        foreach (string str in server)
                        {
                            logger.Append(str + ",");
                        }
                        Logger.log(logger.ToString());
                        lock (_serverListDataTable)
                        {
                            _serverListDataTable.Rows.Add(server);
                            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                            {
                                listServer.DataContext = null;
                                listServer.DataContext = _serverListDataTable;
                            }));
                        }
                    }
                }
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { btnReflushServer.Content = LangManager.GetLangFromResource("btnReflushServer"); btnReflushServer.IsEnabled = true; }));
            }
            else
            {
                if (MessageBox.Show(LangManager.GetLangFromResource("ServerListNotFound"), "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (!Directory.Exists(".minecraft"))
                    {
                        Directory.CreateDirectory(".minecraft");
                    }
                    var serverdat = new FileStream(@".minecraft\servers.dat", FileMode.Create);
                    serverdat.Write(Convert.FromBase64String(Resource.ServerDat.Header), 0, Convert.FromBase64String(Resource.ServerDat.Header).Length);
                    serverdat.WriteByte(0);
                    serverdat.Close();
                    _sl = new serverlist.serverlist();
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnAddServer.IsEnabled = true;
                        btnDeleteServer.IsEnabled = true;
                        btnEditServer.IsEnabled = true;
                        btnReflushServer.IsEnabled = true;
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
                    {
                        btnAddServer.IsEnabled = false;
                        btnDeleteServer.IsEnabled = false;
                        btnEditServer.IsEnabled = false;
                        btnReflushServer.IsEnabled = false;
                    }));
                }
            }
        }

        private void btnAddServer_Click(object sender, RoutedEventArgs e)
        {
            var frmAdd = new serverlist.AddServer(ref _sl);
            if (frmAdd.ShowDialog() == true)
            {
                _sl.Write();
                btnReflushServer_Click(null, null);
            }
        }

        private void btnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _sl.Delete(listServer.SelectedIndex);
                _sl.Write();
                btnReflushServer_Click(null, null);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ServerListNoServerSelect"));
            }
        }

        private void btnEditServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int selected = this.listServer.SelectedIndex;
                var frmEdit = new serverlist.AddServer(ref _sl, selected);
                if (frmEdit.ShowDialog() == true)
                {
                    serverlist.serverinfo info = frmEdit.getEdit();
                    _sl.Edit(selected, info.Name, info.Address, info.IsHide);
                    _sl.Write();
                    btnReflushServer_Click(null, null);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ServerListNoServerSelect"));
            }
        }

        public void ReflushSever()
        {
            btnReflushServer_Click(null, null);
        }
    }
}
