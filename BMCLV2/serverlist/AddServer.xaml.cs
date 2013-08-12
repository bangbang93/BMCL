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

namespace BMCLV2.serverlist
{
    /// <summary>
    /// AddServer.xaml 的交互逻辑
    /// </summary>
    public partial class AddServer : Window
    {
        public AddServer(ref serverlist list)
        {
            InitializeComponent();
            this.list = list;
        }
        serverlist list;
        serverinfo ServerInfo;
        int num = -1;

        public AddServer(ref serverlist list, int num)
        {
            InitializeComponent();
            this.list = list;
            txtServerName.Text = list.info[num].Name;
            txtAddress.Text = list.info[num].Address;
            checkIsHide.IsChecked = list.info[num].IsHide;
            this.num = num;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (num == -1)
            {
                if (txtServerName.Text.Trim() == string.Empty || txtServerName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("输入有误，请检查");
                    return;
                }
                list.Add(txtServerName.Text, txtAddress.Text, checkIsHide.IsChecked.Value);
            }
            else
            {
                if (txtServerName.Text.Trim() == string.Empty || txtServerName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("输入有误，请检查");
                    return;
                }
                list.info[num] = new serverinfo(txtServerName.Text, checkIsHide.IsChecked.Value, txtAddress.Text);
            }
            ServerInfo = new serverinfo(txtServerName.Text, checkIsHide.IsChecked.Value, txtAddress.Text);
        }

        public serverinfo getEdit()
        {
            return ServerInfo;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (num == -1)
            {
                if (txtServerName.Text.Trim() == string.Empty || txtServerName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("输入有误，请检查");
                    return;
                }
                list.Add(txtServerName.Text, txtAddress.Text, checkIsHide.IsChecked.Value);
            }
            else
            {
                if (txtServerName.Text.Trim() == string.Empty || txtServerName.Text.Trim() == string.Empty)
                {
                    MessageBox.Show("输入有误，请检查");
                    return;
                }
                list.info[num] = new serverinfo(txtServerName.Text, checkIsHide.IsChecked.Value, txtAddress.Text);
            }
            ServerInfo = new serverinfo(txtServerName.Text, checkIsHide.IsChecked.Value, txtAddress.Text);
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
