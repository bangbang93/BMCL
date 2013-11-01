Public Class yggdrasil
    Private Const BaseUrl = "https://authserver.mojang.com"
    Private CO As New System.Text.UTF8Encoding
    Private Structure UserList
        Dim Name As String
        Dim SID As String
        Dim UID As String
    End Structure
    Public Structure LoginInfo
        Dim UN As String
        Dim UID As String
        Dim SID As String
        Dim Suc As Boolean
        Dim Errinfo As String
        Dim OtherInfo As String
        Dim Client_identifier As String
    End Structure
    Public Function Login(ByVal username As String, ByVal password As String, Optional Client_identifier As String = "", Optional Language As String = "zh-cn") As LoginInfo
        Dim RT As LoginInfo
        Dim accessToken As String
        Dim UL As UserList()
        Dim yggdrasilFrm As New yggdrasilSelect
        Dim JO As New JSON
        Dim Text As Byte()
        RT = Nothing
        RT.Suc = False
        ReDim JO.JSON.Strings(3)
        ReDim JO.JSON.Values(3)
        JO.JSON.Strings = {"agent", "username", "password", "clientToken"}
        JO.JSON.Values(0).Type = JSON.JSONTYPE.Object
        ReDim JO.JSON.Values(0).Objects.Strings(1)
        ReDim JO.JSON.Values(0).Objects.Values(1)
        JO.JSON.Values(0).Objects.Strings = {"name", "version"}
        JO.JSON.Values(0).Objects.Values(0).Type = JSON.JSONTYPE.String
        JO.JSON.Values(0).Objects.Values(1).Type = JSON.JSONTYPE.String
        JO.JSON.Values(0).Objects.Values(0).Strings = "Minecraft"
        JO.JSON.Values(0).Objects.Values(1).Strings = 1
        JO.JSON.Values(1).Type = JSON.JSONTYPE.String
        JO.JSON.Values(1).Strings = username
        JO.JSON.Values(2).Type = JSON.JSONTYPE.String
        JO.JSON.Values(2).Strings = password
        JO.JSON.Values(3).Type = JSON.JSONTYPE.String
        If Client_identifier = "" Then Client_identifier = System.Guid.NewGuid.ToString()
        JO.JSON.Values(3).Strings = Client_identifier
        Text = CO.GetBytes(JO.ToString)
        Try
            Dim req As Net.HttpWebRequest = Net.HttpWebRequest.Create(BaseUrl & "/authenticate")
            req.Method = "POST"
            req.ContentType = "application/json"
            req.ContentLength = Text.Length
            req.ProtocolVersion = New Version("1.0")
            req.KeepAlive = False
            Dim reqStream As IO.Stream = req.GetRequestStream
            reqStream.Write(Text, 0, Text.Length)
            Dim wr As Net.WebResponse = req.GetResponse
            Dim R() As Byte
            If wr.ContentLength = 0 Then
                RT.Errinfo = "NoReturnData"
                RT.Suc = False
                Return RT
            End If
            ReDim R(wr.ContentLength - 1)
            For i = 0 To wr.ContentLength - 1
                R(i) = wr.GetResponseStream.ReadByte
            Next
            JO.JSONStr = CO.GetString(R)
            If Left(JO.JSONStr, 1) <> "{" Then JO.JSONStr = "{" & JO.JSONStr
            If Right(JO.JSONStr, 1) <> "}" Then JO.JSONStr &= "}"
        Catch WebExcp As System.Net.WebException
            RT.Errinfo = WebExcp.Message
            RT.Suc = False
            Return RT
        Catch ex As Exception
            RT.Errinfo = ex.Message
            RT.Suc = False
            Return RT
        End Try
        Try
            JO.Analysis()
        Catch ex As Exception
            RT.Errinfo = "JSON File Error"
            RT.Suc = False
            Return RT
        End Try
        If JO.JSON.Strings.Length = 0 Then
            RT.Suc = False
            RT.Errinfo = "No user list"
            Return RT
        End If
        For i = 0 To UBound(JO.JSON.Strings)
            Select Case JO.JSON.Strings(i)
                Case "error"
                    RT.Suc = False
                    RT.Errinfo = JO.JSONStr
                    Return RT
                Case "accessToken"
                    Select Case JO.JSON.Values(i).Type
                        Case JSON.JSONTYPE.String
                            accessToken = JO.JSON.Values(i).Strings
                            RT.OtherInfo = accessToken
                        Case Else
                            RT.Suc = False
                            RT.Errinfo = "JSON Error"
                            Return RT
                    End Select
                Case "clientToken"
                    Select Case JO.JSON.Values(i).Type
                        Case JSON.JSONTYPE.String
                            RT.Client_identifier = JO.JSON.Values(i).Strings
                        Case Else
                            RT.Suc = False
                            RT.Errinfo = "JSON Error"
                            Return RT
                    End Select
                Case "availableProfiles"
                    Select Case JO.JSON.Values(i).Arrays.Values.Length
                        Case 0
                            RT.Suc = False
                            RT.Errinfo = JO.JSON.ToString
                            RT.OtherInfo = JO.JSONStr
                            Return RT
                        Case 1
                            Select Case JO.JSON.Values(i).Arrays.Values(0).Type
                                Case JSON.JSONTYPE.Object
                                    If JO.JSON.Values(i).Arrays.Values(0).Objects.Strings.Length = 0 Then
                                        RT.Suc = False
                                        RT.Errinfo = "JSON Error"
                                        Return RT
                                    Else
                                        For k = 0 To UBound(JO.JSON.Values(i).Arrays.Values(0).Objects.Strings)
                                            Select Case JO.JSON.Values(i).Arrays.Values(0).Objects.Strings(k)
                                                Case "id"
                                                    RT.UID = JO.JSON.Values(i).Arrays.Values(0).Objects.Values(k).Strings
                                                    RT.SID = "token:" & RT.OtherInfo & ":" & JO.JSON.Values(i).Arrays.Values(0).Objects.Values(k).Strings
                                                Case "name"
                                                    RT.UN = JO.JSON.Values(i).Arrays.Values(0).Objects.Values(k).Strings
                                            End Select
                                        Next
                                        If (RT.UN <> "") And (RT.SID <> "") Then
                                            RT.Suc = True
                                            Return RT
                                        End If
                                    End If
                                Case Else
                                    RT.Suc = False
                                    RT.Errinfo = "JSON Error"
                                    Return RT
                            End Select
                        Case Else
                            For j = 0 To UBound(JO.JSON.Values(i).Arrays.Values)
                                ReDim UL(UBound(JO.JSON.Values(i).Arrays.Values))
                                Select Case JO.JSON.Values(i).Arrays.Values(j).Type
                                    Case JSON.JSONTYPE.Object
                                        If JO.JSON.Values(i).Arrays.Values(j).Objects.Strings.Length = 0 Then
                                            RT.Suc = False
                                            RT.Errinfo = "No User List"
                                            Return RT
                                        Else
                                            yggdrasilFrm.ComboBox1.Items.Clear()
                                            For k = 0 To UBound(JO.JSON.Values(i).Arrays.Values(j).Objects.Strings)
                                                Select Case JO.JSON.Values(i).Arrays.Values(j).Objects.Strings(k)
                                                    Case "id"
                                                        UL(k).Name = JO.JSON.Values(i).Arrays.Values(j).Objects.Values(k).Strings
                                                    Case "name"
                                                        UL(k).UID = JO.JSON.Values(i).Arrays.Values(j).Objects.Values(k).Strings
                                                        UL(k).SID = "token:" & RT.OtherInfo & ":" & JO.JSON.Values(i).Arrays.Values(j).Objects.Values(k).Strings
                                                        Dim c As Integer
                                                        c = yggdrasilFrm.ComboBox1.Items.Add(UL(k).Name)
                                                        If c <> k Then
                                                            RT.Suc = False
                                                            RT.Errinfo = "JSON Error"
                                                            Return RT
                                                        End If
                                                End Select
                                            Next
                                            yggdrasilFrm.ShowDialog()
                                            If yggdrasilFrm.ComboBox1.SelectedIndex = -1 Then
                                                RT.Suc = False
                                                RT.Errinfo = "Not Selected"
                                                Return RT
                                            End If
                                            RT.UN = yggdrasilFrm.ComboBox1.SelectedItem
                                            RT.SID = UL(yggdrasilFrm.ComboBox1.SelectedIndex).SID
                                            RT.UID = UL(yggdrasilFrm.ComboBox1.SelectedIndex).UID
                                            If (RT.UN <> "") And (RT.SID <> "") Then
                                                RT.Suc = True
                                            End If
                                            Return RT
                                        End If
                                    Case Else
                                        RT.Suc = False
                                        RT.Errinfo = "JSON Error"
                                        Return RT
                                End Select
                            Next
                    End Select
                Case "selectedProfile"

            End Select
        Next
        Return RT
    End Function
    Public Function GetVer() As Long
        Return 1
        '代表为第一代标准化登陆插件
    End Function
    Public Function GetName(Optional Language As String = "zh-cn") As String
        Return "yggdrasilvb"
        '返回使用的名称
    End Function
End Class
