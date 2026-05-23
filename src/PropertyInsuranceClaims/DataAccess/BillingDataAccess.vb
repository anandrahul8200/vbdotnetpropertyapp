Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Billing operations.
''' </summary>
Public Class BillingDataAccess

    Public Shared Sub GenerateInvoice(policyID As Integer, invoiceType As String, premiumAmount As Decimal,
                                      taxAmount As Decimal, feeAmount As Decimal, surchargeAmount As Decimal)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@InvoiceType", invoiceType),
            DatabaseHelper.CreateParam("@PremiumAmount", premiumAmount),
            DatabaseHelper.CreateParam("@TaxAmount", taxAmount),
            DatabaseHelper.CreateParam("@FeeAmount", feeAmount),
            DatabaseHelper.CreateParam("@SurchargeAmount", surchargeAmount),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Billing.usp_Invoice_Generate", params)
    End Sub

    Public Shared Function RecordPayment(policyID As Integer, amount As Decimal, paymentMethod As String,
                                         Optional invoiceID As Integer? = Nothing,
                                         Optional referenceNumber As String = Nothing,
                                         Optional checkNumber As String = Nothing) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@InvoiceID", invoiceID),
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@Amount", amount),
            DatabaseHelper.CreateParam("@PaymentMethod", paymentMethod),
            DatabaseHelper.CreateParam("@ReferenceNumber", referenceNumber),
            DatabaseHelper.CreateParam("@CheckNumber", checkNumber),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim paymentIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PremiumPaymentID", SqlDbType.Int)
        Dim paymentNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PaymentNumber", SqlDbType.VarChar)
        params.Add(paymentIDParam)
        params.Add(paymentNumberParam)

        DatabaseHelper.ExecuteNonQuery("Billing.usp_Payment_Record", params.ToArray())
        Return CInt(paymentIDParam.Value)
    End Function

    Public Shared Function GetBillingByPolicy(policyID As Integer) As DataSet
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@PolicyID", policyID)
        }
        Return DatabaseHelper.ExecuteDataSet("Billing.usp_Billing_GetByPolicy", params)
    End Function

    Public Shared Function CreateRefund(policyID As Integer, refundType As String, amount As Decimal,
                                        Optional calculationMethod As String = "PRO_RATA") As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@RefundType", refundType),
            DatabaseHelper.CreateParam("@Amount", amount),
            DatabaseHelper.CreateParam("@CalculationMethod", calculationMethod),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim refundIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@RefundID", SqlDbType.Int)
        Dim refundNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@RefundNumber", SqlDbType.VarChar)
        params.Add(refundIDParam)
        params.Add(refundNumberParam)

        DatabaseHelper.ExecuteNonQuery("Billing.usp_Refund_Create", params.ToArray())
        Return CInt(refundIDParam.Value)
    End Function

    Public Shared Function GetCommissionStatement(agentID As Integer, Optional periodFrom As Date? = Nothing,
                                                   Optional periodTo As Date? = Nothing) As DataSet
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@AgentID", agentID),
            DatabaseHelper.CreateParam("@PeriodFrom", periodFrom),
            DatabaseHelper.CreateParam("@PeriodTo", periodTo)
        }
        Return DatabaseHelper.ExecuteDataSet("Billing.usp_Commission_GetStatement", params)
    End Function

End Class
