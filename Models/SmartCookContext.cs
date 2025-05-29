using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using Microsoft.EntityFrameworkCore;

namespace SmartCookFinal.Models
{
	public class SmartCookContext : DbContext
	{
		public DbSet<MonAn> MonAns { get; set; }
		public DbSet<NguyenLieu> NguyenLieus { get; set; }
		public DbSet<MonAnNguyenLieu> MonAnNguyenLieus { get; set; }
		public DbSet<NguoiDung> NguoiDungs { get; set; }
		public DbSet<ThucDonNgay> ThucDonNgays { get; set; }
		public DbSet<ThucDonChiTiet> ThucDonChiTiets { get; set; }
		public DbSet<DanhMucMonAn> DanhMucMonAns { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


        public SmartCookContext(DbContextOptions<SmartCookContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MonAn>()
    .Property(m => m.ChiPhiUocTinh)
    .HasPrecision(18, 2); // hoặc .HasColumnType("decimal(18,2)")

            modelBuilder.Entity<NguoiDung>()
                .Property(n => n.NganSachToiDa)
                .HasPrecision(18, 2);


            // BlogComment → Blog (OK, giữ lại cascade)
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(bc => bc.BlogId)
                .OnDelete(DeleteBehavior.Cascade);

            // BlogComment → User (Không cho cascade)
            modelBuilder.Entity<BlogComment>()
                .HasOne(bc => bc.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.NoAction); // ← KHÔNG DÙNG Restrict nếu vẫn lỗi

            // Blog → User (Không nên để cascade vì nó là gốc sinh ra vòng lặp)
            modelBuilder.Entity<Blog>()
                .HasOne(b => b.User)
                .WithMany(u => u.Blogs)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction); // ← CHUYỂN thành NoAction để chắc chắn
        }

    }
}
