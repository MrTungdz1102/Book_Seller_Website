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
	public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
	{
		public ProductImageRepository(BookSellerDbContext context) : base(context)
		{
		}
	}
}
