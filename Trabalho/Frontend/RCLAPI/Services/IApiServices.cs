using RCLAPI.DTO;
using System.Net.Http;

namespace RCLAPI.Services;

public interface IApiServices
{
    public Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria);
    public Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint);
    public Task<List<Categoria>> GetCategorias();
    public Task<(bool Data, string? ErrorMessage)> ActualizaFavorito(string acao,int produtoId);
    public Task<List<ProdutoFavorito>> GetFavoritos(string utilizadorId);
    public Task<ApiResponse<bool>> RegistarUtilizador(Utilizador novoUtilizador);
    public Task<ApiResponse<bool>> Login(LoginModel login);
    public Task<ApiResponse<bool>> AdicionaItemNoCarrinho(ItemCarrinhoCompra carrinhoCompra);
    public Task<ApiResponse<Utilizador>> GetUserInformation(string userID);
    public Task<HttpResponseMessage> UpdateUserInformation(Utilizador user);
    public Task<List<CarOrder>?> ObterCarrinho(string userId);
    public Task<(bool Success, string? Message)> AtualizarCarrinho(string userId, int produtoId, string acao, int quantidade);
    public Task<List<Vendas>> ObterVendas(string userId);
    public Task<bool> CriarVenda(Vendas venda);
    public Task LimparCarrinho(string userId);
}
