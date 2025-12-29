using RESTfulAPIPWEB.Data;
using RESTfulAPIPWEB.Entity.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RESTfulAPIPWEB.Entity
{
    public class Produto
    {
        public int Id { get; set; }

        [StringLength(100)]
        [Required]
        public string? Nome { get; set; }

        [StringLength(100)]
        [Required]
        public string? Detalhe {  get; set; }
        public string? UrlImagem {  get; set; }
        public byte[]?Imagem{get;set;}

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoBase { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MargemPercentual { get; set; } // e.g. 15.00 for 15%

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoFinal { get; set; }

        public ProdutoEstado Estado { get; set; } = ProdutoEstado.Pendente;

        // Ownership: supplier can only manage "their" products.
        [Required]
        public string FornecedorId { get; set; } = default!;
        [JsonIgnore]
        public ApplicationUser? Fornecedor { get; set; }

        public bool Promocao { get; set; }
        public bool MaisVendido { get; set; }

        [Range(0, int.MaxValue)]
        public int EmStock { get; set; }

        public bool ParaVenda { get; set; } = true;
        public string? Origem {  get; set; }
        public int CategoriaId  { get; set; }
        public Categoria? categoria { get; set; }

        [JsonIgnore]
        public int? ModoEntregaId { get; set; }
        public ModoEntrega? modoentrega { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
       
    }
}
