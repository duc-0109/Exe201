namespace SmartCookFinal.Models
{
    public class News
    {
        public int NewsId { get; set; }
        public string Title { get; set; }
        public string? UrlImage { get; set; }
        public string Content { get; set; }
        public string Detail { get; set; }      
        public DateTime CreatedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Navigation
        public NguoiDung User { get; set; }
    }
}
