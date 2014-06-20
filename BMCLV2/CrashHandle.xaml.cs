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
using System.Diagnostics;
using System.Web;
using System.IO;
using System.Collections;

namespace BMCLV2
{
    /// <summary>
    /// CrashHandle.xaml 的交互逻辑
    /// </summary>
    public partial class CrashHandle : Window
    {
        public CrashHandle(Exception ex)
        {
            InitializeComponent();
            StringBuilder Message = new StringBuilder();
            Message.AppendLine("BMCL," + BmclCore.bmclVersion);
            Message.AppendLine(ex.Source);
            Message.AppendLine(ex.ToString());
            Message.AppendLine(ex.Message);
            foreach (DictionaryEntry data in ex.Data)
                Message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
            Message.AppendLine(ex.StackTrace);
            var iex = ex;
            while (iex.InnerException != null)
            {
                Message.AppendLine("------------------------");
                iex = iex.InnerException;
                Message.AppendLine(iex.Source);
                Message.AppendLine(iex.ToString());
                Message.AppendLine(iex.Message);
                foreach (DictionaryEntry data in ex.Data)
                    Message.AppendLine(string.Format("Key:{0}\nValue:{1}", data.Key, data.Value));
                Message.AppendLine(iex.StackTrace);
            }
            Message.AppendLine("\n\n-----------------BMCL LOG----------------------\n");
            StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "bmcl.log");
            Message.AppendLine(sr.ReadToEnd());
            sr.Close();
            txtMessage.Text = Message.ToString();
        }

        private void btnMyWeb_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.bangbang93.com/forum-bmcl-1.html");
            Copy();
        }

        private void btnMcbbs_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.mcbbs.net/thread-137254-1-1.html");
            Copy();
        }

        private void btnWeibo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://weibo.com/bangbang93");
            Copy();
        }

        private void btnTwitter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://twitter.com/bangbangpal");
            Copy();
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mailto:bangbang93@163.com?subject=" + HttpUtility.UrlEncode("BMCL崩溃报告") + "&body=" + HttpUtility.UrlEncode(txtMessage.Text));
            Copy();
        }

        private void labTip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            try
            {
                Clipboard.SetText(txtMessage.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("自动复制失败，请手动复制\n" + ex.Message);
            }
        }
    }
}
