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
            txtMessage.Text += "\n\nBMCL LOG\n";
            StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "bmcl.log");
            txtMessage.Text += sr.ReadToEnd();
            sr.Close();
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
