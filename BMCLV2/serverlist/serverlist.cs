using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace BMCLV2.serverlist
{
    public class serverlist
    {
        public int count = 0;
        private ArrayList list = new ArrayList(5);
        public serverinfo[] info;
        public serverlist()
        {
            try
            {
                using (var s = new StreamReader(@".minecraft\servers.dat"))
                {
                    var split = new string(new char[] { (char)0x1, (char)0x0, (char)0xb });
                    var split1 = new string(new char[] { (char)0x8, (char)0x0, (char)0x2 });
                    var split2 = new string(new char[] { (char)0x8, (char)0x0, (char)0x4 });
                    var x = s.ReadToEnd();
                    var l = x.IndexOf(split + "hideAddress", System.StringComparison.Ordinal) > 0 ?
                        x.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries) :
                        x.Split(new string[] { split2 }, StringSplitOptions.RemoveEmptyEntries);
                    var listlength = (int)l[0][l[0].Length - 1];
                    foreach (var str in l.Skip(1).ToArray())
                    {
                        if (str.IndexOf("hideAddress", System.StringComparison.Ordinal) > -1)
                        {
                            var strr = str.Split(new string[] { split1, split2 }, StringSplitOptions.RemoveEmptyEntries);
                            list.Add(new serverinfo(strr[1].Substring(6), Convert.ToBoolean((int)strr[0][strr[0].Length - 1]), strr[2].Substring(4).Replace("\0", "")));
                        }
                        else
                        {
                            var strr = str.Split(new string[] { split1, split2 }, StringSplitOptions.RemoveEmptyEntries);
                            list.Add(new serverinfo(strr[0].Substring(6), false, strr[1].Substring(4).Replace("\0", "")));
                        }
                    }
                    info = (serverinfo[])list.ToArray(typeof(serverinfo));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("读取文件列表发生错误:" + ex.Message);
            }
            finally
            {
                info = (serverinfo[])list.ToArray(typeof(serverinfo));
            }
        }
        public void Add(string name,string ip,bool ishide)
        {
            
            serverinfo newserver = new serverinfo(name, ishide, ip);
            list.Add(newserver);
            info = (serverinfo[])list.ToArray(typeof(serverinfo));
        }
        public void Write()
        {
            FileStream serverdat = new FileStream(@".minecraft\servers.dat", FileMode.Create);
            serverdat.Write(Convert.FromBase64String(Resource.ServerDat.Header), 0, Convert.FromBase64String(Resource.ServerDat.Header).Length);
            int p = 0;
            serverdat.WriteByte(0);
            serverdat.WriteByte((byte)list.Count);
            serverdat.WriteByte(1);
            foreach (serverinfo aserver in list)
            {
                p++;
                serverdat.Write(Convert.FromBase64String(Resource.ServerDat.HideAddress), 0, Convert.FromBase64String(Resource.ServerDat.HideAddress).Length);
                if (aserver.IsHide)
                    serverdat.WriteByte(1);
                else
                    serverdat.WriteByte(0);
                serverdat.WriteByte(8);
                serverdat.Write(Convert.FromBase64String(Resource.ServerDat.name), 0, Convert.FromBase64String(Resource.ServerDat.name).Length);
                serverdat.WriteByte((byte)Encoding.UTF8.GetBytes(aserver.Name).Length);
                serverdat.Write(Encoding.UTF8.GetBytes(aserver.Name),0,Encoding.UTF8.GetBytes(aserver.Name).Length);
                serverdat.Write(new byte[] { 8, 0}, 0, 2);
                serverdat.Write(Convert.FromBase64String(Resource.ServerDat.address),0,Convert.FromBase64String(Resource.ServerDat.address).Length);
                serverdat.WriteByte((byte)Encoding.UTF8.GetBytes(aserver.Address).Length);
                serverdat.Write(Encoding.UTF8.GetBytes(aserver.Address),0,Encoding.UTF8.GetBytes(aserver.Address).Length);
                serverdat.WriteByte(0);
                if (p!=list.Count)
                    serverdat.WriteByte(1);
                else
                    serverdat.WriteByte(0);
            }
            serverdat.Close();
        }

        public void Delete(int num)
        {
            list.RemoveAt(num);
            info = (serverinfo[])list.ToArray(typeof(serverinfo));
        }

        public void Edit(int num, string Name, string Address, bool IsHide)
        {
            serverinfo aserver = new serverinfo(Name, IsHide, Address);
            list[num] = aserver;
            info = (serverinfo[])list.ToArray(typeof(serverinfo));
        }
    }
}
