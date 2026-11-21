namespace Library.BLL.Common
{
    // نتيجة أي عملية في الـ Services: نجحت ولا لأ + رسالة تتعرض للمستخدم
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ServiceResult Ok(string message = "") => new ServiceResult { Success = true, Message = message };
        public static ServiceResult Fail(string message) => new ServiceResult { Success = false, Message = message };
    }
}
