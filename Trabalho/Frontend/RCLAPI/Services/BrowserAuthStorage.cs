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
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        }

        public async Task SetItemAsync(string key, string? value)
        {
            if (value is null)
            {
                await RemoveItemAsync(key);
                return;
            }

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }

        public async Task RemoveItemAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }
}
