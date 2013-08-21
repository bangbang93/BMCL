Public Class Legacy
    Private CO As New System.Text.UTF8Encoding
    Public Structure LoginInfo
        Dim UN As String
        Dim UID As String
        Dim SID As String
        Dim Suc As Boolean
        Dim Errinfo As String
        Dim OtherInfo As String
        Dim Client_identifier As String
    End Structure
    Public Function Login(UserName As String, Password As String, Optional Client_identifier As String = "", Optional Language As String = "zh-cn") As LoginInfo
        '登陆部分
        Dim RT As LoginInfo
        Dim t As String
        Dim t1 As String()
        RT = Nothing
        RT.Suc = False
        Try
            Dim req As Net.HttpWebRequest
            req = Net.HttpWebRequest.Create("http://login.minecraft.net")
            Dim text As Byte()
            text = CO.GetBytes("user=" & UserName & "&password=" & Password & "&version=14")
            req.Method = "POST"
            req.ContentType = "application/x-www-form-urlencoded"
            req.Accept = "*/*"
            req.UserAgent = "Mozilla/4.0 (compatible)"
            req.ProtocolVersion = New Version("1.1")
            req.Headers("Accept-Encoding") = "gzip, deflate"
            req.ContentLength = text.Length
            Dim reqStream As IO.Stream = req.GetRequestStream
            reqStream.Write(text, 0, text.Length)
            Dim wr As Net.WebResponse = req.GetResponse
            ' 创建一个GZip解压流
            Dim gz As New IO.Compression.GZipStream(wr.GetResponseStream, IO.Compression.CompressionMode.Decompress)
            ' 用一个临时内存流来保存解压数据
            Dim ms As New IO.MemoryStream
            ' 缓冲数据
            Dim buf(99) As Byte, i As Integer = 0
            ' 不断从流中解压数据
            While True
                i = gz.Read(buf, 0, 100)
                If i = 0 Then Exit While
                ms.Write(buf, 0, i)
            End While
            ' 将数据转换为字符
            Dim ret As String = CO.GetString(ms.ToArray)
            t = ret
        Catch WebExcp As System.Net.WebException
            t = WebExcp.Message
        Catch ex As Exception
            t = ex.Message
        End Try
        t1 = t.Split(":")
        If t1.Length <> 5 Then
            RT.Suc = False
            RT.Errinfo = t
            Return RT
        End If
        RT.Suc = True
        RT.UN = t1(2)
        RT.SID = t1(3)
        RT.UID = t1(4)
        Return RT
    End Function
    Public Function GetVer() As Long
        Return 1
        '代表为第一代标准化登陆插件
    End Function
    Public Function GetName(Optional Language As String = "zh-cn") As String
        Select Case Language
            Case "zh-cn", "zh-chs", "zh-hans", "zh-mo"
                Return "正版2"
            Case "zh-tw", "zh-cht", "zh-hant", "zh-hk", "zh-sg"
                Return "正版2"
            Case Else
                If Strings.Left(Language, 2) = "en" Then
                    Return "Genuine2"
                Else
                    MsgBox("不支持您的语言，您可以发送邮件给我" & vbNewLine & "Not support yours language, you can send mail to me." & "zhh0000zhh@sina.com")
                    'MsgBox("程序将以中文继续" & vbNewLine & "Program will be Chinese to continue", , vbInformation)
                    Return "正版2"
                End If
        End Select
        '返回使用的名称
    End Function
End Class
