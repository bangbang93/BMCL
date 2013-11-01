'接口定义说明
'JSON为JSONObject类型，JSON格式化后的值
'JSONStr为String类型，JSON的文本格式值
'Language为String()类型，定义语言，Unicode格式初始值为{"DebugMsg", "BAD JSON String", "Function Not Support Now"}其中"DebugMsg"无用
'zhh0000zhh
Friend Class JSON
    Public Structure JSONValue
        Dim Type As JSONTYPE
        Dim Strings As String
        Dim Objects As JSONObject
        Dim Arrays As JSONArray
    End Structure
    Public Enum JSONTYPE
        [NULL] = 0
        [String] = 1
        [Object] = 2
        [Array] = 3
    End Enum
    Public Structure JSONObject
        Dim Strings() As String
        Dim Values() As JSONValue
        Public Overloads Function ToString() As String
            Dim JO As JSONObject
            Dim J As New JSON
            JO.Strings = Me.Strings
            JO.Values = Me.Values
            Return J.GetObject(JO)
        End Function
    End Structure
    Public Structure JSONArray
        Dim Values() As JSONValue
        Public Overloads Function ToString() As String
            Dim JO As JSONArray
            Dim J As New JSON
            JO.Values = Me.Values
            Return J.GetArr(JO)
        End Function
    End Structure
    Public JSON As JSONObject
    Public JSONStr As String
    Public Language As String() = {"DebugMsg", "BAD JSON String", "Function Not Support Now"}
    Public Sub Analysis()
        JSONStr = Replace(JSONStr, vbLf, "")
        JSONStr = Replace(JSONStr, vbCr, "")
        JSONStr = Mid(JSONStr, 2, Len(JSONStr) - 2)
        JSON = AnalysisObject(JSONStr)
    End Sub
    Private Function AnalysisObject(ByVal Str As String) As JSONObject
        Dim RT As JSONObject
        RT.Values = {}
        RT.Strings = {}
        Dim str1 As String()
        Dim str2 As String()
        Dim i1 As Integer
        Dim i2 As Integer
        Dim c As Long
        Dim tstr As String
        str1 = Str.Split(",")
        c = 0
        i2 = 0
        For Each strtemp As String In str1
            c += 1
            If InStr(Str, strtemp) < i2 Then Continue For
            str2 = strtemp.Split(":")
            If str2.Length = 0 Then Continue For
            If str2.Length > 2 Then
                For i = 2 To str2.Length - 1
                    str2(1) &= ":" & str2(i)
                Next
            End If
            ReDim Preserve RT.Strings(UBound(RT.Strings) + 1)
            ReDim Preserve RT.Values(UBound(RT.Values) + 1)
            If str2.Length = 1 Then RT.Values(UBound(RT.Values)).Type = JSONTYPE.NULL
            If str2(0).Split(Chr(34)).Length = 1 Then
                RT.Strings(UBound(RT.Strings)) = str2(0)
            Else
                RT.Strings(UBound(RT.Strings)) = str2(0).Split(Chr(34))(1)
            End If
            'RT.Strings(UBound(RT.Strings)) = str2(0).Replace(Chr(34), "")
            For i = 1 To Len(str2(1))
                If Mid(str2(1), i, 1) = " " Then Continue For
                Select Case Mid(str2(1), i, 1)
                    Case Chr(34)
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.String
                        tstr = str2(1).Split(Chr(34))(1).Replace("\\", "\").Replace("\/", "/").Replace("\n", vbCrLf).Replace("\t", vbTab).Replace("\" & Chr(34), Chr(34))
                        If Left(tstr, 1) = Chr(34) Then tstr = Right(tstr, Len(tstr) - 1)
                        If Right(tstr, 1) = Chr(34) Then tstr = Left(tstr, Len(tstr) - 1)
                        RT.Values(UBound(RT.Values)).Strings = tstr
                        i2 = 0
                    Case "{"
                        i1 = Instrs(Str, ",", c)
                        i1 = InStr(i1, Str, "{") + 1
                        i = i1
                        i2 = i1
                        Do While True
                            i1 = InStr(i1, Str, "{")
                            i2 = InStr(i2, Str, "}")
                            If i1 = 0 Then Exit Do
                            If i2 = 0 Then MsgBox(Language(1)) : Return RT
                            If i2 < i1 Then
                                Exit Do
                            End If
                        Loop
                        i += 1
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.Object
                        If i2 - i = 0 Then
                            RT.Values(UBound(RT.Values)).Objects = Nothing
                        Else
                            RT.Values(UBound(RT.Values)).Objects = AnalysisObject(Mid(Str, i, i2 - i))
                        End If
                    Case "["
                        i1 = Instrs(Str, ",", c)
                        i1 = InStr(i1, Str, "[") + 1
                        i = i1
                        i2 = i1
                        Do While True
                            i1 = InStr(i1, Str, "[")
                            i2 = InStr(i2, Str, "]")
                            If i1 = 0 Then Exit Do
                            If i2 = 0 Then MsgBox(Language(1)) : Return RT
                            If i2 < i1 Then
                                Exit Do
                            End If
                        Loop
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.Array
                        If i2 - i = 0 Then
                            RT.Values(UBound(RT.Values)).Arrays.Values = {}
                        Else
                            RT.Values(UBound(RT.Values)).Arrays = AnalysisArray(Mid(Str, i, i2 - i))
                        End If
                    Case Else
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.String
                        RT.Values(UBound(RT.Values)).Strings = Mid(str2(1), i)
                        i2 = 0
                End Select
                Exit For
            Next
        Next
        Return RT
    End Function
    Private Function Instrs(ByRef Str As String, ByRef Str1 As String, times As Integer) As Integer
        Dim InS As Integer
        times -= 1
        If times = 0 Then Return 1
        InS = InStr(Str, Str1)
        For i = 1 To times - 1
            InS = InStr(InS + 1, Str, Str1)
        Next
        Return InS
    End Function
    Private Function AnalysisArray(ByVal str As String) As JSONArray
        Dim RT As JSONArray
        RT.Values = {}
        Dim str1 As String()
        Dim i1 As Integer
        Dim i2 As Integer
        Dim c As Long
        Dim tstr As String
        c = 0
        i2 = 0
        str1 = str.Split(",")
        For Each strtemp As String In str1
            c += 1
            If InStr(str, strtemp) < i2 Then Continue For
            ReDim Preserve RT.Values(UBound(RT.Values) + 1)
            For i = 1 To Len(strtemp)
                If Mid(strtemp, i, 1) = " " Then Continue For
                Select Case Mid(strtemp, i, 1)
                    Case Chr(34)
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.String
                        tstr = strtemp.Split(Chr(34))(1).Replace("\/", "/").Replace("\n", vbCrLf).Replace("\t", vbTab).Replace("\" & Chr(34), Chr(34)).Replace("\\", "\")
                        If Left(tstr, 1) = Chr(34) Then tstr = Right(tstr, Len(tstr) - 1)
                        If Right(tstr, 1) = Chr(34) Then tstr = Left(tstr, Len(tstr) - 1)
                        RT.Values(UBound(RT.Values)).Strings = tstr
                        i2 = 0
                    Case "{"
                        i1 = Instrs(str, ",", c)
                        i1 = InStr(i1, str, "{") + 1
                        i = i1
                        i2 = i1
                        Do While True
                            i1 = InStr(i1, str, "{")
                            i2 = InStr(i2, str, "}")
                            If i1 = 0 Then Exit Do
                            If i2 = 0 Then MsgBox(language(1)) : Return RT
                            If i2 < i1 Then
                                Exit Do
                            End If
                        Loop
                        i += 1
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.Object
                        If i2 - i = 0 Then
                            RT.Values(UBound(RT.Values)).Objects = Nothing
                        Else
                            RT.Values(UBound(RT.Values)).Objects = AnalysisObject(Mid(str, i, i2 - i))
                        End If
                    Case "["
                        i1 = Instrs(str, ",", c)
                        i1 = InStr(i1, str, "[") + 1
                        i = i1
                        i2 = i1
                        Do While True
                            i1 = InStr(i1, str, "[")
                            i2 = InStr(i2, str, "]")
                            If i1 = 0 Then Exit Do
                            If i2 = 0 Then MsgBox(language(1)) : Return RT
                            If i2 < i1 Then
                                Exit Do
                            End If
                        Loop
                        i += 1
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.Array
                        If i2 - i = 0 Then
                            RT.Values(UBound(RT.Values)).Arrays.Values = {}
                        Else
                            RT.Values(UBound(RT.Values)).Arrays = AnalysisArray(Mid(str, i, i2 - i))
                        End If
                    Case Else
                        RT.Values(UBound(RT.Values)).Type = JSONTYPE.String
                        RT.Values(UBound(RT.Values)).Strings = Mid(strtemp, i)
                        i2 = 0
                End Select
                Exit For
            Next
        Next
        Return RT
    End Function
    Public Overloads Function ToString() As String
        GetJSONStr(JSON)
        Return JSONStr
    End Function
    Public Function GetJSONStr(ByRef JO As JSONObject) As Boolean
        If JSON.Strings.Length = 0 Then Return False
        JSONStr = GetObject(JO)
        Return True
    End Function
    Private Function GetObject(JO As JSONObject) As String
        Dim RT As String
        RT = "{"
        If JO.Strings.Length = 0 Then RT &= "}" : Return RT
        For i = 0 To UBound(JO.Strings)
            RT &= Chr(34) & JO.Strings(i) & Chr(34) & ": "
            Select Case JO.Values(i).Type
                Case JSONTYPE.Array
                    RT &= GetArr(JO.Values(i).Arrays)
                Case JSONTYPE.NULL
                    RT &= ""
                Case JSONTYPE.Object
                    RT &= GetObject(JO.Values(i).Objects)
                Case JSONTYPE.String
                    Dim t As Long
                    Try
                        t = Val(JO.Values(i).Strings)
                    Catch ex As Exception
                        t = 0
                    End Try
                    If JO.Values(i).Strings = t.ToString Then
                        RT &= JO.Values(i).Strings
                    Else
                        RT &= Chr(34) & JO.Values(i).Strings.Replace("\", "\\").Replace("\", "\/").Replace(vbCrLf, "\n").Replace(vbTab, "\t").Replace(Chr(34), "\" & Chr(34)) & Chr(34)
                    End If
            End Select
            If i <> UBound(JO.Strings) Then RT &= ","
        Next
        RT &= "}"
        Return RT
    End Function
    Private Function GetArr(JA As JSONArray) As String
        Dim RT As String
        RT = "["
        If JA.Values.Length = 0 Then RT &= "]" : Return RT
        For i = 0 To UBound(JA.Values)
            Select Case JA.Values(i).Type
                Case JSONTYPE.Array
                    RT &= GetArr(JA.Values(i).Arrays)
                Case JSONTYPE.NULL
                    RT &= ""
                Case JSONTYPE.Object
                    RT &= GetObject(JA.Values(i).Objects)
                Case JSONTYPE.String
                    Try
                        If JA.Values(i).Strings = Val(JA.Values(i).Strings) Then
                            RT &= JA.Values(i).Strings
                        End If
                    Catch ex As Exception
                        RT &= Chr(34) & JA.Values(i).Strings.Replace("\", "\\").Replace("\", "\/").Replace(vbCrLf, "\n").Replace(vbTab, "\t").Replace(Chr(34), "\" & Chr(34)) & Chr(34)
                    End Try
            End Select
            If i <> UBound(JA.Values) Then RT &= ","
        Next
        RT &= "]"
        Return RT
    End Function
End Class
