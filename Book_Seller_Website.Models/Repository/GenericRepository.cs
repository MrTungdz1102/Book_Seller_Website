using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Book_Seller_Website.Models.Repository
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private readonly BookSellerDbContext _context;
		internal DbSet<T> _dbSet;
        // dùng _dbSet de rut gon code _context.Set<T>().
        public GenericRepository(BookSellerDbContext context)
		{
            _context = context;
            _dbSet = _context.Set<T>();
            _context.Products.Include(u => u.Category);
        }
		public async Task<T> AddAsync(T entity)
		{
			await _context.AddAsync(entity);
            return entity;
        }

		public void Delete(T entity)
		{		
			_dbSet.Remove(entity);
        }

		public void DeleteRange(IEnumerable<T> entity)
		{
			_dbSet.RemoveRange(entity);
		}

        public async Task<bool> Exists(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }

        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            // include voi nhieu dieu kien
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
			return query.ToList();
		}

        public async Task<T> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter, string? includeProperties = null)
		{
            IQueryable<T> query = _dbSet;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return await query.FirstOrDefaultAsync(filter);
        }

        public void Update(T entity)
        {
            _context.Update(entity);
        }
    }
}
