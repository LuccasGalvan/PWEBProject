using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace RCLAPI.DTO;

public class ModoEntrega
    {
        public int Id { get; set; }
        [StringLength(100)]
        [Required]
        public string? Nome { get; set; }
        [StringLength(200)]
         public string? Detalhe { get; set; }

        [JsonIgnore]
        public ICollection<ProdutoDTO>? produtos { get; set; }

    }

