'Imports System.IO

'Public Class AccountManager
'    Public Class Account
'        Public Property AccessKey As OAuth.KeyPair
'        Public Property ConsumerKey As OAuth.KeyPair
'    End Class

'    Private Shared _accounts As List(Of Account) = Nothing
'    Public Shared ReadOnly Property Accounts As List(Of Account)
'        Get
'            If _accounts Is Nothing Then
'                AccountManager.Init()
'            End If
'            Return _accounts
'        End Get
'    End Property

'    Shared Sub Init()
'        Dim ls = Windows.Storage.ApplicationData.Current.LocalSettings
'        If ls.Values.ContainsKey("Accounts") Then
'            Dim xml As String = ls.Values("Accounts")
'            Dim xmlserializer As New System.Xml.Serialization.XmlSerializer(GetType(List(Of Account)))
'            Dim strreader As New StringReader(xml)
'            _accounts = xmlserializer.Deserialize(strreader)
'        Else
'            _accounts = New List(Of Account)
'        End If
'    End Sub

'    Shared Sub Add(ByVal a As Account)
'        _accounts.Add(a)
'        Dim ls = Windows.Storage.ApplicationData.Current.LocalSettings
'        Dim xmlserializer As New System.Xml.Serialization.XmlSerializer(_accounts.GetType)
'        Dim strwriter As New StringWriter()
'        xmlserializer.Serialize(strwriter, _accounts)
'        ls.Values("Accounts") = strwriter.ToString()
'    End Sub
'End Class
