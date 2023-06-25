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
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
        private readonly BookSellerDbContext _context;
        public OrderDetailRepository(BookSellerDbContext context) : base(context)
        {
            _context = context;
        }

       
    }
}
