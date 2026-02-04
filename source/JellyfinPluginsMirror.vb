Imports System
Imports System.Formats
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Mime.MediaTypeNames
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web

Module JellyfinPluginsMirror

    Private Const READ_BUFF As Integer = 1024
    Private Const VERSION_URL As String = "https://github.com/jellyfin/jellyfin/releases/latest"
    Private Const REPO_URL As String = "https://raw.githubusercontent.com/HiranoKohta/mirror-jellyfin-plugins-repo/main"
    Private Const REPO_PATH As String = "H:\GitHub\mirror-jellyfin-plugins-repo"
    Private Const REPO_LIST As String = "repo-list.txt"
    Private Const MIRROR_LOG As String = "mirror-log.html"
    Private Const PLUGINS_PATH As String = REPO_PATH & "\files"
    Private Const OFFICIAL_REPO As String = "repo.jellyfin.org"

    Private JellyfinVer As String
    Private Urls() As String
    Private Plugins() As Plugin
    Private OfficialVersion As Integer = 0
    Private UnOfficialVersion As Integer = 0
    Private IsOfficialRepository As Boolean
    Private IsUnOfficialRepository As Boolean

    Private Structure Plugin
        Public Artifacts As String
        Public Category As String
        Public Description As String
        Public Guid As String
        Public ImageUrl As String
        Public Name As String
        Public Overview As String
        Public Owner As String
        Public Versions() As Ver
    End Structure

    Private Structure Ver
        Public ChangeLog As String
        Public CheckSum As String
        Public Dependencies As String
        Public SourceUrl As String
        Public TargetAbi As String
        Public TimeStamp As String
        Public Version As String
    End Structure

    Private Enum Repositori As Integer
        All = 0
        Official = 1
        UnOffical = 2
    End Enum

    '========================================
    'Принудительная установка версии Jellyfin
    '----------------------------------------
    'JellyfinPluginsMirror.exe /v 10.10.0
    '========================================
    Sub Main()
        Dim StringLog As String, FileName As String, Url As String
        Dim LogFile As String = Path.Combine(REPO_PATH, MIRROR_LOG)
        Dim Arg() As String = Split(Command(), "/")
        IO.File.Delete(LogFile)
        For Each A In Arg
            Select Case True
                Case A.ToUpper.StartsWith("V")
                    JellyfinVer = A.Substring(1).Trim
            End Select
        Next
        Console.Clear()
        Console.CursorVisible = False
        Console.Title = "Jellyfin Plugins Mirror"
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        Call OutLog(vbNullString, False)
        '====================
        StringLog = String.Format("Start updating In {0}", Format(Now, "yyyy-MM-dd HH:mm:ss"))
        Console.WriteLine(StringLog)
        Call OutLog(StringLog)
        '====================
        FileName = Path.Combine(Path.GetTempPath, "LastVersion.html")
        Url = VERSION_URL
        'Запрос текущей версии Jellyfin
        Call DownloadFile(Url, FileName)
        FileName = Path.Combine(Path.GetTempPath, REPO_LIST)
        '====================
        StringLog = String.Format("Jellyfin v{0}", JellyfinVer)
        Console.WriteLine(StringLog)
        Call OutLog(StringLog)
        '====================
        ' Загрузка списка репозиториев
        '====================
        StringLog = String.Format(vbCrLf & "Loading list from {0}", Url)
        Console.WriteLine(StringLog)
        Call OutLog(StringLog)
        '====================
        Url = REPO_URL & "/" & REPO_LIST
        If DownloadFile(Url, FileName) Then
                Urls = IO.File.ReadAllLines(FileName, System.Text.Encoding.UTF8)
                Console.ForegroundColor = ConsoleColor.White
                '====================
                StringLog = String.Format("Loading {0} repository.", Urls.Length)
                Console.WriteLine(StringLog)
                Call OutLog(StringLog)
                '====================
                Call Start()
                StringLog = vbCrLf
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine(StringLog)
                Call OutLog(StringLog)

                If IsOfficialRepository Then
                    FileName = "all-official-plugins.json"
                    Call CreateManifest(Repositori.Official, False, FileName)
                    StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                If IsUnOfficialRepository Then
                    FileName = "all-3rd-party-plugin.json"
                Call CreateManifest(Repositori.UnOffical, False, FileName)
                StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                If IsOfficialRepository And IsUnOfficialRepository Then
                    FileName = "all-in-one-plugins.json"
                Call CreateManifest(Repositori.All, False, FileName)
                StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                If OfficialVersion > 0 Then
                    FileName = "mirror-all-official-plugins.json"
                Call CreateManifest(Repositori.Official, True, FileName)
                StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                If UnOfficialVersion > 0 Then
                    FileName = "mirror-all-3rd-party-plugin.json"
                Call CreateManifest(Repositori.UnOffical, True, FileName)
                StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                If OfficialVersion > 0 And UnOfficialVersion > 0 Then
                    FileName = "mirror-all-in-one-plugins.json"
                Call CreateManifest(Repositori.All, True, FileName)
                StringLog = String.Format("Create {0}", FileName)
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                End If

                StringLog = String.Format(vbCrLf & "Update plugins mirror completed.")
                Console.WriteLine(StringLog)
                Call OutLog(StringLog)
            Else
                Console.ForegroundColor = ConsoleColor.Red
                '====================
                StringLog = String.Format(vbCrLf & "Failed download! Continuation is impossible.")
                Console.WriteLine(StringLog)
                Call OutLog(StringLog)
            End If
            Console.ForegroundColor = ConsoleColor.White
            StringLog = String.Format("Finish updating in {0}", Format(Now, "yyyy-MM-dd HH:mm:ss"))
        Console.WriteLine(StringLog)
        Call OutLog(StringLog)
        Console.ResetColor()
        Call OutLog(vbNullString, False)
        Console.WriteLine(vbCrLf & "Press any key to close windows...")
        Console.ReadKey()
    End Sub

    Private Sub Start()
        Dim UrlPlugin As String, Maniafest As String, StringLog As String
        Dim I As Integer
        Dim JsonArray() As String
        ReDim Plugins(-1)
        Maniafest = Path.Combine(Path.GetTempPath, "manifest.json")
        '===== Для теста репозитория =====
        'Urls = {"https://intro-skipper.org/manifest.json"}
        '=================================
        For I = 0 To Urls.Length - 1
            UrlPlugin = Urls(I)
            If UrlPlugin.Length > 0 Then
                If DownloadFile(UrlPlugin, Maniafest) Then
                    If UrlPlugin.Contains(OFFICIAL_REPO) Then
                        IsOfficialRepository = True
                    Else
                        IsUnOfficialRepository = True
                    End If
                    Console.ForegroundColor = ConsoleColor.Green
                    '====================
                    StringLog = String.Format(vbCrLf & vbCrLf & "{0}. {1} - {2}", I + 1, UrlPlugin, "OK.")
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                    '====================
                    JsonArray = IO.File.ReadAllLines(Maniafest, System.Text.Encoding.UTF8)
                    Call GetPlugins(JsonArray)
                    File.Delete(Maniafest)
                Else
                    Console.ForegroundColor = ConsoleColor.Red
                    '====================
                    StringLog = String.Format(vbCrLf & vbCrLf & "{0}. {1} - {2}", I + 1, UrlPlugin, "Failed download!")
                    Console.WriteLine(StringLog)
                    Call OutLog(StringLog)
                    '====================
                End If
            End If
        Next I
    End Sub

    Private Sub GetPlugins(ByVal JsonArray() As String)
        Dim S As Integer, E As Integer, I As Integer = -1, J As Integer = -1
        Dim Item() As String
        Dim IsVersion As Boolean = False, IsMultiLines As Boolean = False
        Dim PathName As String, Url As String, FileName As String, StringLog As String, Message As String = "", PropertyName As String = ""
        Dim Version As Ver
        For Each ArrayString In JsonArray
            If ArrayString.Trim.Length > 0 Then
                ArrayString = Replace(ArrayString, ":", vbNullChar,, 1)
                Item = Split(ArrayString.Trim, vbNullChar)
                If Item.Length > 1 Then
                    S = Item(0).IndexOf(Chr(34)) + 1
                    E = Item(0).LastIndexOf(Chr(34)) - 1
                    Item(0) = Item(0).Substring(S, E - S + 1).ToLower
                    If Item(0) <> "versions" Then
                        If Item(0) = "dependencies" Or Item(0) = "artifacts" Then
                            PropertyName = Item(0)
                            IsMultiLines = True
                        End If
                        S = Item(1).IndexOf(Chr(34)) + 1
                        If S > 0 Then
                            E = Item(1).LastIndexOf(Chr(34)) - 1
                            Item(1) = Item(1).Substring(S, E - S + 1)
                            If IsVersion Then
                                'Version
                                Select Case Item(0)
                                    Case "changelist"
                                        Plugins(I).Versions(J).ChangeLog = Item(1)
                                    Case "changelog"
                                        Plugins(I).Versions(J).ChangeLog = Item(1)
                                    Case "checksum"
                                        Plugins(I).Versions(J).CheckSum = Item(1).ToLower
                                    Case "sourceurl"
                                        Plugins(I).Versions(J).SourceUrl = Item(1)
                                        If Item(1).Contains(OFFICIAL_REPO) Then
                                            OfficialVersion += 1
                                        Else
                                            UnOfficialVersion += 1
                                        End If
                                    Case "targetabi"
                                        Plugins(I).Versions(J).TargetAbi = Item(1)
                                    Case "timestamp"
                                        Plugins(I).Versions(J).TimeStamp = Item(1)
                                    Case "version"
                                        Plugins(I).Versions(J).Version = Item(1)
                                End Select
                            Else
                                'Header
                                Select Case Item(0)
                                    Case "category"
                                        Plugins(I).Category = Item(1)
                                    Case "description"
                                        Plugins(I).Description = Item(1)
                                    Case "guid"
                                        Plugins(I).Guid = Item(1).ToLower
                                    Case "imageurl"
                                        Plugins(I).ImageUrl = Item(1)
                                    Case "name"
                                        Plugins(I).Name = Item(1)
                                    Case "overview"
                                        Plugins(I).Overview = Item(1)
                                    Case "owner"
                                        Plugins(I).Owner = Item(1)
                                End Select
                            End If
                        End If
                    Else
                        IsVersion = True
                        Console.ForegroundColor = ConsoleColor.Cyan
                        '====================
                        StringLog = String.Format(vbCrLf & vbTab & "{0}: {1}", Plugins(I).Owner, Plugins(I).Name)
                        Console.WriteLine(StringLog)
                        Call OutLog(StringLog)
                        '====================
                        PathName = Path.Combine(PLUGINS_PATH, Plugins(I).Owner, Plugins(I).Name)
                        Directory.CreateDirectory(PathName)
                        If Not IsNothing(Plugins(I).ImageUrl) Then
                            If Plugins(I).ImageUrl.Trim.Length > 0 Then
                                Url = Plugins(I).ImageUrl
                                FileName = Path.Combine(PathName, Path.GetFileName(GetFileName(Url)))
                                If Not File.Exists(FileName) Then
                                    If DownloadFile(Url, FileName) Then
                                        Console.ForegroundColor = ConsoleColor.Green
                                        Message = "OK."
                                    Else
                                        Console.ForegroundColor = ConsoleColor.Red
                                        Message = "Failed download!"
                                    End If
                                    '====================
                                    StringLog = String.Format(vbTab & vbTab & "{0} - {1}", Url, Message)
                                    Console.WriteLine(StringLog)
                                    Call OutLog(StringLog)
                                    '====================
                                End If
                            End If
                        End If
                        '=============================
                    End If
                Else
                    If Item(0) = "{" Then
                        If IsVersion Then
                            J = Plugins(I).Versions.Length
                            ReDim Preserve Plugins(I).Versions(J)
                        Else
                            I = Plugins.Length
                            ReDim Preserve Plugins(I)
                            ReDim Plugins(I).Versions(-1)
                        End If
                    Else
                        If IsMultiLines Then
                            If Item(0).StartsWith("]") Then
                                IsMultiLines = False
                            Else
                                S = Item(0).IndexOf(Chr(34)) + 1
                                E = Item(0).LastIndexOf(Chr(34)) - 1
                                Item(0) = Item(0).Substring(S, E - S + 1)
                                Select Case PropertyName
                                    Case "artifacts"
                                        Plugins(I).Artifacts = Item(0)
                                    Case "dependencies"
                                        Plugins(I).Versions(J).Dependencies = Item(0)
                                End Select
                            End If
                        Else
                            If Item(0) = "]" Then
                                '=============================
                                If IsVersion Then
                                    PathName = Path.Combine(PLUGINS_PATH, Plugins(I).Owner, Plugins(I).Name)
                                    For Each Version In Plugins(I).Versions
                                        If Not IsNothing(Version.SourceUrl) Then
                                            If Version.SourceUrl.Trim.Length > 0 Then
                                                Directory.CreateDirectory(Path.Combine(PathName, Version.Version))
                                                FileName = Path.Combine(PathName, Version.Version, Path.GetFileName(GetFileName(Version.SourceUrl)))
                                                If File.Exists(FileName) Then
                                                    If GetMd5Hash(FileName) = Version.CheckSum Then
                                                        Console.ForegroundColor = ConsoleColor.Green
                                                        Message = "OK."
                                                    Else
                                                        File.Delete(FileName)
                                                    End If
                                                End If
                                                If Not File.Exists(FileName) Then
                                                    If DownloadFile(Version.SourceUrl, FileName) Then
                                                        If GetMd5Hash(FileName) <> Version.CheckSum Then
                                                            Console.ForegroundColor = ConsoleColor.Yellow
                                                            Message = "Error MD5!"
                                                        Else
                                                            Console.ForegroundColor = ConsoleColor.Green
                                                            Message = "OK."
                                                        End If
                                                    Else
                                                        Console.ForegroundColor = ConsoleColor.Red
                                                        Message = "Failed download!"
                                                    End If
                                                End If
                                                '====================
                                                StringLog = String.Format(vbTab & vbTab & "{0} - {1}", Version.SourceUrl, Message)
                                                Console.WriteLine(StringLog)
                                                Call OutLog(StringLog)
                                                '====================
                                            End If
                                        End If
                                    Next
                                End If
                                '=============================
                                IsVersion = False
                            End If
                        End If
                    End If
                End If
            End If
        Next
    End Sub

    Private Function GetFileName(ByVal Url As String) As String
        Dim I As Integer
        I = Url.IndexOf("?")
        If I > -1 Then
            Url = Url.Substring(0, I)
        End If
        GetFileName = Url
    End Function

    Private Function DownloadFile(ByVal Url As String, ByVal FileName As String) As Boolean
        Dim iBytesRead As Integer
        Dim L As Long, D As Long = 0
        Dim P As Single = 0
        Dim ResponseUrl As String = Url
        Dim FileStreamer As FileStream
        Dim MyHttpWebRequest As HttpWebRequest
        Dim MyHttpWebResponse As HttpWebResponse
        Dim bBuffer(READ_BUFF - 1) As Byte
        DownloadFile = False
        FileStreamer = New FileStream(FileName, IO.FileMode.Create)
        Call ConsoleProgressBar(0)
        Try
            Do
                ' Запрос клиента
                Url = ResponseUrl
                MyHttpWebRequest = HttpWebRequest.Create(Url)
                MyHttpWebRequest.UserAgent = "Jellyfin-Server/" & JellyfinVer
                MyHttpWebRequest.Accept = "*/*"
                MyHttpWebRequest.ContinueTimeout = 5000
                MyHttpWebRequest.Timeout = 5000
                MyHttpWebRequest.ReadWriteTimeout = 5000
                MyHttpWebResponse = MyHttpWebRequest.GetResponse()
                ResponseUrl = MyHttpWebResponse.ResponseUri.ToString
                L = MyHttpWebResponse.ContentLength
            Loop Until Url = ResponseUrl
            ' Ответ сервера
            'Если редирект и версия неопределена, значит это запрос версии
            If IsNothing(JellyfinVer) Then
                JellyfinVer = RegularExpressions.Regex.Match(MyHttpWebResponse.ResponseUri.ToString, "v(\d+\.\d+)\.\d+").Groups(1).Value
            End If
            'Загрузка файла
            Using reader As New BinaryReader(MyHttpWebResponse.GetResponseStream())
                Do
                    ' Используем чтение потока данных
                    iBytesRead = reader.Read(bBuffer, 0, READ_BUFF)
                    ' Записываем полученный блок в файл
                    FileStreamer.Write(bBuffer, 0, iBytesRead)
                    D += iBytesRead
                    P = D / L * 100
                    '==============================
                    'ПрогрессБар
                    Call ConsoleProgressBar(P)
                    '==============================
                Loop Until iBytesRead = 0
            End Using
            Call ConsoleProgressBar(100)
            DownloadFile = True
            FileStreamer.Close()
        Catch ex As Exception
            FileStreamer.Close()
            IO.File.Delete(FileName)
        End Try
        Call ConsoleProgressBar(101)
        FileStreamer.Dispose()
    End Function

    Private Function GetMd5Hash(ByVal FilePath As String) As String
        Dim MD5Hash As MD5 = MD5.Create()
        Dim File() As Byte = System.IO.File.ReadAllBytes(FilePath)
        Dim ByteHash() As Byte = MD5Hash.ComputeHash(File)
        Dim sb As New StringBuilder()
        Array.ForEach(ByteHash, Function(x) sb.Append(x.ToString("x2")))
        Return sb.ToString
    End Function

    Private Sub OutLog(ByVal StringLog As String, Optional ByVal IsBody As Boolean = True)
        Dim LogFile As String = Path.Combine(REPO_PATH, MIRROR_LOG)
        Dim objWrite As System.IO.StreamWriter
        objWrite = New System.IO.StreamWriter(LogFile, True, System.Text.Encoding.GetEncoding("UTF-8"))
        Dim Size As String = Console.CursorSize * 0.64
        If IsBody Then
            StringLog = Replace(StringLog, vbCrLf, "<br>")
            StringLog = Replace(StringLog, vbTab, "&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;&emsp;")
            objWrite.WriteLine("  <samp style='color: {0}'>{1}</samp><br>", GetConsoleColor, StringLog)
        Else
            If FileLen(LogFile) = 0 Then
                objWrite.WriteLine("<html xmlns='http://www.w3.org/1999/xhtml' xml:lang='ru' lang='ru'>")
                objWrite.WriteLine("  <head>")
                objWrite.WriteLine("    <title>Jellyfin Plugins Mirror Log</title>")
                objWrite.WriteLine("    <meta http-equiv='Content-Type' content='text/html; charset=utf-8'/>")
                objWrite.WriteLine("  </head>")
                objWrite.WriteLine("  <body style='color: white; background: black; font-size: {0}px'>", Size.Replace(",", "."))
            Else
                objWrite.WriteLine("  </body>")
                objWrite.WriteLine("</html>")
            End If
        End If
        If Console.ForegroundColor = ConsoleColor.Red Then
            Console.Beep()
        End If
        objWrite.Close()
    End Sub

    Private Function GetConsoleColor() As String
        GetConsoleColor = Choose(Console.ForegroundColor, "#0037DA", "#13A10E", "#3A96DD", "#C50F1F", "#881798", "#C19C00", "#CCCCCC", "#767676", "#3B78FF", "#16C60C", "#61D6D6", "#E74856", "#B4009E", "#F9F1A5", "#FFFFFF")
    End Function

    Private Sub CreateManifest(ByVal Rep As Repositori, ByVal IsMirror As Boolean, ByVal JsonFileName As String, Optional ByVal R As Object = Nothing)
        Dim CountVersion As Integer = 0, TabCount As Integer = 1, Tab As Integer = 2, TotalVersion As Integer
        Dim Url As String, PathName As String, FileName As String
        Dim Json As String = "[" & vbCrLf
        Dim P As Single = 0
        Call ConsoleProgressBar(0)
        For Each Plugin In Plugins
            Select Case Rep
                Case Repositori.Official
                    If Not Plugin.Versions(0).SourceUrl.Contains(OFFICIAL_REPO) Then
                        Continue For
                    End If
                    TotalVersion = OfficialVersion
                Case Repositori.UnOffical
                    If Plugin.Versions(0).SourceUrl.Contains(OFFICIAL_REPO) Then
                        Continue For
                    End If
                    TotalVersion = UnOfficialVersion
                Case Else
                    TotalVersion = OfficialVersion + UnOfficialVersion
            End Select
            Json = Json & Space(Tab * TabCount) & "{" & vbCrLf
            TabCount = TabCount + 1
            Json = Json & Space(Tab * TabCount) & GetQuote("name") & ": " & GetQuote(Plugin.Name) & "," & vbCrLf
            Json = Json & Space(Tab * TabCount) & GetQuote("guid") & ": " & GetQuote(Plugin.Guid) & "," & vbCrLf
            Json = Json & Space(Tab * TabCount) & GetQuote("category") & ": " & GetQuote(Plugin.Category) & "," & vbCrLf
            Json = Json & Space(Tab * TabCount) & GetQuote("description") & ": " & GetQuote(Plugin.Description) & "," & vbCrLf
            Json = Json & Space(Tab * TabCount) & GetQuote("overview") & ": " & GetQuote(Plugin.Overview) & "," & vbCrLf
            Json = Json & Space(Tab * TabCount) & GetQuote("owner") & ": " & GetQuote(Plugin.Owner) & "," & vbCrLf
            If Not IsNothing(Plugin.Artifacts) Then
                If Plugin.Artifacts.Trim.Length > 0 Then
                    Json = Json & Space(Tab * TabCount) & GetQuote("artifacts") & ": [" & vbCrLf
                    TabCount = TabCount + 2
                    Json = Json & Space(Tab * TabCount) & GetQuote(Plugin.Artifacts) & vbCrLf
                    TabCount = TabCount - 1
                    Json = Json & Space(Tab * TabCount) & "]," & vbCrLf
                    TabCount = TabCount - 1
                End If
            End If
            If Not IsNothing(Plugin.ImageUrl) Then
                If Plugin.ImageUrl.Trim.Length > 0 Then
                    '===================================
                    If IsMirror Then
                        Url = Join({REPO_URL, "files", HttpUtility.UrlPathEncode(Plugin.Owner), HttpUtility.UrlPathEncode(Plugin.Name), HttpUtility.UrlPathEncode(Path.GetFileName(GetFileName(Plugin.ImageUrl)))}, "/")
                    Else
                        Url = Plugin.ImageUrl
                    End If
                    Json = Json & Space(Tab * TabCount) & GetQuote("imageUrl") & ": " & GetQuote(Url) & "," & vbCrLf
                End If
            End If
            Json = Json & Space(Tab * TabCount) & GetQuote("versions") & ": [" & vbCrLf
            TabCount = TabCount + 1
            For Each Version In Plugin.Versions
                PathName = Path.Combine(PLUGINS_PATH, Plugin.Owner, Plugin.Name)
                FileName = Path.Combine(PathName, Version.Version, Path.GetFileName(GetFileName(Version.SourceUrl)))
                If File.Exists(FileName) Then
                    Json = Json & Space(Tab * TabCount) & "{" & vbCrLf
                    TabCount = TabCount + 1
                    Json = Json & Space(Tab * TabCount) & GetQuote("version") & ": " & GetQuote(Version.Version) & "," & vbCrLf
                    Json = Json & Space(Tab * TabCount) & GetQuote("checksum") & ": " & GetQuote(Version.CheckSum) & "," & vbCrLf
                    Json = Json & Space(Tab * TabCount) & GetQuote("changelog") & ": " & GetQuote(Version.ChangeLog) & "," & vbCrLf
                    Json = Json & Space(Tab * TabCount) & GetQuote("targetAbi") & ": " & GetQuote(Version.TargetAbi) & "," & vbCrLf
                    '===================================
                    If IsMirror Then
                        Url = Join({REPO_URL, "files", HttpUtility.UrlPathEncode(Plugin.Owner), HttpUtility.UrlPathEncode(Plugin.Name), HttpUtility.UrlPathEncode(Version.Version), HttpUtility.UrlPathEncode(Path.GetFileName(GetFileName(Version.SourceUrl)))}, "/")
                    Else
                        Url = Version.SourceUrl
                    End If
                    Json = Json & Space(Tab * TabCount) & GetQuote("sourceUrl") & ": " & GetQuote(Url) & "," & vbCrLf
                    If Not IsNothing(Version.Dependencies) Then
                        If Version.Dependencies.Trim.Length > 0 Then
                            Json = Json & Space(Tab * TabCount) & GetQuote("dependencies") & ": [" & vbCrLf
                            TabCount = TabCount + 2
                            Json = Json & Space(Tab * TabCount) & GetQuote(Version.Dependencies) & vbCrLf
                            TabCount = TabCount - 1
                            Json = Json & Space(Tab * TabCount) & "]," & vbCrLf
                            TabCount = TabCount - 1
                        End If
                    End If
                    Json = Json & Space(Tab * TabCount) & GetQuote("timestamp") & ": " & GetQuote(Version.TimeStamp) & vbCrLf
                    TabCount = TabCount - 1
                    Json = Json & Space(Tab * TabCount) & "}," & vbCrLf
                End If
                CountVersion += 1
                P = CountVersion / TotalVersion * 100
                Call ConsoleProgressBar(P)
            Next
            Json = Json.Substring(0, Json.Length - 3) & vbCrLf
            TabCount = TabCount - 1
            Json = Json & Space(Tab * TabCount) & "]" & vbCrLf
            TabCount = TabCount - 1
            Json = Json & Space(Tab * TabCount) & "}," & vbCrLf
        Next
        Call ConsoleProgressBar(100)
        Json = Json.Substring(0, Json.Length - 3) & vbCrLf
        TabCount = TabCount - 1
        Json = Json & Space(Tab * TabCount) & "]" & vbCrLf
        IO.File.WriteAllText(Path.Combine(REPO_PATH, JsonFileName), Json, System.Text.Encoding.UTF8)
        Call ConsoleProgressBar(101)
    End Sub

    Private Function GetQuote(ByVal Text As String) As String
        GetQuote = Chr(34) & Text & Chr(34)
    End Function

    Private Sub ConsoleProgressBar(ByVal Value As Single, Optional ByVal Color As ConsoleColor = ConsoleColor.Gray)
        Dim CursorRowPosition As Integer = Console.CursorTop
        Dim BarWidth As Integer = Console.WindowWidth - 5
        Dim CurrentColor As ConsoleColor = Console.ForegroundColor
        Dim Bar As String
        If Not Value < 0 Then
            Value = Value / 100
            If Value > 1 Then
                Bar = Space(Console.WindowWidth)
            Else
                Bar = StrDup(CInt(Value * BarWidth), "▓") & StrDup(BarWidth - CInt(Value * BarWidth), "░") & Format(Value, "0%").PadLeft(5)
            End If
            Console.ForegroundColor = Color
            Console.Write(Bar)
            Console.SetCursorPosition(0, CursorRowPosition)
            Console.ForegroundColor = CurrentColor
        End If
    End Sub

End Module
