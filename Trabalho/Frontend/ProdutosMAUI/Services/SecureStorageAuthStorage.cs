using Microsoft.Maui.Storage;
using RCLAPI.Services;

namespace ProdutosMAUI.Services
{
    public class SecureStorageAuthStorage : IAuthStorage
    {
        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await SecureStorage.GetAsync(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetItemAsync(string key, string? value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SecureStorage.Remove(key);
                    return;
                }

                await SecureStorage.SetAsync(key, value);
            }
            catch (Exception)
            {
                SecureStorage.Remove(key);
            }
        }

        public Task RemoveItemAsync(string key)
        {
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }
    }
}
