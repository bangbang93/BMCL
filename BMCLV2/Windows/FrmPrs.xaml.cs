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
        public delegate void changeHandel(string status);

        public FrmPrs(string Name)
        {
            InitializeComponent();
            labName.Content = Name;
        }

        public void ChangeEventH(string status)
        {
            Dispatcher.Invoke(new System.Windows.Forms.MethodInvoker(delegate { 
                labStatus.Content = status;
                Logger.log(status);
            }));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch { }
        }
    }
}
