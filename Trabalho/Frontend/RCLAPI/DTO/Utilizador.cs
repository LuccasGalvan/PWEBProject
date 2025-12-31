using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RCLAPI.DTO
{
    public class Utilizador
    {
        public string? UserId { get; set; }
        public string? Nome { get; set; }
        public string? Apelido { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Endereço de Email Inválido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "A indicação da Password é obrigatória!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "A confirmação da Password é obrigatória!")]
        [Compare("Password", ErrorMessage = "A Password e a Confirmação da Password não coincidem")]
        public string ConfirmPassword { get; set; }

        [ValidarNIF(ErrorMessage = "NIF inválido!")]
        public long? NIF { get; set; }
        public string? Rua { get; set; }
        public string? Localidade1 { get; set; }
        public string? Localidade2 { get; set; }
        public string? Pais { get; set; }
        public byte[]? Fotografia { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public string? UrlImagem { get; set; }

        // Validação customizada para o NIF
        public class ValidarNIF : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                // Verifica se o valor é um número e se tem 9 dígitos
                if (value is long nif && nif > 100000000 && nif < 1000000000)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
