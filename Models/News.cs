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

        public int ViewCount { get; set; } = 0;


        // Foreign Key
        public int UserId { get; set; }
        public int? CategoryId { get; set; }  // Nullable  // Khóa ngoại đến Category


        // Navigation
        public NguoiDung User { get; set; }

        public Category? Category { get; set; } // Navigation tới Category
    }
}
