using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Xml;
using System.Data;
using System.Threading;

namespace BMCLV2
{
    /// <summary>
    /// FrmCheckRes.xaml 的交互逻辑
    /// </summary>
    public partial class FrmCheckRes : Window
    {
        
        public FrmCheckRes()
        {
            InitializeComponent();
        }
        string URL_RESOURCE_BASE = Resource.Url.URL_RESOURCE_BASE;
        DataTable dt=new DataTable();
        int InDownloading = 0;
        int WaitingForSync = 0;
        private void frmCheckRes_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                dt.Columns.Add("文件名");
                dt.Columns.Add("修改时间");
                dt.Columns.Add("大小");
                dt.Columns.Add("状态");
                dt.Columns.Add("MD5");
                byte[] buffer = (new WebClient()).DownloadData(URL_RESOURCE_BASE);
                Stream RawXml = new MemoryStream(buffer);
                XmlDocument doc = new XmlDocument();
                doc.Load(RawXml);
                XmlNodeList nodeLst = doc.GetElementsByTagName("Contents");
                for (int i = 0; i < nodeLst.Count; i++)
                {
                    XmlNode node = nodeLst.Item(i);
                    if (node.GetType() == null)
                        continue;
                    XmlElement element = (XmlElement)node;
                    String key = element.GetElementsByTagName("Key").Item(0).ChildNodes.Item(0).Value;
                    String modtime = element.GetElementsByTagName("LastModified").Item(0).ChildNodes.Item(0).Value;
                    String etag = element.GetElementsByTagName("ETag") == null ? "-" : element.GetElementsByTagName("ETag").Item(0).ChildNodes.Item(0).Value;
                    long size = long.Parse(element.GetElementsByTagName("Size").Item(0).ChildNodes.Item(0).Value);
                    if (size <= 0L)
                        continue;
                    dt.Rows.Add(new string[] { key, modtime, size.ToString(), "待检查", etag.Replace("\"", "").Trim() });
                    listRes.DataContext = dt;
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("与文件服务器通信超时，请重试");
                this.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            prs.Maximum = listRes.Items.Count;
            prs.Value = 0;
            foreach (object item in listRes.Items)
            {
                prs.Value++;
                ThreadPool.QueueUserWorkItem(new WaitCallback(GetMD5HashFromFile), prs.Value);
                //GetMD5HashFromFile(prs.Value);
            }
        }
        public void GetMD5HashFromFile(object obj)
        {
            int num = (int)(double)obj - 1;
            string fileName = Environment.CurrentDirectory + @"\.minecraft\assets\" + dt.Rows[num]["文件名"].ToString();
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
                if (lmd5.Trim() == dt.Rows[num]["MD5"].ToString())
                {
                    lock (dt)
                    {
                        dt.Rows[num]["状态"] = "已完成";
                    }
                }
                else
                {
                    lock (dt)
                    {
                        dt.Rows[num]["状态"] = "待同步";
                    }
                    WaitingForSync++;
                }
            }
            catch (Exception ex)
            {
                lock (dt)
                {
                    dt.Rows[num]["状态"] = "待同步";
                }
                WaitingForSync++;
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            prs.Maximum = WaitingForSync;
            prs.Value = 0;
            int num = -1;
            foreach (object item in listRes.Items)
            {
                num++;
                if (dt.Rows[num]["状态"].ToString() == "待同步")
                {
                    WebClient downer = new WebClient();
                    StringBuilder rpath = new StringBuilder(FrmMain.URL_RESOURCE_BASE);
                    StringBuilder lpath = new StringBuilder(Environment.CurrentDirectory + @"\.minecraft\assets\");
                    rpath.Append(dt.Rows[num]["文件名"].ToString());
                    lpath.Append(dt.Rows[num]["文件名"].ToString());
                    if (!Directory.Exists(System.IO.Path.GetDirectoryName(lpath.ToString())))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(lpath.ToString()));
                    }
                    downer.DownloadFileCompleted += downer_DownloadFileCompleted;
                    InDownloading++;
                    downer.DownloadFileAsync(new Uri(rpath.ToString()), lpath.ToString(), num);
                }
            }
        }

        void downer_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            InDownloading--;
            int num = (int)e.UserState;
            lock (dt)
            {
                dt.Rows[num]["状态"] = "已同步";
            }
            prs.Value++;
            if (InDownloading == 0)
            {
                MessageBox.Show("同步完成");
                this.Close();
            }
        }
    }
}
