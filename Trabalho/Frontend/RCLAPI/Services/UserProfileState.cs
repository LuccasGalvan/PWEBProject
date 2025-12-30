using RCLAPI.DTO;

namespace RCLAPI.Services
{
    public sealed class UserProfileState
    {
        public Utilizador? User { get; init; }
        public string? Role { get; init; }
        public bool IsAuthenticated { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
