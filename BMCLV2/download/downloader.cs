using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;


namespace BMCLV2.download
{
    class downloader
    {
        public downloader(string url)
        {
            this.Url = url;
            this.ThreadCount = 5;
        }

        #region 委托

        public delegate void ExceptionEventHandler(downloader sender, Exception e);
        public delegate void ConnectedEventHandler(downloader sender, string filename, string contentType);
        public delegate void ProgressEventHandler(downloader sender);
        public delegate void FinishedEventHandler(downloader sender);
        public delegate void SpeedHandler(downloader sender);

        #endregion

        #region 成员

        private ExceptionEventHandler _exception;
        private ConnectedEventHandler _connected;
        private ProgressEventHandler _progress;
        private FinishedEventHandler _finished;
        private SpeedHandler _speed;

        private Thread thConnection;
        private Thread[] thdownloaders;
        private Stream fileStream;
        private object lockFinishedLength = new object();
        private int postion = 2 * 2 * 64 * 1024;//块大小

        private Dictionary<string, int> SpeedDic = new Dictionary<string, int>();

        private string ConfigFile = "";
        #endregion

        #region 属性

        /// <summary>
        /// 下载线程数量
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// 下载资源位置
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 资源保存路径
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// 资源长度
        /// </summary>
        public long ContentLength { get; private set; }

        /// <summary>
        /// 资源已下载长度
        /// </summary>
        public long FinishedLength { get; private set; }

        /// <summary>
        /// 下载百分比
        /// </summary>
        public int FinishedRate { get; private set; }

        /// <summary>
        /// 文件夹
        /// </summary>
        public string DirectoryName { get; set; }

        /// <summary>
        /// 进度条
        /// </summary>
        public object ProgressBar { get; set; }

        /// <summary>
        /// 控件
        /// </summary>
        public object Control { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public string SpeedStr { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 当连接上资源后发生
        /// </summary>
        public event ConnectedEventHandler Connected
        {
            add
            {
                this._connected += value;
            }
            remove
            {
                this._connected -= value;
            }
        }

        /// <summary>
        /// 当连下载进度变化后发生
        /// </summary>
        public event ProgressEventHandler Progress
        {
            add
            {
                this._progress += value;
            }
            remove
            {
                this._progress -= value;
            }
        }

        /// <summary>
        /// 当下载完成后发生
        /// </summary>
        public event FinishedEventHandler Finished
        {
            add
            {
                this._finished += value;
            }
            remove
            {
                this._finished -= value;
            }
        }

        /// <summary>
        /// 当连下载发生异常后发生
        /// </summary>
        public event ExceptionEventHandler Exception
        {
            add
            {
                this._exception += value;
            }
            remove
            {
                this._exception -= value;
            }
        }

        /// <summary>
        /// 速度事件
        /// </summary>
        public event SpeedHandler Speed
        {
            add
            {
                this._speed += value;
            }
            remove
            {
                this._speed -= value;
            }
        }

        #endregion

        #region 响应

        protected void OnConnected(string filename, string contentType)
        {
            if (this._connected != null)
            {
                this._connected.Invoke(this, filename, contentType);
            }
        }

        protected void OnProgress()
        {
            // 统计百分比
            lock (this.lockFinishedLength)
            {
                int rate = (int)(this.FinishedLength * 100 / this.ContentLength);
                if (rate != this.FinishedRate)
                    this.FinishedRate = rate;
                else
                    return;
            }
            // 进度事件处理
            if (this._progress != null)
            {
                lock (this._progress)
                {
                    this._progress.Invoke(this);
                }
            }
        }

        protected void OnFinished()
        {
            bool isFinished = true;
            lock (this.thdownloaders)
            {
                foreach (Thread thread in this.thdownloaders)
                {
                    if (thread != null)
                        if (thread.ThreadState != ThreadState.Stopped && thread != Thread.CurrentThread)
                        {
                            isFinished = false;
                            break;
                        }
                }
            }

            if (isFinished)
            {
                this.fileStream.Close();

                if (File.Exists(this.Filename))
                {
                    File.Delete(this.Filename);
                }
                File.Move(this.Filename + ".tfg", this.Filename);

                if (File.Exists(ConfigFile))
                {
                    File.Delete(ConfigFile);
                }

                if (this._finished != null)
                {
                    this._finished.Invoke(this);
                }
            }
        }

        protected void OnException(Exception e)
        {
            this.Abort();

            if (this._exception != null)
            {
                this._exception.Invoke(this, e);
            }
        }

