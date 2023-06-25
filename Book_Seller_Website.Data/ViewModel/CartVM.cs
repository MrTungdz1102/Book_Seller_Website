using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Data.ViewModel
{
    public class CartVM
    {
        public IEnumerable<ShopingCart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    //    public double GrandTotal { get; set; } 
    }
}
