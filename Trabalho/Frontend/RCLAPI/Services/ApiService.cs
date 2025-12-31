using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RCLAPI.DTO;
using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data.Entity.Core.Objects;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.NetworkInformation;
using System.Net.Http.Json;
using RCLAPI.DTO.Fornecedor;

namespace RCLAPI.Services;
public class ApiService : IApiServices
{
    private readonly ILogger<ApiService> _logger;
    private readonly HttpClient _httpClient = new();
    private readonly IAuthStorage _authStorage;

    private readonly IHttpContextAccessor _httpContextAccessor;

    JsonSerializerOptions _serializerOptions;

    private List<ProdutoDTO> produtos;

    private List<Categoria> categorias;

    private ProdutoDTO _detalhesProduto;
    public ApiService(ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor, IAuthStorage authStorage)
    {
        _httpContextAccessor = httpContextAccessor;

        _logger = logger;
        _authStorage = authStorage;
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _detalhesProduto = new ProdutoDTO();
        categorias = new List<Categoria>();
    }
    //private void AddAuthorizationHeader()
    //{
    //    if (!string.IsNullOrEmpty(token))
    //    {
    //        _httpClient.DefaultRequestHeaders.Authorization =
    //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    //    }
    //}
    private async Task<HttpRequestMessage> CreateAuthorizedRequest(HttpMethod method, string endpoint, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, $"{AppConfig.BaseUrl}{endpoint}");
        var token = await _authStorage.GetItemAsync(AuthStorageKeys.AccessToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (content != null)
        {
            request.Content = content;
        }

        return request;
    }

    private async Task<(HttpRequestMessage? Request, string? ErrorMessage)> CreateAuthorizedRequestOrPrompt(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null)
    {
        var token = await _authStorage.GetItemAsync(AuthStorageKeys.AccessToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return (null, "Sessão expirada. Faça login novamente.");
        }

        var request = new HttpRequestMessage(method, $"{AppConfig.BaseUrl}{endpoint}")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return (request, null);
    }

    private async Task ClearAuthTokensAsync()
    {
        await _authStorage.RemoveItemAsync(AuthStorageKeys.AccessToken);
        await _authStorage.RemoveItemAsync(AuthStorageKeys.UserId);
        await _authStorage.RemoveItemAsync(AuthStorageKeys.UserRole);
    }

    // ********************* Categorias  **********
    public async Task<List<Categoria>> GetCategorias()
    {
        string endpoint = $"api/Categorias";

        try
        {
            HttpResponseMessage httpResponseMessage =
                await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = "";

                content = await httpResponseMessage.Content.ReadAsStringAsync();
                categorias = JsonSerializer.Deserialize<List<Categoria>>(content, _serializerOptions)!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }

        return categorias;
    }

