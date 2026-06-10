using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace DataAccessLayer
{
    public class MyDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<AccountMember> AccountMembers { get; set; } = null!;

        private static bool _initialized;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        /// <summary>
        /// Gọi 1 lần lúc app start. Đảm bảo DB tồn tại và có seed.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            using var db = new MyDbContext();
            db.Database.EnsureCreated();
            SeedIfEmpty(db);
            _initialized = true;
        }

        private static void SeedIfEmpty(MyDbContext db)
        {
            if (!db.AccountMembers.Any())
            {
                db.AccountMembers.Add(new AccountMember
                {
                    MemberId = "PS0001",
                    MemberPassword = PasswordHasher.Hash("@1"),
                    FullName = "Admin",
                    EmailAddress = "admin@fpt.edu.vn",
                    MemberRole = Role.Admin
                });
                db.SaveChanges();
            }

            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category { CategoryID = 1, CategoryName = "Beverages" },
                    new Category { CategoryID = 2, CategoryName = "Condiments" },
                    new Category { CategoryID = 3, CategoryName = "Confections" }
                );
                db.SaveChanges();
            }

            if (!db.Products.Any())
            {
                db.Products.AddRange(
                    new Product { ProductName = "Chai", UnitPrice = 18m, UnitsInStock = 39, CategoryId = 1 },
                    new Product { ProductName = "Chang", UnitPrice = 19m, UnitsInStock = 17, CategoryId = 1 },
                    new Product { ProductName = "Aniseed Syrup", UnitPrice = 10m, UnitsInStock = 13, CategoryId = 2 },
                    new Product { ProductName = "Pavlova", UnitPrice = 17.45m, UnitsInStock = 29, CategoryId = 3 }
                );
                db.SaveChanges();
            }
        }

        private static string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
                throw new System.InvalidOperationException("Missing 'DefaultConnection' in appsettings.json");
            return conn;
        }
    }
}
