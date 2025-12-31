using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RCLAPI.DTO;

public class ProdutoDTO
{
    public int Id { get; set; }
    [StringLength(100)]
    [Required]
    public string? Nome { get; set; }
    [StringLength(200)]
    [Required]
    public string? Detalhe { get; set; }

    [StringLength(200)]
    
    public string? Origem { get; set; }

    public string? Titulo { get; set; } = string.Empty;

    [StringLength(200)]
    [Required]
    public string? UrlImagem { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Preco { get; set; }
    public bool Promocao { get; set; }
    public bool MaisVendido { get; set; }
    public decimal EmStock { get; set; }
    public bool Disponivel { get; set; }

    public int? ModoEntregaId { get; set; }
    public ModoEntrega? modoentrega { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? categoria { get; set; }

    public byte[]? Imagem { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }

    [JsonIgnore]
    [NotMapped]
    public bool Favorito { get; set; }
}
