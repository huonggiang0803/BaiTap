using Microsoft.EntityFrameworkCore;
using WebApi.Models.Entities;
using WebApp.Models.DTOs;

namespace WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<MasterProduct> MasterProducts { get; set; }
        public virtual DbSet<SaleOutReportDto> SaleOutReport { get; set; }

        public virtual DbSet<SaleOut> SaleOuts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ n-1
            modelBuilder.Entity<SaleOut>()
                .HasOne(s => s.Product)
                .WithMany(p => p.SaleOuts)
                .HasForeignKey(s => s.ProductId);
            modelBuilder.Entity<SaleOutReportDto>().HasNoKey();
            base.OnModelCreating(modelBuilder);

        }
    }
}
