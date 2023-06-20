using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IEnumerable<T> GetAll()
		{
            IQueryable<T> query = _dbSet;
			return query.ToList();
		}

        public async Task<T> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter)
		{
            //IQueryable<T> query = _dbSet;
            //     query = query.Where(filter);
            //    return await query.FirstOrDefaultAsync();
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public void Update(T entity)
        {
            _context.Update(entity);
        }
    }
}
