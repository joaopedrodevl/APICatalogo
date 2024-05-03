using System.Linq.Expressions;

namespace APICatalogo.Repositories
{
    public interface IRepository<T> // T é um tipo genérico
    {
        // Cuidado para não violar o princípio ISP (Interface Segregation Principle)
        Task<IEnumerable<T>> GetAllAsync(); // Indicada para consultar coleções em memória e mais leve
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate); // predicate é uma expressão lambda, recebe um objeto do tipo T e retorna um booleano
        T Create(T entity); 
        T Update(T entity);
        T Delete(T entity);
    }
}
