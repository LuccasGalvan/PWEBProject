using RCLAPI.DTO;

namespace RCLAPI.Services
{
    public class AuthService
    {
        private readonly IApiServices _apiServices;
        private readonly IAuthStorage _authStorage;
        public string? LastErrorMessage { get; private set; }

        public AuthService(IApiServices apiServices, IAuthStorage authStorage)
        {
            _apiServices = apiServices;
            _authStorage = authStorage;
        }

        // Obtém as informações do utilizador diretamente da API
        public async Task<Utilizador?> GetUserInformation()
        {
            LastErrorMessage = null;
            // Chama o método que obtém as informações do utilizador da API
            var response = await _apiServices.GetUserInformation();

            return await BuildUserFromResponseAsync(response);
        }

        public async Task<UserProfileState> GetCurrentUserAsync()
        {
            LastErrorMessage = null;

            var accessToken = await _authStorage.GetItemAsync(AuthStorageKeys.AccessToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new UserProfileState
                {
                    IsAuthenticated = false,
                    ErrorMessage = "utilizador não autenticado. Faça login primeiro."
                };
            }

            var role = await _apiServices.GetRoleFromTokenAsync();
            var user = await GetUserInformation();

            if (user != null)
            {
                return new UserProfileState
                {
                    IsAuthenticated = true,
                    Role = role,
                    User = user
                };
            }

            var errorMessage = NormalizeProfileErrorMessage(LastErrorMessage);
            return new UserProfileState
            {
                IsAuthenticated = false,
                Role = role,
                ErrorMessage = errorMessage ?? "Não foi possível carregar as informações do utilizador."
            };
        }

        public async Task<bool> UpdateUserInformation(Utilizador user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.UserId))
                {
                    user.UserId = await _apiServices.GetUserIdFromTokenAsync()
                        ?? await _authStorage.GetItemAsync(AuthStorageKeys.UserId);
                }

                // Chama a API para atualizar as informações do usuário
                var response = await _apiServices.UpdateUserInformation(user);

                if (response != null && response.IsSuccessStatusCode)
                {
                    return true;  // Se a resposta for bem-sucedida, retorna true
                }

                // Se a resposta não for bem-sucedida, você pode logar ou tratar o erro conforme necessário
                return false;
            }
            catch (Exception ex)
            {
                // Trate possíveis exceções, como problemas de rede
                Console.WriteLine($"Erro ao atualizar as informações: {ex.Message}");
                return false;
            }
        }

        public async Task<UserProfileState> LoadUserProfileStateAsync()
        {
            LastErrorMessage = null;

            var accessToken = await _authStorage.GetItemAsync(AuthStorageKeys.AccessToken);
            var role = await _apiServices.GetRoleFromTokenAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new UserProfileState
                {
                    IsAuthenticated = false,
                    Role = role,
                    ErrorMessage = "Sessão não encontrada no armazenamento local. Faça login novamente."
                };
            }

            var user = await GetUserInformation();
            if (user != null)
            {
                return new UserProfileState
                {
                    IsAuthenticated = true,
                    Role = role,
                    User = user
                };
            }

            var errorMessage = NormalizeProfileErrorMessage(LastErrorMessage);
            return new UserProfileState
            {
                IsAuthenticated = false,
                Role = role,
                ErrorMessage = errorMessage ?? "Não foi possível carregar as informações do utilizador."
            };
        }

        public async Task ClearUserAsync()
        {
            await _authStorage.RemoveItemAsync(AuthStorageKeys.AccessToken);
            await _authStorage.RemoveItemAsync(AuthStorageKeys.UserId);
            await _authStorage.RemoveItemAsync(AuthStorageKeys.UserRole);
        }

        private async Task<Utilizador?> BuildUserFromResponseAsync(ApiResponse<Utilizador>? response)
        {
            // Verifica se a resposta da API foi bem-sucedida
            if (response != null && response.Data != null)
            {
                // Mapeia os dados da API para Utilizador
                var utilizador = new Utilizador
                {
                    UserId = response.Data.UserId,
                    EMail = response.Data.EMail,
                    Nome = response.Data.Nome,
                    Apelido = response.Data.Apelido,
                    NIF = response.Data.NIF,
                    Rua = response.Data.Rua,
                    Localidade1 = response.Data.Localidade1,
                    Localidade2 = response.Data.Localidade2,
                    Pais = response.Data.Pais,
                    Fotografia = null,
                    UrlImagem = null
                };

                if (!string.IsNullOrWhiteSpace(utilizador.UserId))
                {
                    await _authStorage.SetItemAsync(AuthStorageKeys.UserId, utilizador.UserId);
                }

                return utilizador;
            }

            if (response != null && !string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                LastErrorMessage = response.ErrorMessage;
                if (LastErrorMessage.Contains("Sessão expirada", StringComparison.OrdinalIgnoreCase)
                    || LastErrorMessage.Contains("sem permissões", StringComparison.OrdinalIgnoreCase)
                    || LastErrorMessage.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
                    || LastErrorMessage.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
                {
                    await ClearUserAsync();
                }
            }
            else if (response == null)
            {
                LastErrorMessage = "Erro ao obter resposta da API.";
            }

            // Se não encontrar dados, retorna null
            return null;
        }

        private static string? NormalizeProfileErrorMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            if (message.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                return "Sessão inválida. Faça login novamente.";
            }

            if (message.Contains("Sessão expirada", StringComparison.OrdinalIgnoreCase)
                || message.Contains("sem permissões", StringComparison.OrdinalIgnoreCase))
            {
                return "Sessão expirada. Faça login novamente.";
            }

            if (message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
                || message.Contains("Forbidden", StringComparison.OrdinalIgnoreCase))
            {
                return "Sessão expirada. Faça login novamente.";
            }

            return $"Não foi possível carregar as informações do utilizador: {message}";
        }
    }
}
