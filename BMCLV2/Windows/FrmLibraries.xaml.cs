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
using System.Data;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections;
using BMCLV2.I18N;
using BMCLV2.libraries;

namespace BMCLV2
{
    /// <summary>
    /// FrmLibraries.xaml 的交互逻辑
    /// </summary>
    public partial class FrmLibraries : Window
    {
        libraryies[] Lib;
        DataTable LibTable = new DataTable();
        bool changed = false;
        public FrmLibraries(libraryies[] gamelibraries)
        {
            InitializeComponent();
            this.Lib = gamelibraries.Clone() as libraryies[];
            LibTable.Columns.Add("name");
            LibTable.Columns.Add("url");
            for (int i = 0; i < gamelibraries.Count(); i++)
            {
                LibTable.Rows.Add(gamelibraries[i].name, gamelibraries[i].url);
            }
            LibTable.RowChanged += LibTable_RowChanged;
            this.dataLib.ItemsSource = LibTable.DefaultView;
        }

        void LibTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            int SelectedIndex = dataLib.SelectedIndex;
            if (SelectedIndex == -1)
                return;
            LibTable.RowChanged -= LibTable_RowChanged;
            if (SelectedIndex > Lib.Count())
            {
                ArrayList a = new ArrayList(Lib);
                libraryies l = new libraryies();
                l.name = e.Row["name"].ToString();
                l.url = e.Row["url"].ToString();
                if (string.IsNullOrEmpty(l.url))
                    l.url = null;
                a.Add(l);
                Lib = a.ToArray(typeof(libraryies)) as libraryies[];
                LibTable.RowChanged+=LibTable_RowChanged;
            }
            else
            {
                Lib[SelectedIndex].name = e.Row["name"].ToString();
                Lib[SelectedIndex].url = e.Row["url"].ToString();
                if (string.IsNullOrEmpty(Lib[SelectedIndex].url))
                    Lib[SelectedIndex].url = null;
            }
            LibTable.RowChanged += LibTable_RowChanged;
            changed = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dataLib.CancelEdit();
            if (changed)
            {
                if (MessageBox.Show(LangManager.GetLangFromResource("LibIsSaveQuestion"), "", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
#if DEBUG
                    DataContractJsonSerializer j = new DataContractJsonSerializer(typeof(libraryies[]));
                    MemoryStream s = new MemoryStream();
                    j.WriteObject(s, Lib);
                    StreamReader sr = new StreamReader(s);
                    s.Position = 0;
                    MessageBox.Show(sr.ReadToEnd());
#endif
                    this.DialogResult = true;
                }
                else
                {
                    this.DialogResult = false;
                }
            }
        }

        public libraryies[] GetChange()
        {
            return Lib;
        }

        private void MenuLibMenuInsert_Click(object sender, RoutedEventArgs e)
        {
            int SelectedIndex = dataLib.SelectedIndex;
            LibTable.RowChanged -= LibTable_RowChanged;
            ArrayList a = new ArrayList(Lib);
            LibTable.Rows.InsertAt(LibTable.NewRow(), SelectedIndex);
            a.Insert(SelectedIndex, new libraryies());
            Lib = a.ToArray(typeof(libraryies)) as libraryies[];
            LibTable.RowChanged += LibTable_RowChanged;
        }

        private void MenuLibMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            changed = true;
            int SelectedIndex = dataLib.SelectedIndex;
            LibTable.RowChanged -= LibTable_RowChanged;
            ArrayList a = new ArrayList(Lib);
            a.RemoveAt(SelectedIndex);
            LibTable.Rows.RemoveAt(SelectedIndex);
            Lib = a.ToArray(typeof(libraryies)) as libraryies[];
            LibTable.RowChanged += LibTable_RowChanged;
        }
    }
}
