using RESTfulAPIPWEB.Entity.Enums;

namespace RESTfulAPIPWEB.DTO.Fornecedor
{
    public class FornecedorVendaDto
    {
        public Guid EncomendaId { get; set; }
        public DateTime EncomendaCriadaEmUtc { get; set; }
        public EncomendaEstado EncomendaEstado { get; set; }
        public int ProdutoId { get; set; }
        public string? ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
