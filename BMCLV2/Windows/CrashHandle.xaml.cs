using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace BMCLV2.Windows
{
    /// <summary>
    /// CrashHandle.xaml 的交互逻辑
    /// </summary>
    public partial class CrashHandle
    {
        public CrashHandle(Exception ex)
        {
            InitializeComponent();
            var message = new StringBuilder();
            message.AppendLine("BMCL," + BmclCore.BmclVersion);
            message.AppendLine(ex.Source);
            message.AppendLine(ex.ToString());
            message.AppendLine(ex.Message);
            foreach (DictionaryEntry data in ex.Data)
                message.AppendLine($"Key:{data.Key}\nValue:{data.Value}");
            message.AppendLine(ex.StackTrace);
            var iex = ex;
            while (iex.InnerException != null)
            {
                message.AppendLine("------------------------");
                iex = iex.InnerException;
                message.AppendLine(iex.Source);
                message.AppendLine(iex.ToString());
                message.AppendLine(iex.Message);
                foreach (DictionaryEntry data in ex.Data)
                    message.AppendLine($"Key:{data.Key}\nValue:{data.Value}");
                message.AppendLine(iex.StackTrace);
            }
            message.AppendLine("\n\n-----------------BMCL LOG----------------------\n");
            var sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "bmcl.log");
            message.AppendLine(sr.ReadToEnd());
            sr.Close();
            TxtMessage.Text = message.ToString();
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
            Process.Start("mailto:bangbang93@163.com?subject=" + HttpUtility.UrlEncode("BMCL崩溃报告") + "&body=" + HttpUtility.UrlEncode(TxtMessage.Text));
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
                Clipboard.SetText(TxtMessage.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("自动复制失败，请手动复制\n" + ex.Message);
            }
        }
    }
}
