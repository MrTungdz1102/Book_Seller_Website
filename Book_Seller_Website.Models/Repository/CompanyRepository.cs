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
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly BookSellerDbContext _context;
        public CompanyRepository(BookSellerDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
