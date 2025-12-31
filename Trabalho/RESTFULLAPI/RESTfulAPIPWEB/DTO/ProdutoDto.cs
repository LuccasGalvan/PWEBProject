using RESTfulAPIPWEB.Entity;

namespace RESTfulAPIPWEB.DTO;

public class ProdutoDto
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Detalhe { get; set; }
    public string? Origem { get; set; }
    public string? Titulo { get; set; }
    public string? UrlImagem { get; set; }
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
}
