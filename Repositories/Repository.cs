using APICatalogo.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    // Classe genérica
    public class Repository<T> : IRepository<T> where T : class // T é um tipo genérico e deve ser uma classe
    {
        protected readonly AppDbContext _context;

        public Repository(AppDbContext contexto)
        {
            _context = contexto;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Set<T> é um método genérico que é usado para acessar a entidade do contexto
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public T Create(T entity)
        {
            _context.Set<T>().Add(entity);
            //_context.SaveChanges();
            return entity;
        }

        public T Update(T entity)
        {
            // _context.Entry(entity).State = EntityState.Modified;
            _context.Set<T>().Update(entity);
            //_context.SaveChanges();
            return entity;
        }

        public T Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            //_context.SaveChanges();
            return entity;
        }

    }
}
