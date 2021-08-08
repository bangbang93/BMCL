﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using BMCLV2.I18N;
using Application = System.Windows.Application;

namespace BMCLV2.Windows
{
    public  class NotiIcon
    {
        public readonly NotifyIcon NIcon = new();
        private readonly ContextMenuStrip _nMenu = new();
        private Window _mainWindow;
        public Window MainWindow
        {
            private get { return this._mainWindow; }
            set
            {
                this._mainWindow = value;
                var menuItems = _nMenu.Items.Find("ShowMainWindow", false);
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
            var s = Application.GetResourceStream(new Uri("pack://application:,,,/screenLaunch.png"));
            if (s != null) this.NIcon.Icon = Icon.FromHandle(new Bitmap(s.Stream).GetHicon());
            var menuItem = _nMenu.Items.Add(LangManager.GetLangFromResource("MenuShowMainWindow"));
            menuItem.Name = "ShowMainWindow";
            menuItem.Enabled = false;
            menuItem.Click += NMenu_ShowMainWindows_Click;
            NIcon.DoubleClick += NIcon_DoubleClick;
            var debugMode = _nMenu.Items.Add(LangManager.GetLangFromResource("MenuUseDebugMode"));
            debugMode.Name = "DebugMode";
            debugMode.Click += DebugMode_Click;
            NIcon.ContextMenuStrip = _nMenu;
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
