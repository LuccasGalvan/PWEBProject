namespace RESTfulAPIPWEB.Entity.Enums;

public enum ProdutoEstado
{
    Pendente = 0, // supplier inserted/edited => not visible
    Activo = 1,   // visible to clients
    Inactivo = 2  // admin/employee disabled
}
