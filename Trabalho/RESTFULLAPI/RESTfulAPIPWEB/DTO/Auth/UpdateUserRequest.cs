using System.ComponentModel.DataAnnotations;

namespace RESTfulAPIPWEB.DTO.Auth;

public class UpdateUserRequest
{
    [Required]
    public string UserId { get; set; } = default!;

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? Nome { get; set; }

    [StringLength(100)]
    public string? Apelido { get; set; }

    public long? NIF { get; set; }

    public string? Rua { get; set; }
    public string? Localidade1 { get; set; }
    public string? Localidade2 { get; set; }
    public string? Pais { get; set; }
    public byte[]? Fotografia { get; set; }

    // Optional password change
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
