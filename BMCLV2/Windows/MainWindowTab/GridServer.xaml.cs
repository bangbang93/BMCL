using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using BMCLV2.Lang;
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
                    DateTime start = DateTime.Now;
                    var server = new object[7];
                    server[0] = info.Name;
                    server[1] = info.IsHide ? LangManager.GetLangFromResource("ServerListYes") : LangManager.GetLangFromResource("ServerListNo");
                    if (info.IsHide)
                        server[2] = string.Empty;
                    else
                        server[2] = info.Address;
                    try
                    {
                        var con = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            ReceiveTimeout = 3000,
                            SendTimeout = 3000
                        };
                        con.Connect(Dns.GetHostAddresses(info.Address.Split(':')[0]),
                            info.Address.Split(':').Length == 1 ? 25565 : int.Parse(info.Address.Split(':')[1]));
                        con.Send(new byte[] { 254 });
                        con.Send(new byte[] { 1 });
                        var recive = new byte[512];
                        int bytes = con.Receive(recive);
                        if (recive[0] != 255)
                        {
                            throw new Exception(LangManager.GetLangFromResource("ServerListInvildReply"));
                        }
                        string message = Encoding.UTF8.GetString(recive, 4, bytes - 4);
                        var remessage = new StringBuilder(30);
                        for (int i = 0; i <= message.Length; i += 2)
                        {
                            remessage.Append(message[i]);
                        }
                        message = remessage.ToString();
                        con.Shutdown(SocketShutdown.Both);
                        con.Close();
                        DateTime end = DateTime.Now;
                        char[] achar = message.ToCharArray();

                        for (int i = 0; i < achar.Length; ++i)
                        {
                            if (achar[i] != 167 && achar[i] != 0 && char.IsControl(achar[i]))
                            {
                                achar[i] = (char)63;
                            }
                        }
                        message = new String(achar);
                        if (message[0] == (char)253 || message[0] == (char)65533)
                        {
                            message = (char)167 + message.Substring(1);
                        }
                        if (message.StartsWith("\u00a7") && message.Length > 1)
                        {
                            string[] astring = message.Substring(1).Split('\0');
                            if (MathHelper.parseIntWithDefault(astring[0], 0) == 1)
                            {
                                server[3] = astring[3];
                                server[4] = astring[2];
                                int online = MathHelper.parseIntWithDefault(astring[4], 0);
                                int maxplayer = MathHelper.parseIntWithDefault(astring[5], 0);
                                server[5] = online + "/" + maxplayer;
                            }
                        }
                        else
                        {
                            server[3] = " ";
                            server[4] = " ";
                            server[5] = " ";
                        }
                        server[6] = (end - start).Milliseconds + " ms";
                    }
                    catch (SocketException ex)
                    {
                        server[3] = " ";
                        server[4] = " ";
                        server[5] = " ";
                        server[6] = LangManager.GetLangFromResource("ServerListSocketException") + ex.Message;
                        //server.SubItems[0].ForeColor = Color.Red;
                    }
                    catch (Exception ex)
                    {
                        server[3] = " ";
                        server[4] = " ";
                        server[5] = " ";
                        server[6] = LangManager.GetLangFromResource("ServerListUnknowServer") + ex.Message;
                        //server.SubItems[0].ForeColor = Color.Red;
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
