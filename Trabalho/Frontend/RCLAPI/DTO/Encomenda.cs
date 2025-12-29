using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCLAPI.DTO;

public class Encomenda
{
    public Guid Id { get; set; }

    [Required]
    public string ClienteId { get; set; } = string.Empty;

    public DateTime CriadaEmUtc { get; set; }

    public EncomendaEstado Estado { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    public List<EncomendaItem> Itens { get; set; } = new();
}
