
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace RCLAPI.DTO;
public class Categoria
{
    public int Id { get; set; }
    public string? Nome { get; set; }

    public int? Ordem { get; set; }
    public string? UrlImagem { get;set; }

    public byte[]? Imagem { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}
