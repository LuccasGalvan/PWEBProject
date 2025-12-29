namespace RCLAPI.DTO.Fornecedor;

public class FornecedorProdutoUpdateDto
{
    public string? Nome { get; set; }
    public string? Detalhe { get; set; }
    public string? UrlImagem { get; set; }
    public byte[]? Imagem { get; set; }
    public decimal PrecoBase { get; set; }
    public decimal? MargemPercentual { get; set; }
    public bool Promocao { get; set; }
    public bool MaisVendido { get; set; }
    public int EmStock { get; set; }
    public bool ParaVenda { get; set; }
    public string? Origem { get; set; }
    public int CategoriaId { get; set; }
    public int? ModoEntregaId { get; set; }
}
