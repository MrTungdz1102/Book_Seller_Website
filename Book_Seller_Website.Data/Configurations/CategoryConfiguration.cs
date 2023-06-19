using Book_Seller_Website.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasData(
               new Category { Id = 1, Name = "Category 1", DisplayOrder = 1, CreatedDateTime = DateTime.Now },
               new Category { Id = 2, Name = "Category 2", DisplayOrder = 2, CreatedDateTime = DateTime.Now }
               );
        }
    }
}
