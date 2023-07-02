using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Data
{
	public class ProductImage
	{
		public int Id { get; set; }
		[Required]
		public string Image { get; set; }
		public int ProductId { get; set; }
		[ForeignKey("ProductId")]
		public Product Product { get; set; }
	}
}
