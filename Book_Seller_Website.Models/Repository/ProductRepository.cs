using Book_Seller_Website.Data;
using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Models.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly BookSellerDbContext _context;
        public ProductRepository(BookSellerDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