        protected void OnSpeed()
        {
            int result = 0;
            lock (SpeedDic)
            {
                foreach (string key in SpeedDic.Keys)
                {
                    result += SpeedDic[key];
                }
            }
            SpeedStr = result.ToString("f0") + "KB/s";
            if (this._speed != null)
            {
                lock (this._speed)
                {
                    this._speed.Invoke(this);
                }
            }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 开始下载
        /// </summary>
        public void Start()
        {
            if (thConnection == null)
            {
                this.thConnection = new Thread(new ThreadStart(this.Connection));
                this.thConnection.Start();
            }
        }

        /// <summary>
        /// 停止下载
        /// </summary>
        public void Stop()
        {
            // 停止连接
            if (this.thConnection != null)
            {
                this.thConnection.Abort();
                this.thConnection.Join();
            }
            // 停止下载
            this.Abort();
            // 释放资源
            if (this.fileStream != null)
            {
                this.fileStream.Dispose();
            }
        }

        /// <summary>
        /// 终止下载线程
        /// </summary>
        private void Abort()
        {
            if (this.thdownloaders != null)
            {
                lock (this.thdownloaders)
                {
                    foreach (Thread thread in this.thdownloaders)
                    {
                        if (thread != Thread.CurrentThread)
                        {
                            thread.Abort();
                            thread.Join();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 资源连接（读取资源长度，文件名称）
        /// </summary>
        private void Connection()
        {
            try
            {
                // 请求
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(this.Url);
                request.Method = "Head";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                this.ContentLength = response.ContentLength;
                string filename = this.GetFilename(response.GetResponseHeader("Content-Disposition"));
                response.Close();

                this.OnConnected(filename, response.ContentType);

                if (this.ContentLength == 0)
                {
                    this.OnFinished();
                    return;
                }

                // 下载
                if (this.ThreadCount < 1 || this.ThreadCount > 100)
                {
                    throw new Exception("无效的线程数量");
                }

                if (!Directory.Exists(DirectoryName))
                {
                    Directory.CreateDirectory(DirectoryName);
                }

                this.Filename = DirectoryName + "\\" + Filename;

                ConfigFile = Filename + ".cfg";

                #region [ 创建控制文件和空白文件 ]
                if (!File.Exists(ConfigFile))
                {
                    WriteConfigFile();
                    byte[] emptyByte = new byte[ContentLength];
                    FileStream fs = new FileStream(this.Filename + ".tfg", FileMode.Create, FileAccess.Write);
                    fs.Write(emptyByte, 0, emptyByte.Length);
                    fs.Dispose();
                    fs.Close();
                }
                #endregion

                #region [ 下载断点 ]
                ArrayList MustDoan = new ArrayList();
                int complete = 0;
                string[] strBlocks = ReadConfigFile().Split(new char[2] { '\r', '\n' });
                for (int j = 0; j < strBlocks.Length; j++)
                {
                    if (strBlocks[j].Trim().Length != 0 && strBlocks[j].Substring(strBlocks[j].Length - 1) == "0")
                    {
                        MustDoan.Add(strBlocks[j].Trim());
                    }
                    else if (strBlocks[j].Trim().Length != 0 && strBlocks[j].Substring(strBlocks[j].Length - 1) == "1")
                    {
                        complete++;
                    }
                }
                this.FinishedLength = complete * postion;
                #endregion

                this.fileStream = new FileStream(this.Filename + ".tfg", FileMode.Open, FileAccess.Write);

                this.thdownloaders = new Thread[this.ThreadCount];
                int arrLen = MustDoan.Count;
                int modNum = arrLen / ThreadCount;
                int endNum = modNum + arrLen % ThreadCount;
                // int modNum =  (int)(this.ContentLength / this.thdownloaders.Length);
                for (int i = 0; i < thdownloaders.Length; i++)
                {
                    int from = i * modNum;
                    //int to = (i == thdownloaders.Length - 1) ? (int)this.ContentLength : from + modNum;
                    int to = from + (i == ThreadCount - 1 ? endNum : modNum);
                    this.thdownloaders[i] = new Thread(new ParameterizedThreadStart(this.downloaderBlock));
                    this.thdownloaders[i].Name = "T" + i;
                    this.thdownloaders[i].Start(new object[] { from, to, MustDoan });
                }
            }
            catch (Exception e)
            {
                if (!(e is ThreadAbortException))
                {
                    this.OnException(e);
                }
            }
        }

        /// <summary>
        /// 下载数据
        /// </summary>
        private void downloaderBlock(object rang)
        {
            int from = Convert.ToInt32((rang as object[])[0]);
            int to = Convert.ToInt32((rang as object[])[1]);
            ArrayList MustDoan = (rang as object[])[2] as ArrayList;

            HttpWebRequest hwRequest = null;
            HttpWebResponse hwResponse = null;
            Stream responseStream = null;
            while (from < to)
            {
                string[] strRecord = MustDoan[from].ToString().Split(',');
                int reStart = int.Parse(strRecord[0]);
                int reEnd = int.Parse(strRecord[1]);
                try
                {
                    hwRequest = (HttpWebRequest)HttpWebRequest.Create(this.Url);
                    hwRequest.AddRange(reStart, reStart + reEnd);
                    hwResponse = (HttpWebResponse)hwRequest.GetResponse();
                    responseStream = hwResponse.GetResponseStream();

                    byte[] buffer = new byte[1024 * 10];
                    int size = 0, count = 0;
                    TimeSpan tsBegin;
                    DateTime dtBegin = DateTime.Now;
                    int secondSize = size;
                    do
                    {
                        DateTime dtEnd = DateTime.Now;
                        tsBegin = dtEnd - dtBegin;
                        if (tsBegin.TotalSeconds >= 1)
                        {
                            dtBegin = DateTime.Now;
                            double totalSecond = tsBegin.TotalSeconds;
                            double speed = secondSize / (totalSecond * 1024);
                            SetSpeed(Thread.CurrentThread.Name, (int)speed);
                            secondSize = 0;
                        }

                        size = responseStream.Read(buffer, 0, buffer.Length);
                        lock (this.lockFinishedLength)
                        {
                            this.FinishedLength += size;
                        }

                        lock (this.fileStream)
                        {
                            this.fileStream.Seek(reStart + count, SeekOrigin.Begin);
                            this.fileStream.Write(buffer, 0, size);
                        }

                        UpdateConfigFile(MustDoan[from].ToString());

                        secondSize += size;
                        reStart += size;
                        this.OnProgress();
                        this.OnSpeed();

                    } while (size > 0);
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        this.OnException(e);
                    }
                }
                finally
                {
                    if (responseStream != null)
                    {
                        responseStream.Close();
                    }
                    if (hwResponse != null)
                    {
                        hwResponse.Close();
                    }
                }
                from++;
            }

            this.OnFinished();
        }

        /// <summary>
        /// 得到文件名称
        /// </summary>
        private string GetFilename(string contentDisposition)
        {
            string filename = "";
            if (contentDisposition.IndexOf("filename=") != -1)
            {
                filename = filename.Substring(filename.IndexOf("filename=") + 9);
            }
            else
            {
                filename = this.Url.Substring(this.Url.LastIndexOf("/") + 1);
            }
            return filename;
        }

        /// <summary>
        /// 创建控制文件
        /// </summary>
        private void WriteConfigFile()
        {
            StreamWriter sw = new StreamWriter(ConfigFile);
            int p = 0;
            int l = (int)this.ContentLength;
            int m = postion;
            while (l >= m)
            {
                l -= m;
                if (l < m)
                {
                    m += l;
                }
                sw.WriteLine(p.ToString() + "," + m.ToString() + "," + "0");
                p += m;
            }
            sw.Dispose();
            sw.Close();
        }

        /// <summary>
        /// 读取控制文件
        /// </summary>
        /// <returns></returns>
        public string ReadConfigFile()
        {
            string result = "";
            using (StreamReader sr = new StreamReader(ConfigFile))
            {
                result = sr.ReadToEnd().Trim();
            }
            return result;
        }

        /// <summary>
        /// 更新控制文件
        /// </summary>
        /// <param name="str"></param>
        public void UpdateConfigFile(string str)
        {
            lock (ConfigFile)
            {
                string s = null;
                using (StreamReader sr = new System.IO.StreamReader(ConfigFile))
                {
                    s = sr.ReadToEnd().Trim();

                    string[] ss = str.Split(',');
                    s = s.Replace(ss[0] + "," + ss[1] + ",0", ss[0] + "," + ss[1] + ",1");
                }
                using (StreamWriter sw = new StreamWriter(ConfigFile, false, Encoding.Default))
                {
                    sw.WriteLine(s);
                }
            }
        }

        /// <summary>
        /// SetSpeed
        /// </summary>
        /// <param name="threadName"></param>
        /// <param name="speed"></param>
        private void SetSpeed(string threadName, int speed)
        {
            lock (SpeedDic)
            {
                if (SpeedDic.ContainsKey(threadName))
                    SpeedDic[threadName] = speed;
                else
                    SpeedDic.Add(threadName, speed);
            }
        }
        #endregion
    }

}
