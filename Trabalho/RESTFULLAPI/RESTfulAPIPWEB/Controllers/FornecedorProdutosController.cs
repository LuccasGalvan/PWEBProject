using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.DTO.Fornecedor;
using RESTfulAPIPWEB.Entity;
using RESTfulAPIPWEB.Entity.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RESTfulAPIPWEB.Controllers
{
    [Route("api/[controller]")]
    [Route("api/fornecedor/produtos")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Fornecedor")]
    public class FornecedorProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FornecedorProdutosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FornecedorProdutoDto>>> GetProdutos()
        {
            var fornecedorId = GetFornecedorId();
            if (string.IsNullOrWhiteSpace(fornecedorId))
                return Unauthorized();

            var produtos = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.FornecedorId == fornecedorId)
                .OrderBy(p => p.Nome)
                .Select(p => new FornecedorProdutoDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Detalhe = p.Detalhe,
                    UrlImagem = p.UrlImagem,
                    Imagem = p.Imagem,
                    PrecoBase = p.PrecoBase,
                    MargemPercentual = p.MargemPercentual,
                    PrecoFinal = p.PrecoFinal,
                    Estado = p.Estado,
                    Promocao = p.Promocao,
                    MaisVendido = p.MaisVendido,
                    EmStock = p.EmStock,
                    ParaVenda = p.ParaVenda,
                    Origem = p.Origem,
                    CategoriaId = p.CategoriaId,
                    ModoEntregaId = p.ModoEntregaId
                })
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorProdutoDto>> CreateProduto([FromBody] FornecedorProdutoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var fornecedorId = GetFornecedorId();
            if (string.IsNullOrWhiteSpace(fornecedorId))
                return Unauthorized();

            var produto = new Produto
            {
                Nome = dto.Nome,
                Detalhe = dto.Detalhe,
                UrlImagem = dto.UrlImagem,
                Imagem = dto.Imagem,
                PrecoBase = dto.PrecoBase,
                MargemPercentual = dto.MargemPercentual,
                Estado = ProdutoEstado.Pendente,
                PrecoFinal = null,
                FornecedorId = fornecedorId,
                Promocao = dto.Promocao,
                MaisVendido = dto.MaisVendido,
                EmStock = dto.EmStock,
                ParaVenda = dto.ParaVenda,
                Origem = dto.Origem,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return Created($"/api/FornecedorProdutos/{produto.Id}", MapProdutoDto(produto));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<FornecedorProdutoDto>> UpdateProduto(int id, [FromBody] FornecedorProdutoUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var fornecedorId = GetFornecedorId();
            if (string.IsNullOrWhiteSpace(fornecedorId))
                return Unauthorized();

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedorId);

            if (produto is null)
                return NotFound();

            produto.Nome = dto.Nome;
            produto.Detalhe = dto.Detalhe;
            produto.UrlImagem = dto.UrlImagem;
            if (dto.Imagem is not null)
                produto.Imagem = dto.Imagem;
            produto.PrecoBase = dto.PrecoBase;
            produto.MargemPercentual = dto.MargemPercentual;
            produto.Promocao = dto.Promocao;
            produto.MaisVendido = dto.MaisVendido;
            produto.EmStock = dto.EmStock;
            produto.ParaVenda = dto.ParaVenda;
            produto.Origem = dto.Origem;
            produto.CategoriaId = dto.CategoriaId;
            produto.ModoEntregaId = dto.ModoEntregaId;
            produto.Estado = ProdutoEstado.Pendente;
            produto.PrecoFinal = null;

            await _context.SaveChangesAsync();

            return Ok(MapProdutoDto(produto));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var fornecedorId = GetFornecedorId();
            if (string.IsNullOrWhiteSpace(fornecedorId))
                return Unauthorized();

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.FornecedorId == fornecedorId);

            if (produto is null)
                return NotFound();

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("vendas")]
        public async Task<ActionResult<IEnumerable<FornecedorVendaDto>>> GetVendas()
        {
            var fornecedorId = GetFornecedorId();
            if (string.IsNullOrWhiteSpace(fornecedorId))
                return Unauthorized();

            var vendas = await _context.EncomendaItens
                .AsNoTracking()
                .Where(i => i.Produto != null && i.Produto.FornecedorId == fornecedorId)
                .Include(i => i.Produto)
                .Include(i => i.Encomenda)
                .OrderByDescending(i => i.Encomenda!.CriadaEmUtc)
                .Select(i => new FornecedorVendaDto
                {
                    EncomendaId = i.EncomendaId,
                    EncomendaCriadaEmUtc = i.Encomenda!.CriadaEmUtc,
                    EncomendaEstado = i.Encomenda.Estado,
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.Produto!.Nome,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Subtotal = i.Subtotal
                })
                .ToListAsync();

            return Ok(vendas);
        }

        private string? GetFornecedorId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static FornecedorProdutoDto MapProdutoDto(Produto produto)
        {
            return new FornecedorProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Detalhe = produto.Detalhe,
                UrlImagem = produto.UrlImagem,
                Imagem = produto.Imagem,
                PrecoBase = produto.PrecoBase,
                MargemPercentual = produto.MargemPercentual,
                PrecoFinal = produto.PrecoFinal,
                Estado = produto.Estado,
                Promocao = produto.Promocao,
                MaisVendido = produto.MaisVendido,
                EmStock = produto.EmStock,
                ParaVenda = produto.ParaVenda,
                Origem = produto.Origem,
                CategoriaId = produto.CategoriaId,
                ModoEntregaId = produto.ModoEntregaId
            };
        }
    }
}
