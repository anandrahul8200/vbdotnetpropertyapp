Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration

''' <summary>
''' Centralized database connection and stored procedure execution helper.
''' All data access goes through this class.
''' </summary>
Public Class DatabaseHelper

    Private Shared ReadOnly _connectionString As String = ConfigurationManager.ConnectionStrings("PropertyInsuranceDB").ConnectionString

    ''' <summary>
    ''' Creates a new SqlConnection instance.
    ''' </summary>
    Public Shared Function GetConnection() As SqlConnection
        Return New SqlConnection(_connectionString)
    End Function

    ''' <summary>
    ''' Executes a stored procedure and returns a single DataTable.
    ''' </summary>
    Public Shared Function ExecuteStoredProcedure(spName As String, params As SqlParameter()) As DataTable
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(spName, conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandTimeout = 120
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)

                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    conn.Open()
                    adapter.Fill(dt)
                End Using
                Return dt
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Executes a stored procedure and returns multiple result sets as a DataSet.
    ''' </summary>
    Public Shared Function ExecuteDataSet(spName As String, params As SqlParameter()) As DataSet
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(spName, conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandTimeout = 120
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)

                Dim ds As New DataSet()
                Using adapter As New SqlDataAdapter(cmd)
                    conn.Open()
                    adapter.Fill(ds)
                End Using
                Return ds
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Executes a stored procedure that modifies data (INSERT/UPDATE/DELETE).
    ''' Returns the number of rows affected.
    ''' </summary>
    Public Shared Function ExecuteNonQuery(spName As String, params As SqlParameter()) As Integer
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(spName, conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandTimeout = 120
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                conn.Open()
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Executes a stored procedure and returns a single scalar value.
    ''' </summary>
    Public Shared Function ExecuteScalar(spName As String, params As SqlParameter()) As Object
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(spName, conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandTimeout = 120
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)
                conn.Open()
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Executes a stored procedure with output parameters.
    ''' Returns the output parameter values via the params array (by reference).
    ''' </summary>
    Public Shared Function ExecuteWithOutput(spName As String, params As SqlParameter()) As DataTable
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(spName, conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.CommandTimeout = 120
                If params IsNot Nothing Then cmd.Parameters.AddRange(params)

                Dim dt As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    conn.Open()
                    adapter.Fill(dt)
                End Using
                Return dt
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Creates a standard SqlParameter.
    ''' </summary>
    Public Shared Function CreateParam(name As String, value As Object) As SqlParameter
        Dim param As New SqlParameter(name, If(value, DBNull.Value))
        Return param
    End Function

    ''' <summary>
    ''' Creates an output SqlParameter.
    ''' </summary>
    Public Shared Function CreateOutputParam(name As String, dbType As SqlDbType) As SqlParameter
        Dim param As New SqlParameter(name, dbType)
        param.Direction = ParameterDirection.Output
        If dbType = SqlDbType.VarChar OrElse dbType = SqlDbType.NVarChar Then
            param.Size = 200
        End If
        Return param
    End Function

End Class
