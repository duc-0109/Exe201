namespace SmartCookFinal.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        // Quan hệ 1-n: Một Category có nhiều News
        public ICollection<News> NewsList { get; set; }
    }
}
