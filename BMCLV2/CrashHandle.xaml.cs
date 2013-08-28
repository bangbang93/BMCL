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
            txtMessage.Text = "BMCL," + FrmMain.ver + "\n";
            txtMessage.Text += ex.Message;
            txtMessage.Text += "\n" + ex.StackTrace;
            var iex = ex;
            while (iex.InnerException != null)
            {
                iex = iex.InnerException;
                txtMessage.Text += "\n" + iex.Message;
                txtMessage.Text += "\n" + ex.StackTrace;
            }
        }

        private void btnMyWeb_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "http://www.bangbang93.com/forum-bmcl-1.html";
            p.Start();
            Copy();
        }

        private void btnMcbbs_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "http://www.mcbbs.net/thread-137254-1-1.html";
            p.Start();
            Copy();
        }

        private void btnWeibo_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "http://weibo.com/bangbang93";
            p.Start();
            Copy();
        }

        private void btnTwitter_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "https://twitter.com/bangbangpal";
            p.Start();
            Copy();
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "mailto:bangbang93@163.com?subject=" + HttpUtility.UrlEncode("BMCL崩溃报告") + "&body=" + HttpUtility.UrlEncode(txtMessage.Text);
            p.Start();
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
