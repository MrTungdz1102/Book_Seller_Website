using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Models.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookSellerDbContext _context;
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        public IShopingCartRepository ShopingCartRepository { get; private set; }
        public IUserRepository UserRepository { get; private set; }
        public IOrderHeaderRepository OrderHeaderRepository { get; private set; }
        public IOrderDetailRepository OrderDetailRepository { get; private set; }
        public UnitOfWork(BookSellerDbContext context)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(_context);
            ProductRepository = new ProductRepository(_context);
            CompanyRepository = new CompanyRepository(_context);
            ShopingCartRepository = new ShopingCartRepository(_context);
            UserRepository = new UserRepository(_context);
            OrderDetailRepository = new OrderDetailRepository(_context);
            OrderHeaderRepository = new OrderHeaderRepository(_context);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
