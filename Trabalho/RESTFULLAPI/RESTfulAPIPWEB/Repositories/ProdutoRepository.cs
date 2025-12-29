using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.Entity;
using RESTfulAPIPWEB.Entity.Enums;

namespace RESTfulAPIPWEB.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly ApplicationDbContext _context;

        public ProdutoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<Produto> QueryCatalogoVisivel()
        {
            return _context.Produtos
                // Enunciado: only validated/active products visible to clients
                .Where(p => p.Estado == ProdutoEstado.Activo)
                .Where(p => p.PrecoFinal != null)
                // your old "must have image" rule, but null-safe
                .Where(p => p.Imagem != null && p.Imagem.Length > 0)
                .Include(p => p.modoentrega)
                .Include(p => p.categoria);
        }

        public async Task<IEnumerable<Produto>> ObterProdutosPorCategoriaAsync(int categoriaID)
        {
            var categoriaIds = await GetCategoriaComDescendentesAsync(categoriaID);
            return await QueryCatalogoVisivel()
                .Where(p => categoriaIds.Contains(p.CategoriaId))
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Produto>> ObterProdutosPromocaoAsync()
        {
            return await QueryCatalogoVisivel()
                .Where(p => p.Promocao)
                .OrderBy(p => p.categoria!.Ordem)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Produto>> ObterProdutosMaisVendidosAsync()
        {
            return await QueryCatalogoVisivel()
                .Where(p => p.MaisVendido)
                .OrderBy(p => p.categoria!.Ordem)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Produto>> ObterTodosProdutosAsync()
        {
            return await QueryCatalogoVisivel()
                .OrderBy(p => p.categoria!.Ordem)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<Produto?> ObterDetalheProdutoAsync(int id)
        {
            // Detail endpoint for catalog also must respect visibility
            return await QueryCatalogoVisivel()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Produto?> ObterProdutoEmDestaqueAsync()
        {
            return await QueryCatalogoVisivel()
                .OrderBy(p => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        private async Task<HashSet<int>> GetCategoriaComDescendentesAsync(int categoriaId)
        {
            var categorias = await _context.Categorias
                .AsNoTracking()
                .Select(c => new { c.Id, c.ParentId })
                .ToListAsync();

            var lookup = categorias
                .GroupBy(c => c.ParentId)
                .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

            var ids = new HashSet<int> { categoriaId };
            var stack = new Stack<int>();
            stack.Push(categoriaId);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!lookup.TryGetValue(current, out var children))
                {
                    continue;
                }

                foreach (var child in children)
                {
                    if (ids.Add(child))
                    {
                        stack.Push(child);
                    }
                }
            }

            return ids;
        }
    }
}
