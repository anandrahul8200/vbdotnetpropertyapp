''' <summary>
''' Application-wide constants for status codes, types, and lookup values.
''' Mirrors the database lookup values for compile-time safety.
''' </summary>
Public Class Constants

    ' --- Policy Status ---
    Public Class PolicyStatus
        Public Const Quote As String = "QUOTE"
        Public Const Referred As String = "REFERRED"
        Public Const Bound As String = "BOUND"
        Public Const Active As String = "ACTIVE"
        Public Const PendingCancel As String = "PENDING_CANCEL"
        Public Const Cancelled As String = "CANCELLED"
        Public Const Expired As String = "EXPIRED"
        Public Const NonRenewed As String = "NON_RENEWED"
        Public Const Declined As String = "DECLINED"
    End Class

    ' --- Claim Status ---
    Public Class ClaimStatus
        Public Const FNOL As String = "FNOL"
        Public Const Assigned As String = "ASSIGNED"
        Public Const Investigating As String = "INVESTIGATING"
        Public Const Assessed As String = "ASSESSED"
        Public Const Approved As String = "APPROVED"
        Public Const Denied As String = "DENIED"
        Public Const Settled As String = "SETTLED"
        Public Const Closed As String = "CLOSED"
        Public Const Reopened As String = "REOPENED"
        Public Const Litigation As String = "LITIGATION"
    End Class

    ' --- Claim Types ---
    Public Class ClaimType
        Public Const PropertyDamage As String = "PROPERTY_DAMAGE"
        Public Const Theft As String = "THEFT"
        Public Const Liability As String = "LIABILITY"
        Public Const WaterDamage As String = "WATER_DAMAGE"
        Public Const Fire As String = "FIRE"
        Public Const Wind As String = "WIND"
        Public Const Hail As String = "HAIL"
        Public Const Other As String = "OTHER"
    End Class

    ' --- Payment Status ---
    Public Class PaymentStatus
        Public Const Pending As String = "PENDING"
        Public Const Approved As String = "APPROVED"
        Public Const Issued As String = "ISSUED"
        Public Const Cleared As String = "CLEARED"
        Public Const Voided As String = "VOIDED"
        Public Const Stopped As String = "STOPPED"
    End Class

    ' --- Invoice Status ---
    Public Class InvoiceStatus
        Public Const Open As String = "OPEN"
        Public Const Paid As String = "PAID"
        Public Const PartialPaid As String = "PARTIAL"
        Public Const Overdue As String = "OVERDUE"
        Public Const Cancelled As String = "CANCELLED"
        Public Const WrittenOff As String = "WRITTEN_OFF"
    End Class

    ' --- Priority ---
    Public Class Priority
        Public Const Low As String = "LOW"
        Public Const Normal As String = "NORMAL"
        Public Const High As String = "HIGH"
        Public Const Critical As String = "CRITICAL"
    End Class

    ' --- Customer Type ---
    Public Class CustomerType
        Public Const Individual As String = "I"
        Public Const Commercial As String = "C"
    End Class

    ' --- Policy Types ---
    Public Class PolicyType
        Public Const Homeowners As String = "HO3"
        Public Const Renters As String = "HO4"
        Public Const Condo As String = "HO6"
        Public Const DwellingFire As String = "DP3"
        Public Const Commercial As String = "BOP"
    End Class

    ' --- Construction Types ---
    Public Class ConstructionType
        Public Const Frame As String = "FRAME"
        Public Const Masonry As String = "MASONRY"
        Public Const MasonryVeneer As String = "MASONRY_VENEER"
        Public Const FireResistive As String = "FIRE_RESISTIVE"
        Public Const Superior As String = "SUPERIOR"
    End Class

    ' --- Reserve Types ---
    Public Class ReserveType
        Public Const CaseReserve As String = "CASE"
        Public Const Expense As String = "EXPENSE"
        Public Const IBNR As String = "IBNR"
        Public Const Bulk As String = "BULK"
    End Class

    ' --- Activity Types ---
    Public Class ActivityType
        Public Const Note As String = "NOTE"
        Public Const PhoneCall As String = "PHONE_CALL"
        Public Const Email As String = "EMAIL"
        Public Const Inspection As String = "INSPECTION"
        Public Const Document As String = "DOCUMENT"
        Public Const StatusChange As String = "STATUS_CHANGE"
        Public Const Payment As String = "PAYMENT"
        Public Const ReserveChange As String = "RESERVE_CHANGE"
    End Class

    ' --- Number Prefixes ---
    Public Class NumberPrefix
        Public Const Customer As String = "CUS"
        Public Const PropertyPrefix As String = "PRP"
        Public Const Policy As String = "POL"
        Public Const Claim As String = "CLM"
        Public Const Payment As String = "PAY"
        Public Const Invoice As String = "INV"
        Public Const Refund As String = "RFD"
        Public Const Vendor As String = "VND"
    End Class

End Class
