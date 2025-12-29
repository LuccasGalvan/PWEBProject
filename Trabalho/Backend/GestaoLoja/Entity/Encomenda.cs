using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestaoLoja.Entity.Enums;

namespace GestaoLoja.Entity
{
    public class Encomenda
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Identity user id (Cliente)
        [Required]
        public string ClienteId { get; set; } = default!;

        public DateTime CriadaEmUtc { get; set; } = DateTime.UtcNow;

        public EncomendaEstado Estado { get; set; } = EncomendaEstado.PendentePagamento;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // Navigation
        public ICollection<EncomendaItem> Itens { get; set; } = new List<EncomendaItem>();
    }
}