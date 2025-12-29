using RESTfulAPIPWEB.Entity;

namespace RESTfulAPIPWEB.Repositories
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetCategorias();
    }
}
