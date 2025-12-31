namespace RCLAPI.DTO;

public class EncomendaPagamentoResponse
{
    public Guid EncomendaId { get; set; }

    public EncomendaEstado Estado { get; set; }

    public DateTime PagoEmUtc { get; set; }
}
