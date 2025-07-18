namespace Webapiproje.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Tarih { get; set; }
        public int? Employeeid { get; set; }
        public string User { get; set; }
        public string State { get; set; }

        // İlişki varsa 
        public Employee Employee { get; set; }
    }
}
