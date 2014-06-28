using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Xml;
using BMCLV2.Lang;

namespace BMCLV2.Windows
{
    /// <summary>
    /// FrmCheckRes.xaml 的交互逻辑
    /// </summary>
    public partial class FrmCheckRes : Window
    {
        public bool AutoStart = false;
        public FrmCheckRes()
        {
            InitializeComponent();
        }

        private const string UrlResourceBase = "http://www.bangbang93.com/bmcl/resources/";
        readonly DataTable _dt = new DataTable();
        int _inDownloading;
        int _waitingForSync;
        bool _ischecked;
        int _checkedfile;
        delegate void GetInfoFinishEventHandle(DataTable dt);
        event GetInfoFinishEventHandle GetInfoFinishEvent;
        delegate void GetInfoFailedEventHandle();
        event GetInfoFailedEventHandle GetInfoFailedEvent;
        string _soundsJsonString;

        private void frmCheckRes_Loaded(object sender, RoutedEventArgs e)
        {
            GetInfoFinishEvent += FrmCheckRes_GetInfoFinishEvent;
            GetInfoFailedEvent += FrmCheckRes_GetInfoFailedEvent;
            var thGetInfo = new Thread(new ThreadStart(delegate
            {
                try
                {
                    _dt.Columns.Add("FileName");
                    _dt.Columns.Add("ModifyTime");
                    _dt.Columns.Add("Size");
                    _dt.Columns.Add("Status");
                    _dt.Columns.Add("MD5");
                    var client = new WebClient();
                    client.Headers.Add("User-Agent", "BMCL" + BmclCore.BmclVersion);
                    byte[] buffer = client.DownloadData(UrlResourceBase + "ResourcesList.xml");
                    Stream rawXml = new MemoryStream(buffer);
                    var doc = new XmlDocument();
                    doc.Load(rawXml);
                    XmlNodeList nodeLst = doc.GetElementsByTagName("Contents");
                    for (int i = 0; i < nodeLst.Count; i++)
                    {
                        XmlNode node = nodeLst.Item(i);
                        if (node.GetType() == null)
                            continue;
                        var element = (XmlElement)node;
                        String key = element.GetElementsByTagName("Key").Item(0).ChildNodes.Item(0).Value;
                        String modtime = element.GetElementsByTagName("LastModified").Item(0).ChildNodes.Item(0).Value;
                        String etag = element.GetElementsByTagName("ETag") == null ? "-" : element.GetElementsByTagName("ETag").Item(0).ChildNodes.Item(0).Value;
                        long size = long.Parse(element.GetElementsByTagName("Size").Item(0).ChildNodes.Item(0).Value);
                        if (size <= 0L)
                            continue;
                        _dt.Rows.Add(new object[] { key, modtime, size.ToString(CultureInfo.InvariantCulture), LangManager.GetLangFromResource("ResWaitingForCheck"), etag.Replace("\"", "").Trim() });
                    }
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { GetInfoFinishEvent(_dt); }));
                    _soundsJsonString = (new WebClient()).DownloadString(UrlResourceBase + "sounds.json");
                }
                catch (WebException ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ResServerTimeOut") + ex.Message);
                    Logger.log("与资源服务器通信出错");
                    Logger.log(ex);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { GetInfoFailedEvent(); }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ResServerTimeOut") + ex.Message);
                    Logger.log("与资源服务器通信出错");
                    Logger.log(ex);
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { GetInfoFailedEvent(); }));
                }
            }));
            thGetInfo.Start();
        }

        void FrmCheckRes_GetInfoFailedEvent()
        {
            this.Close();
        }

        void FrmCheckRes_GetInfoFinishEvent(DataTable dt)
        {
            listRes.DataContext = dt;
            if (AutoStart)
            {
                btnSync_Click(null, null);
            }
            DoubleAnimation da1 = new DoubleAnimation(gridGetting.ActualHeight, 0, TimeSpan.FromMilliseconds(200));
            DoubleAnimation da2 = new DoubleAnimation(gridGetting.ActualWidth, 0, TimeSpan.FromMilliseconds(200));
            gridGetting.BeginAnimation(Grid.HeightProperty, da1);
            gridGetting.BeginAnimation(Grid.WidthProperty, da2);
        }
        bool checking = false;
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (checking)
                return;
            checking = true;
            prs.Maximum = listRes.Items.Count;
            prs.Value = 0;
            _checkedfile = 0;
            int nextcheck = -1;
            foreach (object item in listRes.Items)
            {
                nextcheck++;
                ThreadPool.QueueUserWorkItem(new WaitCallback(GetMD5HashFromFile), nextcheck);
                //GetMD5HashFromFile(prs.Value);
            }
            Thread thCount = new Thread(new ThreadStart(new System.Windows.Forms.MethodInvoker(delegate
            {
                while (_checkedfile != _dt.Rows.Count) { }
                Logger.log(string.Format("检查资源文件，共有{0}个文件待同步，共计{1}个文件", _waitingForSync, _dt.Rows.Count));
                _ischecked = true;
            })));
            thCount.Start();
        }
        public void GetMD5HashFromFile(object obj)
        {
            int num = (int)obj;
            string fileName = AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets\" + _dt.Rows[num]["FileName"].ToString();
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                string lmd5 = sb.ToString();
                if (lmd5.Trim() == _dt.Rows[num]["MD5"].ToString())
                {
                    lock (_dt)
                    {
                        _dt.Rows[num]["Status"] = LangManager.GetLangFromResource("ResNoNeedForSync");
                        Logger.log(string.Format("检查资源文件{0}，无需同步", _dt.Rows[num]["FileName"]));
                    }
                }
                else
                {
                    lock (_dt)
                    {
                        _dt.Rows[num]["Status"] = LangManager.GetLangFromResource("ResWaitingForSync");
                        Logger.log(string.Format("检查资源文件{0}，需要同步，文件MD5{1}，目标MD5{2}", _dt.Rows[num]["FileName"], lmd5.Trim(), _dt.Rows[num]["MD5"]));
                    }
                    _waitingForSync++;
                }
            }
            catch (Exception ex)
            {
                lock (_dt)
                {
                    _dt.Rows[num]["Status"] = LangManager.GetLangFromResource("ResWaitingForSync");
                    Logger.log(string.Format("检查资源文件{0}，需要同步，由于{1}", _dt.Rows[num]["FileName"], ex.Message), Logger.LogType.Exception);
                }
                _waitingForSync++;
            }
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.Value++; }));
            _checkedfile++;
            if (_checkedfile == _dt.Rows.Count)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ResCheckComplete"));
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            if (!_ischecked)
            {
                if (!checking)
                    btnCheck_Click(null, null);
                MessageBox.Show(LangManager.GetLangFromResource("ResPlsWaitingForCheck"));
                return;
            }
            if (_waitingForSync == 0)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ResNoFileForSync"));
            }
            prs.Maximum = _waitingForSync;
            prs.Value = 0;
            int num = -1;
            this.btnSync.IsEnabled = false;
            foreach (object item in listRes.Items)
            {
                num++;
                if (_dt.Rows[num]["Status"].ToString() == LangManager.GetLangFromResource("ResWaitingForSync"))
                {
                    WebClient downer = new WebClient();
                    StringBuilder rpath = new StringBuilder(UrlResourceBase);
                    StringBuilder lpath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets\");
                    rpath.Append(_dt.Rows[num]["FileName"].ToString());
                    lpath.Append(_dt.Rows[num]["FileName"].ToString());
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(lpath.ToString())))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(lpath.ToString()));
                    }
                    downer.DownloadFileCompleted += downer_DownloadFileCompleted;
                    _inDownloading++;
                    int tnum = num;
                    ThreadPool.QueueUserWorkItem(a => Downer(new Uri(rpath.ToString()), lpath.ToString(), tnum, ref downer));
                }
            }
        }
        void Downer(Uri url, string lpath, int num, ref WebClient downer)
        {
            Logger.log(string.Format("开始下载第{0}个资源文件{1}", num, url.ToString()));
            downer.DownloadFileAsync(url, lpath.ToString(), num);
        }
        void downer_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _inDownloading--;
            int num = (int)e.UserState;
            if (e.Error != null)
            {
                Logger.log(string.Format("下载资源文件失败{0}，远程路径为{1}", _dt.Rows[num]["FileName"],(sender as WebClient).BaseAddress));
                Logger.log(e.Error);
            }
            else
            {
                lock (_dt)
                {
                    _dt.Rows[num]["Status"] = LangManager.GetLangFromResource("ResInSync");
                }
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { prs.Value++; }));
                Logger.log(string.Format("下载资源文件成功{0}，远程路径为{1}", _dt.Rows[num]["FileName"], (sender as WebClient).BaseAddress));
                if (_inDownloading == 0)
                {
                    Logger.log(string.Format("下载资源文件完毕"));
                    MessageBox.Show(LangManager.GetLangFromResource("ResFinish"));
                    Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { this.Close(); }));
                }
            }
        }

        private void btnNewMusic_Click(object sender, RoutedEventArgs e)
        {
            JavaScriptSerializer SoundsJsonSerizlizer = new JavaScriptSerializer();
            var sounds = SoundsJsonSerizlizer.Deserialize<Dictionary<string, Dictionary<string, object>>>(_soundsJsonString);
            Hashtable DownloadFile = new Hashtable();
            int FileCount=0;
            int DuplicateFileCount=0;
            int JsonDuplicateFileCount = 0;
            foreach (KeyValuePair<string, Dictionary<string, object>> SoundEntity in sounds)
            {
                switch (SoundEntity.Value["category"] as string)
                {
                    case "ambient":
                    case "weather":
                    case "player":
                    case "neutral":
                    case "hostile":
                    case "block":
                    case "master":
                        //arraylist
                        var SoundFile = SoundEntity.Value["sounds"] as ArrayList;
                        if (SoundFile==null) goto case "music";
                        foreach (string FileName in SoundFile)
                        {
                            FileCount++;
                            string Url = UrlResourceBase + "sounds/" + FileName + ".ogg";
                            string SoundName = AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets\sounds\" + FileName + ".ogg";
                            DataRow[] result = _dt.Select("FileName = " + "'sounds/" + FileName + ".ogg'");
                            if (result.Count() != 0)
                            {
                                DuplicateFileCount++;
                                continue;
                            }
                            if (DownloadFile.ContainsKey(Url))
                            {
                                JsonDuplicateFileCount++;
                                continue;
                            }
                            DownloadFile.Add(Url, SoundName);
                        }
                        break;
                    case "music":
                        var MusicFile = SoundEntity.Value["sounds"] as ArrayList;
                        foreach (Dictionary<string,object> music in MusicFile)
                        {
                            if (!music.ContainsKey("stream")) continue;
                            if ((bool)music["stream"] == false) continue;
                            FileCount++;
                            string Url = UrlResourceBase + "sounds/" + music["name"] + ".ogg";
                            string SoundName = AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\assets\sounds\" + music["name"] + ".ogg";
                            DataRow[] result = _dt.Select("FileName = " + "'sounds/" + music["name"] as string + ".ogg'");
                            if (result.Count() != 0)
                            {
                                DuplicateFileCount++;
                                continue;
                            }
                            if (DownloadFile.ContainsKey(Url))
                            {
                                JsonDuplicateFileCount++;
                                continue;
                            }
                            DownloadFile.Add(Url, SoundName);
                        }
                        break;
                    case "record":
                        var RecordFile = SoundEntity.Value["sounds"] as ArrayList;
                        if (RecordFile[0] is string)
                            goto case "master";
                        else
                            goto case "music";

                }
            }
            Logger.log(string.Format("共计{0}个文件，{1}个文件重复,{2}个文件json内部重复，{3}个文件待下载",FileCount,DuplicateFileCount,JsonDuplicateFileCount,DownloadFile.Count));
            FrmDownload frmDownload = new FrmDownload(DownloadFile);
            frmDownload.Show();
        }
    }
}
