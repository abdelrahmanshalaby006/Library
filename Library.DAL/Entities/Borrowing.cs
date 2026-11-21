namespace Library.DAL.Entities
{
    // سجل عملية استعارة كتاب واحد (من الطلب لحد الإرجاع والغرامة).
    public class Borrowing
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public BorrowingStatus Status { get; set; } = BorrowingStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public DateTime? BorrowedAt { get; set; }   // لحظة موافقة/تسليم الكتاب
        public DateTime? DueDate { get; set; }      // آخر ميعاد للإرجاع
        public DateTime? ReturnedAt { get; set; }

        public decimal FineAmount { get; set; }     // الغرامة النهائية (بتتسجل وقت الإرجاع)
        public FineStatus FineStatus { get; set; } = FineStatus.None;

        // ----- خصائص محسوبة (مش بتتخزن في الداتابيز) -----

        // عدد أيام التأخير (لحد النهارده لو لسه مستعار، أو لحد يوم الإرجاع)
        public int OverdueDays
        {
            get
            {
                if (DueDate == null) return 0;
                if (Status != BorrowingStatus.Borrowed && Status != BorrowingStatus.Returned) return 0;
                var end = ReturnedAt ?? DateTime.Now;
                var days = (end.Date - DueDate.Value.Date).Days;
                return days > 0 ? days : 0;
            }
        }

        public bool IsOverdue => Status == BorrowingStatus.Borrowed && OverdueDays > 0;

        // الغرامة لحد دلوقتي لو الكتاب لسه متأخر
        public decimal CurrentFine => OverdueDays * LibraryRules.FinePerDay;
    }
}
