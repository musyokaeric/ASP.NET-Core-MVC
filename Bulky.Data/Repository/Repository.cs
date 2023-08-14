using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Data.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext dbContext;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            dbSet = this.dbContext.Set<T>();
        }

        public void Add(T entity) => dbSet.Add(entity);

        public T Get(Expression<Func<T, bool>> expression) => dbSet.Where(expression).FirstOrDefault();

        public IEnumerable<T> GetAll() => dbSet.ToList();

        public void Remove(T entity) => dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => dbSet.RemoveRange(entities);
    }
}
