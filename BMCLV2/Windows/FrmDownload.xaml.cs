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
using System.Collections;
using System.Net;
using System.Threading;
using System.IO;

using BMCLV2.Lang;

namespace BMCLV2
{
    /// <summary>
    /// FrmDownload.xaml 的交互逻辑
    /// </summary>
    public partial class FrmDownload : Window
    {
        Hashtable FileTable;
        int FinishFileCount;
        public FrmDownload(Hashtable FileTable)
        {
            InitializeComponent();
            this.FileTable = FileTable;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labCount.Content = FinishFileCount + "/" + FileTable.Count;
            prsCount.Maximum = FileTable.Count;
            foreach (DictionaryEntry DownloadFile in FileTable)
            {
                ThreadPool.QueueUserWorkItem(a => this.DownloadFile(DownloadFile.Key as string, DownloadFile.Value as string));
            }
        }

        private void DownloadFile(string Url,string FileName)
        {
            WebClient wc = new WebClient();
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(FileName)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FileName));
            
            wc.DownloadFileAsync(new Uri(Url), FileName,FileName);
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
        }

        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string FileName = e.UserState as string;
            if (e.Error != null)
            {
                Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { listLog.Items.Add(FileName + e.Error.Message); }));
                Logger.log(e.Error);
            }
            FinishFileCount++;
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate
            {
                listLog.Items.Add(FileName + LangManager.GetLangFromResource("DownloadFinish"));
                listLog.ScrollIntoView(listLog.Items[listLog.Items.Count - 1]);
                labCount.Content = FinishFileCount + "/" + FileTable.Count;
                prsCount.Value = FinishFileCount;
                if (FinishFileCount == FileTable.Count)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("DownloadFinish"));
                    Logger.log("下载文件完毕");
                    this.Close();
                }
            }));
        }
    }
}
