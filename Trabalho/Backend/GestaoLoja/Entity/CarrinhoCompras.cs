using System.ComponentModel.DataAnnotations;

namespace GestaoLoja.Entity
{
    public class CarrinhoCompras
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        [Required]
        public string UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
        public int Quantidade { get; set; }
    }
}
