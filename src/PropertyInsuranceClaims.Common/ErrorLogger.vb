Imports System
Imports System.IO

''' <summary>
''' Application-level error logging. Logs to both file and database.
''' </summary>
Public Class ErrorLogger

    Private Shared ReadOnly _logPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
    Private Shared ReadOnly _lockObj As New Object()

    ''' <summary>
    ''' Logs an exception with context information.
    ''' </summary>
    Public Shared Sub LogError(ex As Exception, context As String, Optional additionalInfo As String = Nothing)
        Try
            ' Log to file
            LogToFile(ex, context, additionalInfo)

            ' Log to database (best effort)
            LogToDatabase(ex, context, additionalInfo)
        Catch
            ' Swallow — logging should never crash the app
        End Try
    End Sub

    ''' <summary>
    ''' Logs an error message without an exception object.
    ''' </summary>
    Public Shared Sub LogMessage(message As String, context As String, Optional severity As String = "ERROR")
        Try
            Dim logEntry As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{severity}] [{context}] {message}"
            WriteToFile(logEntry)
        Catch
            ' Swallow
        End Try
    End Sub

    Private Shared Sub LogToFile(ex As Exception, context As String, additionalInfo As String)
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("========================================")
        sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
        sb.AppendLine($"Context: {context}")
        sb.AppendLine($"User: {GlobalState.CurrentUser}")
        sb.AppendLine($"Machine: {Environment.MachineName}")
        If Not String.IsNullOrEmpty(additionalInfo) Then
            sb.AppendLine($"Additional Info: {additionalInfo}")
        End If
        sb.AppendLine($"Exception Type: {ex.GetType().FullName}")
        sb.AppendLine($"Message: {ex.Message}")
        sb.AppendLine($"Stack Trace: {ex.StackTrace}")

        If ex.InnerException IsNot Nothing Then
            sb.AppendLine($"Inner Exception: {ex.InnerException.Message}")
            sb.AppendLine($"Inner Stack: {ex.InnerException.StackTrace}")
        End If

        WriteToFile(sb.ToString())
    End Sub

    Private Shared Sub WriteToFile(content As String)
        SyncLock _lockObj
            If Not Directory.Exists(_logPath) Then
                Directory.CreateDirectory(_logPath)
            End If

            Dim fileName As String = $"ErrorLog_{DateTime.Now:yyyyMMdd}.log"
            Dim filePath As String = Path.Combine(_logPath, fileName)
            File.AppendAllText(filePath, content & Environment.NewLine)
        End SyncLock
    End Sub

    Private Shared Sub LogToDatabase(ex As Exception, context As String, additionalInfo As String)
        Try
            Dim params() As Data.SqlClient.SqlParameter = {
                DatabaseHelper.CreateParam("@ErrorNumber", 0),
                DatabaseHelper.CreateParam("@ErrorSeverity", 16),
                DatabaseHelper.CreateParam("@ErrorState", 1),
                DatabaseHelper.CreateParam("@ErrorProcedure", context),
                DatabaseHelper.CreateParam("@ErrorLine", 0),
                DatabaseHelper.CreateParam("@ErrorMessage", ex.Message),
                DatabaseHelper.CreateParam("@AdditionalInfo", If(additionalInfo, ex.StackTrace))
            }
            DatabaseHelper.ExecuteNonQuery("Admin.usp_ErrorLog_Insert", params)
        Catch
            ' Database logging failed — file log is the fallback
        End Try
    End Sub

End Class
