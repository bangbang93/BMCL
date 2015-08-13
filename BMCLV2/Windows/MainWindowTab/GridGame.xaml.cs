using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BMCLV2.Lang;
using BMCLV2.Mod;
using BMCLV2.Versions;

namespace BMCLV2.Windows.MainWindowTab
{
    /// <summary>
    /// GridGame.xaml 的交互逻辑
    /// </summary>
    public partial class GridGame
    {
        public GridGame()
        {
            InitializeComponent();
        }
        private readonly FrmMain _mainWindow = BmclCore.MainWindow;
        private void listVer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listVer.SelectedIndex == -1)
            {
                listVer.SelectedIndex = 0;
                return;
            }
            this.listVer.ScrollIntoView(listVer.SelectedItem);
            string jsonFilePath = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                MessageBox.Show(LangManager.GetLangFromResource("ErrorNoGameJson"));
                _mainWindow.SwitchStartButton(false);
                return;
            }
            _mainWindow.SwitchStartButton(true);
            BmclCore.GameInfo = gameinfo.Read(jsonFilePath);
            if (BmclCore.GameInfo == null)
            {
                MessageBox.Show(LangManager.GetLangFromResource("ErrorJsonEncoding"));
                return;
            }
            labVer.Content = BmclCore.GameInfo.id;
            labTime.Content = BmclCore.GameInfo.time;
            labRelTime.Content = BmclCore.GameInfo.releaseTime;
            labType.Content = BmclCore.GameInfo.type;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(LangManager.GetLangFromResource("DeleteMessageBoxInfo"), LangManager.GetLangFromResource("DeleteMessageBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                try
                {
                    if (BmclCore.GameInfo != null)
                    {
                        FileStream isused = File.OpenWrite(".minecraft\\versions\\" + listVer.SelectedItem + "\\" + BmclCore.GameInfo.id + ".jar");
                        isused.Close();
                    }
                    Directory.Delete(".minecraft\\versions\\" + listVer.SelectedItem, true);
                    if (Directory.Exists(".minecraft\\libraries\\" + listVer.SelectedItem))
                    {
                        Directory.Delete(".minecraft\\libraries\\" + listVer.SelectedItem, true);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("DeleteFailedMessageInfo"));
                }
                catch (IOException)
                {
                    MessageBox.Show(LangManager.GetLangFromResource("DeleteFailedMessageInfo"));
                }
                finally
                {
                    ReFlushlistver();
                }
            }
        }

        private void btnReName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rname = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("RenameNewName"), LangManager.GetLangFromResource("RenameTitle"), listVer.SelectedItem.ToString());
                if (rname == "") return;
                if (rname == listVer.SelectedItem.ToString()) return;
                if (listVer.Items.IndexOf(rname) != -1) throw new Exception(LangManager.GetLangFromResource("RenameFailedExist"));
                Directory.Move(".minecraft\\versions\\" + listVer.SelectedItem, ".minecraft\\versions\\" + rname);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(LangManager.GetLangFromResource("RenameFailedError"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.ReFlushlistver();
            }
        }

        private void btnModMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments =
                     Path.Combine(ModHelper.SetupModPath(listVer.SelectedItem.ToString()), "mods")
            });
        }

        private void btnImportOldMc_Click(object sender, RoutedEventArgs e)
        {
            var folderImportOldVer = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = LangManager.GetLangFromResource("ImportDirInfo")
            };
            var prs = new FrmPrs(LangManager.GetLangFromResource("ImportPrsTitle"));
            prs.Show();
            if (folderImportOldVer.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string importFrom = folderImportOldVer.SelectedPath;
                if (!File.Exists(importFrom + "\\bin\\minecraft.jar"))
                {
                    MessageBox.Show(LangManager.GetLangFromResource("ImportFailedNoMinecraftFound"));
                    return;
                }
                bool f1, f2;
                string importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportOldMcInfo"), "OldMinecraft");
                do
                {
                    f1 = false;
                    f2 = false;
                    if (importName.Length <= 0 || importName.IndexOf('.') != -1)
                        importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportInvildName"), "OldMinecraft");
                    else
                        f1 = true;
                    if (Directory.Exists(".minecraft\\versions\\" + importName))
                        importName = Microsoft.VisualBasic.Interaction.InputBox(LangManager.GetLangFromResource("ImportNameInfo"), LangManager.GetLangFromResource("ImportFailedExist"), "OldMinecraft");
                    else
                        f2 = true;

                } while (!(f1 && f2));
                VersionHelper.ImportOldMc(importName, importFrom, new Action(() =>
                {
                    prs.Close();
                    MessageBox.Show(BmclCore.MainWindow, LangManager.GetLangFromResource("ImportOldMCInfo"));
                }
                ));
            }
            else prs.Close();
        }

        private void btnCoreModMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments =
                    Path.Combine(ModHelper.SetupModPath(listVer.SelectedItem.ToString()), "coremods")
            });
        }

        private void btnModdirMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments =
                    Path.Combine(ModHelper.SetupModPath(listVer.SelectedItem.ToString()), "moddir")
            });
        }

        private void btnModCfgMrg_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments =
                    Path.Combine(ModHelper.SetupModPath(listVer.SelectedItem.ToString()), "config")
            });
        }

        private void listVer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BmclCore.MainWindow.ClickStartButton();
        }

        private void btnLibraries_Click(object sender, RoutedEventArgs e)
        {
            var f = new FrmLibraries(BmclCore.GameInfo.libraries);
            if (f.ShowDialog() == true)
            {
                BmclCore.GameInfo.libraries = f.GetChange();
                string jsonFile = gameinfo.GetGameInfoJsonPath(listVer.SelectedItem.ToString());
                File.Delete(jsonFile + ".bak");
                File.Move(jsonFile, jsonFile + ".bak");
                gameinfo.Write(BmclCore.GameInfo, jsonFile);
                this.listVer_SelectionChanged(null, null);
            }
        }

        public void ReFlushlistver()
        {
            listVer.Items.Clear();

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\.minecraft"))
            {
                {
                    MessageBox.Show(LangManager.GetLangFromResource("NoClientFound"));
                    BmclCore.MainWindow.SwitchStartButton(false);
                    btnDelete.IsEnabled = false;
                    btnModCfgMrg.IsEnabled = false;
                    btnModdirMrg.IsEnabled = false;
                    btnModMrg.IsEnabled = false;
                    btnCoreModMrg.IsEnabled = false;
                    return;
                }
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions\"))
            {
                MessageBox.Show(LangManager.GetLangFromResource("InvidMinecratDir"));
                BmclCore.MainWindow.SwitchStartButton(false);
                btnDelete.IsEnabled = false;
                btnModCfgMrg.IsEnabled = false;
                btnModdirMrg.IsEnabled = false;
                btnModMrg.IsEnabled = false;
                btnCoreModMrg.IsEnabled = false;
                return;
            }
            DirectoryInfo[] versions = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\.minecraft\versions").GetDirectories();
            foreach (DirectoryInfo version in versions)
            {
                listVer.Items.Add(version.Name);
            }
            if (listVer.Items.Count != 0)
            {
                listVer.SelectedIndex = 0;
                BmclCore.MainWindow.SwitchStartButton(true);
                btnDelete.IsEnabled = true;
                btnModCfgMrg.IsEnabled = true;
                btnModdirMrg.IsEnabled = true;
                btnModMrg.IsEnabled = true;
                btnCoreModMrg.IsEnabled = true;
            }
            else
            {
                BmclCore.MainWindow.SwitchStartButton(false);
                btnDelete.IsEnabled = false;
                btnModCfgMrg.IsEnabled = false;
                btnModdirMrg.IsEnabled = false;
                btnModMrg.IsEnabled = false;
                btnCoreModMrg.IsEnabled = false;
            }
        }

        public string GetSelectedVersion()
        {
            return listVer.SelectedItem as string;
        }
    }
}
