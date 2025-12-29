using System.ComponentModel.DataAnnotations;

namespace RESTfulAPIPWEB.DTO.Fornecedor
{
    public class FornecedorProdutoUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(100)]
        public string? Detalhe { get; set; }

        public string? UrlImagem { get; set; }
        public byte[]? Imagem { get; set; }

        [Required]
        public decimal PrecoBase { get; set; }

        public decimal? MargemPercentual { get; set; }

        public bool Promocao { get; set; }
        public bool MaisVendido { get; set; }

        [Range(0, int.MaxValue)]
        public int EmStock { get; set; }

        public bool ParaVenda { get; set; }
        public string? Origem { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoriaId { get; set; }

        public int? ModoEntregaId { get; set; }
    }
}
