using GestaoLoja.Data;
using GestaoLoja.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja.Services;

public sealed class EncomendaWorkflowService
{
    private static readonly IReadOnlyDictionary<EncomendaEstado, IReadOnlyCollection<EncomendaEstado>> AllowedTransitions =
        new Dictionary<EncomendaEstado, IReadOnlyCollection<EncomendaEstado>>
        {
            [EncomendaEstado.Paga] = new[] { EncomendaEstado.Confirmada, EncomendaEstado.Rejeitada },
            [EncomendaEstado.Confirmada] = new[] { EncomendaEstado.Expedida }
        };

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public EncomendaWorkflowService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public bool CanTransition(EncomendaEstado from, EncomendaEstado to)
        => AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public async Task<TransitionResult> TryTransitionAsync(Guid encomendaId, EncomendaEstado targetEstado)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var encomenda = await db.Encomendas
            .Include(e => e.Itens)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(e => e.Id == encomendaId);

        if (encomenda is null)
            return TransitionResult.Fail("Encomenda não encontrada.");

        if (!CanTransition(encomenda.Estado, targetEstado))
            return TransitionResult.Fail("Transição de estado não permitida.");

        if (targetEstado == EncomendaEstado.Rejeitada)
        {
            foreach (var item in encomenda.Itens)
            {
                if (item.Produto is null)
                    return TransitionResult.Fail("Produto associado em falta para repor stock.");

                item.Produto.EmStock += item.Quantidade;
            }
        }

        encomenda.Estado = targetEstado;

        await db.SaveChangesAsync();

        return TransitionResult.Ok(targetEstado);
    }

    public sealed record TransitionResult(bool Success, string? ErrorMessage, EncomendaEstado? NewEstado)
    {
        public static TransitionResult Ok(EncomendaEstado estado)
            => new(true, null, estado);

        public static TransitionResult Fail(string message)
            => new(false, message, null);
    }
}
