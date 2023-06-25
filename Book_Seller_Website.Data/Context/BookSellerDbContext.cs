using Book_Seller_Website.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Book_Seller_Website.Data.Context
{
    public class BookSellerDbContext : IdentityDbContext<IdentityUser>
    {
        public BookSellerDbContext(DbContextOptions<BookSellerDbContext> options) : base(options)
        {

        }
        
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShopingCart> ShopingCarts { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new Configurations.CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ProductConfiguration());
        }

        public class BookSellerDbContextFactory : IDesignTimeDbContextFactory<BookSellerDbContext>
        {
            public BookSellerDbContext CreateDbContext(string[] args)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // setbasepath in package config.json
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var optionsBuilder = new DbContextOptionsBuilder<BookSellerDbContext>();
                var conn = config.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(conn);
                return new BookSellerDbContext(optionsBuilder.Options);
            }
        }

       
        
    }
}
