using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.DTO.Encomendas;
using RESTfulAPIPWEB.Entity;
using RESTfulAPIPWEB.Entity.Enums;

namespace RESTfulAPIPWEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class EncomendasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EncomendasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Encomendas/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Encomenda>>> GetEncomendas(string userId)
        {
            var encomendas = await _context.Encomendas
                .Where(e => e.ClienteId == userId)
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(e => e.CriadaEmUtc)
                .ToListAsync();

            return Ok(encomendas);
        }

        // GET: api/Encomendas/detalhes/{id}
        [HttpGet("detalhes/{id:guid}")]
        public async Task<ActionResult<Encomenda>> GetEncomendaDetalhes(Guid id)
        {
            var encomenda = await _context.Encomendas
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encomenda == null)
                return NotFound();

            return Ok(encomenda);
        }

        // POST: api/Encomendas/checkout?userId=...
        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Checkout([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("userId obrigatório.");

            // Load cart first
            var cartItems = await _context.CarrinhoCompras
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Count == 0)
                return BadRequest("Carrinho vazio.");

            // Load all products in one query
            var produtoIds = cartItems.Select(c => c.ProdutoId).Distinct().ToList();
            var produtos = await _context.Produtos
                .Where(p => produtoIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Validate everything before creating the order
            foreach (var c in cartItems)
            {
                if (!produtos.TryGetValue(c.ProdutoId, out var produto))
                    return BadRequest($"Produto inválido: {c.ProdutoId}");

                if (produto.Estado != ProdutoEstado.Activo || produto.PrecoFinal == null)
                    return BadRequest($"Produto não disponível: {produto.Nome}");

                if (c.Quantidade <= 0)
                    return BadRequest($"Quantidade inválida no carrinho para: {produto.Nome}");

                if (produto.EmStock < c.Quantidade)
                    return BadRequest($"Stock insuficiente para: {produto.Nome}");
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            var encomenda = new Encomenda
            {
                ClienteId = userId,
                Estado = EncomendaEstado.PendentePagamento
            };

            _context.Encomendas.Add(encomenda);

            decimal total = 0m;

            foreach (var c in cartItems)
            {
                var produto = produtos[c.ProdutoId];

                var precoUnit = produto.PrecoFinal!.Value;
                var subtotal = precoUnit * c.Quantidade;

                _context.EncomendaItens.Add(new EncomendaItem
                {
                    EncomendaId = encomenda.Id,
                    ProdutoId = produto.Id,
                    Quantidade = c.Quantidade,
                    PrecoUnitario = precoUnit,
                    Subtotal = subtotal
                });

                produto.EmStock -= c.Quantidade;
                total += subtotal;
            }

            encomenda.Total = total;

            // Clear cart
            _context.CarrinhoCompras.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(nameof(GetEncomendaDetalhes), new { id = encomenda.Id }, encomenda);
        }

        // POST: api/Encomendas/{id}/pagar
        [HttpPost("{id:guid}/pagar")]
        [ProducesResponseType(typeof(EncomendaPagamentoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EncomendaPagamentoResponse>> Pagar(Guid id)
        {
            var encomenda = await _context.Encomendas.FirstOrDefaultAsync(e => e.Id == id);

            if (encomenda == null)
                return NotFound();

            if (encomenda.Estado != EncomendaEstado.PendentePagamento)
                return BadRequest("Encomenda não está pendente de pagamento.");

            encomenda.Estado = EncomendaEstado.Paga;
            await _context.SaveChangesAsync();

            var response = new EncomendaPagamentoResponse
            {
                EncomendaId = encomenda.Id,
                Estado = encomenda.Estado,
                PagoEmUtc = DateTime.UtcNow
            };

            return Ok(response);
        }
    }
}
