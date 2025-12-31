using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWEB.DTO;
using RESTfulAPIPWEB.Entity;
using RESTfulAPIPWEB.Repositories;


namespace RESTfulAPIPWEB.Controllers
{
    [Route("api/Produtos")]
    [ApiController]
    [Authorize]
    public class ProdutoController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoController(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProdutos(int? categoriaId = null, bool soAtivos = true)
        {
            IEnumerable<Produto> produtos;
            if (!soAtivos)
            {
                produtos = await _produtoRepository.ObterTodosProdutosAsync();
            }
            else if (categoriaId != null)
            {
                produtos = await _produtoRepository.ObterProdutosPorCategoriaAsync(categoriaId.Value);
            }
            else
            {
                produtos = await _produtoRepository.ObterTodosProdutosAsync();
            }

            var produtosDto = produtos.Select(MapProduto);
            return Ok(produtosDto);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDetalheProduto(int id)
        {
            var produto = await _produtoRepository.ObterDetalheProdutoAsync(id);
            if (produto == null)
                return NotFound($"Produto com id={id} não encontrado");

            return Ok(MapProduto(produto));
        }

        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProdutoEmDestaque()
        {
            var produto = await _produtoRepository.ObterProdutoEmDestaqueAsync();
            if (produto == null)
                return NotFound("Nenhum produto activo disponível.");

            return Ok(MapProduto(produto));
        }

        private static ProdutoDto MapProduto(Produto produto)
        {
            return new ProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Detalhe = produto.Detalhe,
                Origem = produto.Origem,
                Titulo = string.Empty,
                UrlImagem = produto.UrlImagem,
                Preco = produto.PrecoFinal ?? produto.PrecoBase,
                Promocao = produto.Promocao,
                MaisVendido = produto.MaisVendido,
                EmStock = produto.EmStock,
                Disponivel = produto.ParaVenda,
                ModoEntregaId = produto.ModoEntregaId,
                modoentrega = produto.modoentrega,
                CategoriaId = produto.CategoriaId,
                categoria = produto.categoria,
                Imagem = produto.Imagem,
            };
        }
    }
}
