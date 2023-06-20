using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Book_Seller_Website.Models.Interface
{
	public interface IGenericRepository<T> where T : class
	{
        // dung find chi find duoc id, nen su dung expression , string? includeProperties = null, bool tracked = false
        IEnumerable<T> GetAll();
		Task<T> GetAsync(Expression<Func<T, bool>> filter);
		Task<T> AddAsync(T entity);
		void Delete(T entity);
		void Update(T entity);
		void DeleteRange(IEnumerable<T> entity);
        Task<bool> Exists(int id);
    }
}
