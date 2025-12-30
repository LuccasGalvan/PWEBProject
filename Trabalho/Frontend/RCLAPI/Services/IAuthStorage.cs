namespace RCLAPI.Services
{
    public interface IAuthStorage
    {
        Task<string?> GetItemAsync(string key);
        Task SetItemAsync(string key, string? value);
        Task RemoveItemAsync(string key);
    }
}
