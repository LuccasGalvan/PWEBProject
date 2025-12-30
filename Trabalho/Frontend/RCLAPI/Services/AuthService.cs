using RCLAPI.DTO;
using Microsoft.JSInterop;

namespace RCLAPI.Services
{
    public class AuthService
    {
        private readonly IApiServices _apiServices;
        private readonly IJSRuntime _jsRuntime;
        public string? LastErrorMessage { get; private set; }

        public AuthService(IApiServices apiServices, IJSRuntime jsRuntime)
        {
            _apiServices = apiServices;
            _jsRuntime = jsRuntime;
        }

        // Obtém as informações do utilizador diretamente da API
        public async Task<Utilizador?> GetUserInformation()
        {
            LastErrorMessage = null;
            // Chama o método que obtém as informações do utilizador da API
            var response = await _apiServices.GetUserInformation();

            // Verifica se a resposta da API foi bem-sucedida
            if (response != null && response.Data != null)
            {
                // Mapeia os dados da API para Utilizador
                return new Utilizador
                {
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

        public async Task<bool> UpdateUserInformation(Utilizador user)
        {
            try
            {
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

        public async Task ClearUserAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userID");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userRole");
        }
    }
}
