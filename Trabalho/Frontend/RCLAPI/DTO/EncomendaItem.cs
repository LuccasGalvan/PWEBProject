using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCLAPI.DTO;

public class EncomendaItem
{
    public int Id { get; set; }

    [Required]
    public Guid EncomendaId { get; set; }

    [Required]
    public int ProdutoId { get; set; }

    public ProdutoDTO? Produto { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecoUnitario { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }
}
