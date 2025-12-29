using System.ComponentModel.DataAnnotations;

namespace RESTfulAPIPWEB.DTO.Auth
{
    public class RegisterRequest
    {
        [Required, StringLength(100)]
        public string Nome { get; set; } = default!;

        [Required, StringLength(100)]
        public string Apelido { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MinLength(6)]
        public string Password { get; set; } = default!;

        [Required, Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;

        public long? NIF { get; set; }

        public string? Rua { get; set; }
        public string? Localidade1 { get; set; }
        public string? Localidade2 { get; set; }
        public string? Pais { get; set; }
        public byte[]? Fotografia { get; set; }
    }
}