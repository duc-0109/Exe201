using SmartCookFinal.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EmailConfirmation
{
    [Key]
    public Guid ConfirmationID { get; set; } = Guid.NewGuid();

    [Required]
    public int Id { get; set; }  // Khóa ngoại trỏ tới NguoiDung.Id

    [Required]
    public string Token { get; set; }

    public DateTime ExpirationTime { get; set; }

    public bool IsConfirmed { get; set; } = false;

    [ForeignKey("Id")]
    public NguoiDung NguoiDung { get; set; }
}