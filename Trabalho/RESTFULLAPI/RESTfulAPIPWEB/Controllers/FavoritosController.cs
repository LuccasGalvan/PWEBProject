using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.Entity;

namespace RESTfulAPIPWEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class FavoritosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Favoritos/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Favoritos>>> GetFavoritos(string userId)
        {
            var favoritos = await _context.Favoritos
                .Where(f => f.UserId == userId)
                .Include(f => f.Produto) // Inclui os dados do produto.
                .ToListAsync();

            if (favoritos == null || favoritos.Count == 0)
            {
                return NotFound(new { Message = "Nenhum favorito encontrado." });
            }

            var favoritosDTO = favoritos.Select(f => new Favoritos
            {
                Id = f.Id,
                ProdutoId = f.ProdutoId,
                Produto = new Produto
                {
                    Id = f.Produto.Id,
                    Nome = f.Produto.Nome,
                    PrecoFinal = f.Produto.PrecoFinal,
                    Estado = f.Produto.Estado,
                    Imagem = f.Produto.Imagem
                },
                UserId = f.UserId
            }).ToList();

            return Ok(favoritosDTO);
        }

        // PUT: api/Favoritos/{produtoId}/{acao}
        [HttpPut("{produtoId}/{acao}")]
        public async Task<IActionResult> AtualizarFavorito(int produtoId, string acao, [FromQuery] string userId)
        {
            var favorito = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.ProdutoId == produtoId && f.UserId == userId);

            if (acao.ToLower() == "adicionar")
            {
                if (favorito != null)
                {
                    return BadRequest(new { Message = "O produto já está nos favoritos." });
                }

                var novoFavorito = new Favoritos
                {
                    ProdutoId = produtoId,
                    UserId = userId
                };

                _context.Favoritos.Add(novoFavorito);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetFavoritos), new { userId = userId }, novoFavorito);
            }
            else if (acao.ToLower() == "remover")
            {
                if (favorito == null)
                {
                    return NotFound(new { Message = "O produto não está nos favoritos." });
                }

                _context.Favoritos.Remove(favorito);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return BadRequest(new { Message = "Ação inválida. Use 'adicionar' ou 'remover'." });
            }
        }

        private bool FavoritosExists(int id)
        {
            return _context.Favoritos.Any(e => e.Id == id);
        }
    }
}
