using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Models.Interface
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IProductRepository ProductRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        IShopingCartRepository ShopingCartRepository { get; }
        IUserRepository UserRepository { get; }
        void Save();
    }
}
