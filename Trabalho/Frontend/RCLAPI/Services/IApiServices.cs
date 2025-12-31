using RCLAPI.DTO;
using RCLAPI.DTO.Fornecedor;
using System;
using System.Net.Http;

namespace RCLAPI.Services;

public interface IApiServices
{
    public Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria);
    public Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint);
    public Task<List<Categoria>> GetCategorias();
    public Task<(bool Data, string? ErrorMessage)> ActualizaFavorito(string acao,int produtoId);
    public Task<(List<ProdutoFavorito>? Data, string? ErrorMessage)> GetFavoritos(string utilizadorId);
    public Task<ApiResponse<bool>> RegistarUtilizador(Utilizador novoUtilizador);
    public Task<ApiResponse<bool>> RegistarFornecedor(Utilizador novoUtilizador);
    public Task<ApiResponse<bool>> Login(LoginModel login);
    public Task<ApiResponse<bool>> AdicionaItemNoCarrinho(ItemCarrinhoCompra carrinhoCompra);
    public Task<ApiResponse<Utilizador>> GetUserInformation();
    public Task<HttpResponseMessage> UpdateUserInformation(Utilizador user);
    public Task<List<CarOrder>?> ObterCarrinho(string userId);
    public Task<(bool Success, string? Message)> AtualizarCarrinho(string userId, int produtoId, string acao, int quantidade);
    public Task<List<Encomenda>> ObterEncomendas(string userId);
    public Task<ApiResponse<Encomenda>> CheckoutEncomenda(string userId);
    public Task<ApiResponse<EncomendaPagamentoResponse>> PagarEncomenda(Guid encomendaId);
    public Task LimparCarrinho(string userId);
    public Task<ApiResponse<List<FornecedorProdutoDto>>> GetFornecedorProdutos();
    public Task<ApiResponse<FornecedorProdutoDto>> CreateFornecedorProduto(FornecedorProdutoCreateDto produto);
    public Task<ApiResponse<FornecedorProdutoDto>> UpdateFornecedorProduto(int id, FornecedorProdutoUpdateDto produto);
    public Task<ApiResponse<bool>> DeleteFornecedorProduto(int id);
    public Task<ApiResponse<List<FornecedorVendaDto>>> GetFornecedorVendas();
}
