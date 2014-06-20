using System;
using System.Reflection;

namespace BMCLV2.Login
{
    class LoginThread
    {
        internal delegate void LoginFinishEventHandler(LoginInfo loginInfo);

        public event LoginFinishEventHandler loginFinishEvent;


        private LoginInfo _loginans;
        private readonly string _username;
        private readonly string _password;
        private readonly object _auth;


        protected virtual void onLoginFinishEvent(LoginInfo logininfo)
        {
            LoginFinishEventHandler handler = loginFinishEvent;
            if (handler != null) handler(logininfo);
        }

        public LoginThread(string username, string password, string auth, int selectIndex)
        {
            this._username = username;
            this._password = password;
            if (selectIndex != 0)
            {
                this._auth = BmclCore.Auths[auth];
            }
        }

        public void start()
        {
            if (_auth != null)
            {
                Type T = _auth.GetType();
                MethodInfo login = T.GetMethod("Login");
                try
                {
                    object loginansobj = login.Invoke(_auth, new object[] { _username, _password, System.Guid.NewGuid().ToString(), "zh-cn" });
                    Type li = loginansobj.GetType();
                    _loginans.Suc = (bool)li.GetField("Suc").GetValue(loginansobj);
                    if (_loginans.Suc)
                    {
                        _loginans.UN = li.GetField("UN").GetValue(loginansobj) as string;
                        _loginans.SID = li.GetField("SID").GetValue(loginansobj) as string;
                        _loginans.Client_identifier = li.GetField("Client_identifier").GetValue(loginansobj) as string;
                        _loginans.UID = li.GetField("UID").GetValue(loginansobj) as string;
                        _loginans.OtherInfo = li.GetField("OtherInfo").GetValue(loginansobj) as string;
                        if (li.GetField("OutInfo") != null)
                        {
                            _loginans.OutInfo = li.GetField("OutInfo").GetValue(loginansobj) as string;
                        }
                        Logger.log(string.Format("登陆成功，使用用户名{0},sid{1},Client_identifier{2},uid{3}", _loginans.UN ?? "", _loginans.SID ?? "", _loginans.Client_identifier ?? "", _loginans.UID ?? ""));
                        onLoginFinishEvent(_loginans);
                    }
                    else
                    {
                        _loginans.Errinfo = li.GetField("Errinfo").GetValue(loginansobj) as string;
                        _loginans.OtherInfo = li.GetField("OtherInfo").GetValue(loginansobj) as string;
                        Logger.log(string.Format("登陆失败，错误信息:{0}，其他信息:{1}", _loginans.Errinfo ?? "", _loginans.OtherInfo ?? ""));
                        onLoginFinishEvent(_loginans);
                    }
                }
                catch (Exception ex)
                {
                    Logger.log(ex);
                    _loginans.Suc = false;
                    _loginans.Errinfo = ex.Message;
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        _loginans.Errinfo += "\n" + ex.Message;
                    }
                    onLoginFinishEvent(_loginans);
                }
            }
            else
            {
                _loginans.Suc = true;
                _loginans.SID = "no";
                _loginans.UN = this._username;
                onLoginFinishEvent(_loginans);
            }
        }
    }
}
