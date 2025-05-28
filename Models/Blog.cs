namespace SmartCookFinal.Models
{
    // Models/Blog.cs
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string? UrlImage { get; set; }
        public string Content { get; set; }
        public string Detail {  get; set; }
        public bool isChecked { get; set; }
        public DateTime CreatedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Navigation
        public NguoiDung User { get; set; }
        public ICollection<BlogComment> Comments { get; set; }
    }

}