    // ********************* Produtos  **********
    public async Task<List<ProdutoDTO>> GetProdutosEspecificos(string produtoTipo, int? IdCategoria)
    {
        string endpoint = "";

        if (produtoTipo == "categoria" && IdCategoria != null)
        {
            endpoint = $"api/Produtos?categoriaId={IdCategoria}&soAtivos=true";
        }
        else if (produtoTipo == "todos")
        {
            endpoint = $"api/Produtos?soAtivos=true";
        }
        else
        {
            return null;
        }
        try
        {
            HttpResponseMessage httpResponseMessage =
                await _httpClient.GetAsync($"{AppConfig.BaseUrl}{endpoint}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                string content = "";
                content = await httpResponseMessage.Content.ReadAsStringAsync();
                produtos = JsonSerializer.Deserialize<List<ProdutoDTO>>(content, _serializerOptions)!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        return produtos;
    }
    public async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);
                    return (data ?? Activator.CreateInstance<T>(), null);
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Erro de desserialização JSON: {ex.Message}. Conteúdo recebido: {responseString}");
                    return (default, $"Erro de desserialização JSON: {ex.Message}");
                }
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string errorMessage = "Unauthorized";
                    _logger.LogWarning(errorMessage);
                    return (default, errorMessage);
                }
                string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                _logger.LogError(generalErrorMessage);
                return (default, generalErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            string errrMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errrMessage);
            return (default, errrMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(errorMessage);
            return (default, errorMessage);
        }
    }



    // ***************** Compras ******************
    public async Task<ApiResponse<bool>> AdicionaItemNoCarrinho(ItemCarrinhoCompra carrinhoCompra)
    {
        try
        {
            var json = JsonSerializer.Serialize(carrinhoCompra, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest("api/ItensCarrinhoCompra", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                };
            }
            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao adicionar item no carrinho de compras: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    // ****************** Utilizadores ********************
    public async Task<ApiResponse<bool>> RegistarUtilizador(Utilizador novoUtilizador)
    {
        try
        {
            string endpoint = "api/Utilizadores/RegistarUser";

            var json = JsonSerializer.Serialize(novoUtilizador, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest($"{AppConfig.BaseUrl}{endpoint}", content);

            Console.WriteLine(response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.LogError($"Erro de BadRequest ao registar o utilizador: {errorResponse}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro de BadRequest: {errorResponse}"
                    };
                }
                else
                {
                    _logger.LogError($"Erro ao enviar requisitos Http: {response.StatusCode} - {errorResponse}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}. Detalhes: {errorResponse}"
                    };
                }
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao registar o utilizador: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> RegistarFornecedor(Utilizador novoUtilizador)
    {
        try
        {
            string endpoint = "api/Utilizadores/RegistarFornecedor";

            var json = JsonSerializer.Serialize(novoUtilizador, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest($"{AppConfig.BaseUrl}{endpoint}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.LogError($"Erro de BadRequest ao registar o fornecedor: {errorResponse}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro de BadRequest: {errorResponse}"
                    };
                }

                _logger.LogError($"Erro ao enviar requisitos Http: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}. Detalhes: {errorResponse}"
                };
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao registar o fornecedor: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> Login(LoginModel login)
    {
        try
        {
            string endpoint = "api/Utilizadores/LoginUser";

            var json = JsonSerializer.Serialize(login, _serializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await PostRequest($"{AppConfig.BaseUrl}{endpoint}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Erro ao enviar requisição Http: {response.StatusCode} - {errorContent}";
                _logger.LogError(errorMessage);
                return new ApiResponse<bool> { ErrorMessage = errorMessage };
            }
            var jsonResult = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Resposta do servidor: {jsonResult}");

            try
            {
                using var document = JsonDocument.Parse(jsonResult);
                if (document.RootElement.TryGetProperty("hasError", out var hasErrorElement) && hasErrorElement.GetBoolean())
                {
                    var errorMessage = document.RootElement.TryGetProperty("errorMessage", out var messageElement)
                        ? messageElement.GetString()
                        : "Erro ao fazer login.";
                    return new ApiResponse<bool> { ErrorMessage = errorMessage };
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Resposta inesperada ao fazer login. Tentando desserializar token.");
            }

            var result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions);
            if (result == null || string.IsNullOrWhiteSpace(result.accesstoken) || string.IsNullOrWhiteSpace(result.utilizadorid))
            {
                _logger.LogError("Erro ao fazer login: token ou utilizador inválido.");
                return new ApiResponse<bool> { ErrorMessage = "Erro ao fazer login" };
            }

            // Salva o utilizadorid no LocalStorage
            string userID = result.utilizadorid;
            await _authStorage.SetItemAsync(AuthStorageKeys.UserId, userID);
            if (!string.IsNullOrWhiteSpace(result.accesstoken))
            {
                await _authStorage.SetItemAsync(AuthStorageKeys.AccessToken, result.accesstoken);
            }
            if (!string.IsNullOrWhiteSpace(result.role))
            {
                await _authStorage.SetItemAsync(AuthStorageKeys.UserRole, result.role);
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(ex.Message);
            return (default);
        }

    }
    private async Task<HttpResponseMessage> PostRequest(string enderecoURL, HttpContent content)
    {
        try
        {
            var result = await _httpClient.PostAsync(enderecoURL, content);
            return result;
        }
        catch (Exception ex)
        {
            // Log o erro ou trata conforme necessario
            _logger.LogError($"Erro ao enviar requisição POST para enderecoURL: {ex.Message}");
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    // *************** Gerir Favoritos ******************

    public async Task<(List<ProdutoFavorito>? Data, string? ErrorMessage)> GetFavoritos(string utilizadorId)
    {
        string endpoint = $"api/Favoritos/{utilizadorId}";

        try
        {
            var (request, authError) = await CreateAuthorizedRequestOrPrompt(HttpMethod.Get, endpoint);
            if (request == null)
            {
                _logger.LogWarning(authError);
                return (null, authError);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                List<ProdutoFavorito>? data = JsonSerializer.Deserialize<List<ProdutoFavorito>>(responseString, _serializerOptions);
                return (data ?? new List<ProdutoFavorito>(), null);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized ? "Unauthorized" : "Forbidden";
                _logger.LogWarning(errorMessage);
                return (null, errorMessage);
            }

            string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
            _logger.LogError(generalErrorMessage);
            return (null, generalErrorMessage);
        }
        catch (HttpRequestException ex)
        {
            string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errorMessage);
            return (null, errorMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(errorMessage);
            return (null, errorMessage);
        }
    }

    public async Task<(bool Data, string? ErrorMessage)> ActualizaFavorito(string acao, int produtoId)
    {
        try
        {
            // Mapeia as ações do frontend para as ações esperadas pela API
            string apiAcao = acao switch
            {
                "heartfill" => "adicionar",
                "heartsimples" => "remover",
                _ => throw new ArgumentException("Ação inválida.")
            };

            // Recupera o userId do local storage
            var userId = await _authStorage.GetItemAsync(AuthStorageKeys.UserId);
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "UserID não encontrado no local storage.";
                _logger.LogError(errorMessage);
                return (false, errorMessage);
            }

            // Constrói a URL com o userId como query string
            string url = $"api/Favoritos/{produtoId}/{apiAcao}?userId={userId}";

            // Envia a requisição PUT para a API
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var (request, authError) = await CreateAuthorizedRequestOrPrompt(HttpMethod.Put, url, content);
            if (request == null)
            {
                _logger.LogWarning(authError);
                return (false, authError);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                string generalErrorMessage = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Unauthorized",
                    HttpStatusCode.Forbidden => "Forbidden",
                    _ => $"Erro na requisição: {response.ReasonPhrase}"
                };

                _logger.LogError(generalErrorMessage);
                return (false, generalErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(errorMessage);
            return (false, errorMessage);
        }
    }

    private sealed class UserInfoResponse
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Nome { get; set; }
        public string? Apelido { get; set; }
        public long? NIF { get; set; }
        public string? Estado { get; set; }
    }

    public async Task<ApiResponse<Utilizador>> GetUserInformation()
    {
        try
        {
            var userId = await _authStorage.GetItemAsync(AuthStorageKeys.UserId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ApiResponse<Utilizador> { ErrorMessage = "Sessão expirada. Faça login novamente." };
            }

            string endpoint = $"api/Utilizadores/{Uri.EscapeDataString(userId)}";

            // Realiza a requisição HTTP GET
            var request = await CreateAuthorizedRequest(HttpMethod.Get, endpoint);
            var response = await _httpClient.SendAsync(request);

            // Verifica se a resposta foi bem-sucedida
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    _logger.LogWarning("Sessão expirada. Faça login novamente.");
                    return new ApiResponse<Utilizador>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                _logger.LogError($"Erro ao enviar requisição Http: {response.StatusCode}");
                return new ApiResponse<Utilizador> { ErrorMessage = $"Erro ao enviar requisição: {response.StatusCode}" };
            }

            // Lê o conteúdo da resposta HTTP
            var jsonResult = await response.Content.ReadAsStringAsync();

            // Deserializa a resposta para o objeto DTO
            var result = JsonSerializer.Deserialize<UserInfoResponse>(jsonResult, _serializerOptions);

            // Verifica se a deserialização falhou
            if (result == null)
            {
                Console.WriteLine("Erro ao fazer login");
                return new ApiResponse<Utilizador> { ErrorMessage = "Erro ao fazer login" };
            }

            var utilizador = new Utilizador
            {
                UserId = result.Id,
                Nome = result.Nome,
                Apelido = result.Apelido,
                EMail = result.Email,
                NIF = result.NIF,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };

            // Retorna a resposta com os dados do utilizador
            return new ApiResponse<Utilizador> { Data = utilizador };
        }
        catch (Exception ex)
        {
            // Em caso de exceção, loga o erro e retorna uma resposta com o erro
            string errorMessage = $"Erro inesperado: {ex.Message}";
            _logger.LogError(ex.Message);
            return new ApiResponse<Utilizador> { ErrorMessage = errorMessage };
        }
    }


    public async Task<HttpResponseMessage> UpdateUserInformation(Utilizador user)
    {
        string endpoint = "api/Utilizadores/UpdateUser";

        try
        {
            var json = JsonSerializer.Serialize(user, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = await CreateAuthorizedRequest(HttpMethod.Put, endpoint, content);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return response; 
            }

            string errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Erro ao atualizar utilizador. Status code: {response.StatusCode}, Detalhes: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erro na requisição HTTP: {ex.Message}");
            throw; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado: {ex.Message}");
            throw;
        }
    }

    public async Task<List<CarOrder>?> ObterCarrinho(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{AppConfig.BaseUrl}api/CarrinhoCompras/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var carrinho = await response.Content.ReadFromJsonAsync<List<CarOrder>>();
                return carrinho;
            }
            else
            {
                Console.WriteLine($"Erro ao obter carrinho: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro inesperado ao obter carrinho: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string? Message)> AtualizarCarrinho(string userId, int produtoId, string acao, int quantidade)
    {
        try
        {
            // Construir a URL com base na ação e quantidade
            string url = acao switch
            {
                "adicionar" => $"{AppConfig.BaseUrl}api/CarrinhoCompras/{produtoId}/adicionar?userId={userId}&quantidade={quantidade}",
                "remover" => $"{AppConfig.BaseUrl}api/CarrinhoCompras/{produtoId}/remover?userId={userId}&quantidade={quantidade}",
                _ => throw new ArgumentException("Ação inválida.", nameof(acao))
            };

            var response = await _httpClient.PutAsync(url, null); // PUT sem corpo

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                string message = await response.Content.ReadAsStringAsync();
                return (false, message);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Erro ao atualizar o carrinho: {ex.Message}");
        }
    }

    public async Task<List<Encomenda>> ObterEncomendas(string userId)
    {
        var response = await _httpClient.GetAsync($"{AppConfig.BaseUrl}api/Encomendas/{userId}");
        if (response.IsSuccessStatusCode)
        {
            var encomendas = await response.Content.ReadFromJsonAsync<List<Encomenda>>();
            return encomendas ?? new List<Encomenda>();
        }
        else
        {
            Console.WriteLine("Erro ao buscar encomendas.");
            return new List<Encomenda>();
        }
    }

    public async Task<ApiResponse<Encomenda>> CheckoutEncomenda(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new ApiResponse<Encomenda> { ErrorMessage = "UserId inválido para checkout." };
        }

        try
        {
            var request = await CreateAuthorizedRequest(HttpMethod.Post, $"api/Encomendas/checkout?userId={userId}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<Encomenda> { ErrorMessage = "Sessão expirada. Faça login novamente." };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<Encomenda> { ErrorMessage = $"Erro ao fazer checkout: {errorContent}" };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var encomenda = JsonSerializer.Deserialize<Encomenda>(jsonResult, _serializerOptions);
            return new ApiResponse<Encomenda> { Data = encomenda };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao fazer checkout da encomenda: {ex.Message}");
            return new ApiResponse<Encomenda> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<EncomendaPagamentoResponse>> PagarEncomenda(Guid encomendaId)
    {
        if (encomendaId == Guid.Empty)
        {
            return new ApiResponse<EncomendaPagamentoResponse> { ErrorMessage = "Encomenda inválida para pagamento." };
        }

        try
        {
            var request = await CreateAuthorizedRequest(HttpMethod.Post, $"api/Encomendas/{encomendaId}/pagar");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<EncomendaPagamentoResponse> { ErrorMessage = "Sessão expirada. Faça login novamente." };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse<EncomendaPagamentoResponse> { ErrorMessage = $"Erro ao pagar encomenda: {errorContent}" };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var pagamento = JsonSerializer.Deserialize<EncomendaPagamentoResponse>(jsonResult, _serializerOptions);
            return new ApiResponse<EncomendaPagamentoResponse> { Data = pagamento };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao pagar encomenda: {ex.Message}");
            return new ApiResponse<EncomendaPagamentoResponse> { ErrorMessage = ex.Message };
        }
    }

    public async Task LimparCarrinho(string userId)
    {
        var response = await _httpClient.PutAsync($"{AppConfig.BaseUrl}api/CarrinhoCompras/limpar/{userId}", null);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Erro ao limpar o carrinho.");
        }
    }

    public async Task<ApiResponse<List<FornecedorProdutoDto>>> GetFornecedorProdutos()
    {
        try
        {
            var request = await CreateAuthorizedRequest(HttpMethod.Get, "api/FornecedorProdutos");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<List<FornecedorProdutoDto>>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao obter produtos do fornecedor: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<List<FornecedorProdutoDto>>
                {
                    ErrorMessage = $"Erro ao obter produtos: {response.StatusCode}"
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<FornecedorProdutoDto>>(jsonResult, _serializerOptions);
            return new ApiResponse<List<FornecedorProdutoDto>> { Data = result ?? new List<FornecedorProdutoDto>() };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao obter produtos do fornecedor: {ex.Message}");
            return new ApiResponse<List<FornecedorProdutoDto>> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<FornecedorProdutoDto>> CreateFornecedorProduto(FornecedorProdutoCreateDto produto)
    {
        try
        {
            var json = JsonSerializer.Serialize(produto, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = await CreateAuthorizedRequest(HttpMethod.Post, "api/FornecedorProdutos", content);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<FornecedorProdutoDto>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao criar produto do fornecedor: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<FornecedorProdutoDto>
                {
                    ErrorMessage = $"Erro ao criar produto: {response.StatusCode}"
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FornecedorProdutoDto>(jsonResult, _serializerOptions);
            return new ApiResponse<FornecedorProdutoDto> { Data = result };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao criar produto do fornecedor: {ex.Message}");
            return new ApiResponse<FornecedorProdutoDto> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<FornecedorProdutoDto>> UpdateFornecedorProduto(int id, FornecedorProdutoUpdateDto produto)
    {
        try
        {
            var json = JsonSerializer.Serialize(produto, _serializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = await CreateAuthorizedRequest(HttpMethod.Put, $"api/FornecedorProdutos/{id}", content);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<FornecedorProdutoDto>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao atualizar produto do fornecedor: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<FornecedorProdutoDto>
                {
                    ErrorMessage = $"Erro ao atualizar produto: {response.StatusCode}"
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FornecedorProdutoDto>(jsonResult, _serializerOptions);
            return new ApiResponse<FornecedorProdutoDto> { Data = result };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar produto do fornecedor: {ex.Message}");
            return new ApiResponse<FornecedorProdutoDto> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> DeleteFornecedorProduto(int id)
    {
        try
        {
            var request = await CreateAuthorizedRequest(HttpMethod.Delete, $"api/FornecedorProdutos/{id}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao eliminar produto do fornecedor: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<bool>
                {
                    ErrorMessage = $"Erro ao eliminar produto: {response.StatusCode}"
                };
            }

            return new ApiResponse<bool> { Data = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao eliminar produto do fornecedor: {ex.Message}");
            return new ApiResponse<bool> { ErrorMessage = ex.Message };
        }
    }

    public async Task<ApiResponse<List<FornecedorVendaDto>>> GetFornecedorVendas()
    {
        try
        {
            var request = await CreateAuthorizedRequest(HttpMethod.Get, "api/FornecedorProdutos/vendas");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    await ClearAuthTokensAsync();
                    return new ApiResponse<List<FornecedorVendaDto>>
                    {
                        ErrorMessage = "Sessão expirada. Faça login novamente."
                    };
                }

                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro ao obter vendas do fornecedor: {response.StatusCode} - {errorResponse}");
                return new ApiResponse<List<FornecedorVendaDto>>
                {
                    ErrorMessage = $"Erro ao obter vendas: {response.StatusCode}"
                };
            }

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<FornecedorVendaDto>>(jsonResult, _serializerOptions);
            return new ApiResponse<List<FornecedorVendaDto>> { Data = result ?? new List<FornecedorVendaDto>() };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao obter vendas do fornecedor: {ex.Message}");
            return new ApiResponse<List<FornecedorVendaDto>> { ErrorMessage = ex.Message };
        }
    }

}
