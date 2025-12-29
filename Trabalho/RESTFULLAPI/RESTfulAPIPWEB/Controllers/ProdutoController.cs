using Microsoft.AspNetCore.Mvc;
using RESTfulAPIPWEB.Entity;
using RESTfulAPIPWEB.Repositories;


namespace RESTfulAPIPWEB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProdutoController : Controller
    {
        private readonly IProdutoRepository _produtoRepository;

        public ProdutoController(IProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
        }

        [HttpGet]
        public async Task<IActionResult> GetProdutos(string? tipoProduto, int? categoriaId = null)
        {
            tipoProduto = string.IsNullOrWhiteSpace(tipoProduto) ? "todos" : tipoProduto;

            IEnumerable<Produto> produtos;

            if (tipoProduto == "categoria" && categoriaId != null)
                produtos = await _produtoRepository.ObterProdutosPorCategoriaAsync(categoriaId.Value);
            else if (tipoProduto == "promocao")
                produtos = await _produtoRepository.ObterProdutosPromocaoAsync();
            else if (tipoProduto == "maisvendido")
                produtos = await _produtoRepository.ObterProdutosMaisVendidosAsync();
            else if (tipoProduto == "todos")
                produtos = await _produtoRepository.ObterTodosProdutosAsync();
            else
                return BadRequest("Tipo de produto inválido. Use: categoria, promocao, maisvendido, todos.");

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalheProduto(int id)
        {
            var produto = await _produtoRepository.ObterDetalheProdutoAsync(id);
            if (produto == null)
                return NotFound($"Produto com id={id} não encontrado");

            return Ok(produto);
        }
    }
}