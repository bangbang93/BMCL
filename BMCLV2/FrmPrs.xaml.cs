using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BMCLV2
{
    /// <summary>
    /// FrmPrs.xaml 的交互逻辑
    /// </summary>
    public partial class FrmPrs : Window
    {
        public FrmPrs(string Name)
        {
            InitializeComponent();
            labName.Content = Name;
            launcher.changeEvent += changeEvent;
            FrmMain.changeEvent += changeEvent;
        }

        void changeEvent(string status)
        {
            labStatus.Content = status;
        }
    }
}
