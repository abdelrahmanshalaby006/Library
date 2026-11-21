namespace Library.DAL.Entities
{
    // قواعد المكتبة في مكان واحد. غيّر الرقم هنا وهيتطبق في كل المشروع.
    public static class LibraryRules
    {
        public const int LoanDays = 14;          // مدة الاستعارة (أيام)
        public const decimal FinePerDay = 5m;    // غرامة التأخير عن كل يوم (جنيه)
        public const int MaxOpenLoans = 3;       // أقصى عدد كتب (مستعارة + طلبات معلّقة) للعضو الواحد
        public const string Currency = "EGP";
    }
}
