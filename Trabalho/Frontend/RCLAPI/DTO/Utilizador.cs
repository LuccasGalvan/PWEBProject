using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RCLAPI.DTO
{
    public class Utilizador
    {
        public string? Nome { get; set; }
        public string? Apelido { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Endereço de Email Inválido")]
        public string? EMail { get; set; }

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
                    // Converte o número NIF para uma string para fácil manipulação
                    string nifString = nif.ToString();

                    // Se o NIF não tiver 9 dígitos, é inválido
                    if (nifString.Length != 9)
                        return false;

                    // Pesos para o cálculo do dígito de verificação
                    int[] pesos = { 1, 2, 3, 4, 5, 6, 7, 8 };

                    // Calcula o somatório
                    int soma = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        soma += (nifString[i] - '0') * pesos[i];
                    }

                    // Calcula o resto da divisão da soma por 11
                    int resto = soma % 11;

                    // Calcula o dígito de verificação
                    int digitoVerificacao = (resto == 0 || resto == 1) ? 0 : 11 - resto;

                    // Verifica se o dígito de verificação calculado é igual ao 9º dígito do NIF
                    return digitoVerificacao == (nifString[8] - '0');
                }

                return false;
            }
        }
    }
}
