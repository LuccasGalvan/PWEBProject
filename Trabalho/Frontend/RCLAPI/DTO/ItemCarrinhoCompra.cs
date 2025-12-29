using System.ComponentModel.DataAnnotations.Schema;

namespace RCLAPI.DTO;
public class ItemCarrinhoCompra
{
    public int Id { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoUnitario { get; set; }    
    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal ValorTotal { get; set; }   
    public int ProdutoId { get; set; }
    public string UserId { get; set; }

}
