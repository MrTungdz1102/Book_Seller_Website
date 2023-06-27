using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
			// khong can may cai nay
			//	_context.Products.Include(u => u.Category); 
			//    _context.ShopingCarts.Include(u => u.Product);
			//	_context.OrderHeaders.Include(u => u.User);
		}
		public async Task<T> AddAsync(T entity)
		{
			await _context.AddAsync(entity);
			return entity;
		}
		public void Add(T entity)
		{
			_dbSet.Add(entity);
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

		public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
		{
			IQueryable<T> query;
			if (tracked)
			{
				query = _dbSet;
			}
			else
			{
				query = _dbSet.AsNoTracking();
			}
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var item in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return query.SingleOrDefault(filter);
		}

		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null)
		{
			IQueryable<T> query = _dbSet;
            if(filter != null)
			{
				query = query.Where(filter);
			}
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
			_dbSet.Update(entity);
		}
       
    }
}
