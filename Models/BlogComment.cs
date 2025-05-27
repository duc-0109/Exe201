namespace SmartCookFinal.Models
{
    // Models/BlogComment.cs
    public class BlogComment
    {
        public int BlogCommentId { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentedAt { get; set; }

        // Foreign Keys
        public int BlogId { get; set; }
        public int UserId { get; set; }

        // Navigation
        public Blog Blog { get; set; }
        public NguoiDung User { get; set; }
    }

}
