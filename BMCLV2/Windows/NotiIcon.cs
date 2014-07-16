using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using BMCLV2.Lang;

namespace BMCLV2.Windows
{
    public  class NotiIcon
    {
        public readonly NotifyIcon NIcon = new NotifyIcon();
        private readonly ContextMenu _nMenu = new ContextMenu();
        private Window _mainWindow;
        public Window MainWindow
        {
            private get { return this._mainWindow; }
            set
            {
                this._mainWindow = value;
                var menuItems = _nMenu.MenuItems.Find("ShowMainWindow", false);
                if (menuItems.Length != 0)
                {
                    menuItems[0].Enabled = true;
                }
            }
        }

        public NotiIcon()
        {
            NIcon.Visible = false;
            NIcon = new NotifyIcon { Visible = true };
            var s = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/screenLaunch.png"));
            if (s != null) this.NIcon.Icon = System.Drawing.Icon.FromHandle(new System.Drawing.Bitmap(s.Stream).GetHicon());
            MenuItem menuItem = _nMenu.MenuItems.Add(LangManager.GetLangFromResource("MenuShowMainWindow"));
            menuItem.Name = "ShowMainWindow";
            menuItem.DefaultItem = true;
            menuItem.Enabled = false;
            menuItem.Click += NMenu_ShowMainWindows_Click;
            NIcon.DoubleClick += NIcon_DoubleClick;
            MenuItem debugMode = _nMenu.MenuItems.Add(LangManager.GetLangFromResource("MenuUseDebugMode"));
            debugMode.Name = "DebugMode";
            debugMode.Click += DebugMode_Click;
            NIcon.ContextMenu = _nMenu;
        }

        public void ShowBalloonTip(int time, string message, ToolTipIcon toolTipIcon = ToolTipIcon.Info)
        {
            this.NIcon.ShowBalloonTip(time, "BMCL", message, toolTipIcon);
        }

        public void Hide()
        {
            this.NIcon.Visible = false;
        }

        public void Show()
        {
            this.NIcon.Visible = true;
        }

        private void NIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.MainWindow != null)
            {
                MainWindow.Show();
            }
        }

        private void NMenu_ShowMainWindows_Click(object sender, EventArgs e)
        {
            if (this.MainWindow != null)
            {
                MainWindow.Show();
            }
        }

        void DebugMode_Click(object sender, EventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName, "-Debug");
            BmclCore.NIcon.Hide();
            Environment.Exit(0);
        }
    }
}
