using Microsoft.JSInterop;

namespace RCLAPI.Services
{
    public class BrowserAuthStorage : IAuthStorage
    {
        private readonly IJSRuntime _jsRuntime;

        public BrowserAuthStorage(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetItemAsync(string key, string? value)
        {
            if (value is null)
            {
                await RemoveItemAsync(key);
                return;
            }

            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            }
            catch (Exception)
            {
                await RemoveItemAsync(key);
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch (Exception)
            {
                // Ignore storage failures to avoid crashing during prerender.
            }
        }
    }
}
