namespace Library.DAL.Entities
{
    public enum BorrowingStatus
    {
        Pending = 0,    // العضو طلب الاستعارة وبانتظار موافقة الموظف
        Borrowed = 1,   // الكتاب خرج مع العضو
        Returned = 2,   // الموظف أكّد الإرجاع
        Rejected = 3,   // الموظف رفض الطلب
        Cancelled = 4   // العضو لغى طلبه
    }

    public enum FineStatus
    {
        None = 0,       // مفيش غرامة
        Unpaid = 1,     // فيه غرامة لسه ما اتدفعتش
        Paid = 2        // اتدفعت كاش عند الموظف
    }
}
