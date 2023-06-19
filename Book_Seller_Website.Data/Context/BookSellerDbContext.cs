using Book_Seller_Website.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Book_Seller_Website.Data.Context
{
    public class BookSellerDbContext : DbContext
    {
        public BookSellerDbContext(DbContextOptions<BookSellerDbContext> options) : base(options)
        {

        }
        
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new Configurations.CategoryConfiguration());
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
