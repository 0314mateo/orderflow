using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Dtos;

namespace OrderFlow.Orders.Api.Validation;
public static class PedidoValidator
{
    public static List<string> Validar(CrearPedidoRequest req, OrdersDbContext db)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(req.ClienteNombre))
            errores.Add("clienteNombre no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(req.Sku) || !db.Catalogo.Any(p => p.Sku == req.Sku))
            errores.Add($"El sku '{req.Sku}' no existe en el catálogo.");

        if (req.Cantidad < 1 || req.Cantidad > 100)
            errores.Add("cantidad debe estar entre 1 y 100.");

        return errores;
    }
}
