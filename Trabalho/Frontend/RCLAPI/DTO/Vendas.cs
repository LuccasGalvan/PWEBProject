using System.ComponentModel.DataAnnotations;

namespace RCLAPI.DTO
{
    public class Vendas
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public ProdutoDTO Produto { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        public int Quantidade { get; set; }
        public double Preco { get; set; }
        public bool Confirmada { get; set; } = false;
        public DateTime DataAdicionado { get; set; } = DateTime.UtcNow;
    }
}
