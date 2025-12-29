
using System.ComponentModel.DataAnnotations;

namespace RCLAPI.DTO;
public class CarOrder
{
    //public decimal PrecoUnitario {  get; set; }
    //public int Quantidade { get; set; }
    //public decimal ValorTotal { get; set; }
    //public int ProdutoId { get; set; }
    //public int ClienteID { get; set; }

    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public ProdutoDTO Produto { get; set; }
    [Required]
    public string UserId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1.")]
    public int Quantidade { get; set; }

}
