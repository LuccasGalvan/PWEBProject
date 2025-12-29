using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTfulAPIPWEB.Entity
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = default!;
        public int? ParentId { get; set; }
        [JsonIgnore]
        public Categoria? Parent { get; set; }
        public ICollection<Categoria> Children { get; set; } = new List<Categoria>();
        public int? Ordem {  get; set; }
        public string? UrlImagem { get; set; }
        public byte[]? Imagem { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}
