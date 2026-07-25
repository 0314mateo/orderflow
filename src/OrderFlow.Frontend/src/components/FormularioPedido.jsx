import { useState } from "react";
import { crearPedido } from "../services/ordersApi";

const SKUS_DISPONIBLES = ["ABC-01", "ABC-02", "ABC-03"];

function FormularioPedido({ onPedidoCreado }) {
  const [clienteNombre, setClienteNombre] = useState("");
  const [sku, setSku] = useState(SKUS_DISPONIBLES[0]);
  const [cantidad, setCantidad] = useState(1);
  const [error, setError] = useState(null);
  const [enviando, setEnviando] = useState(false);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setEnviando(true);

    try {
      await crearPedido({ clienteNombre, sku, cantidad: Number(cantidad) });
      setClienteNombre("");
      setCantidad(1);
      onPedidoCreado(); // avisa al padre para refrescar la lista de inmediato
    } catch (err) {
      setError(err.message);
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="formulario-pedido">
      <h2>Crear pedido</h2>

      <label>
        Cliente
        <input
          type="text"
          value={clienteNombre}
          onChange={(e) => setClienteNombre(e.target.value)}
          placeholder="Nombre del cliente"
          required
        />
      </label>

      <label>
        Producto (SKU)
        <select value={sku} onChange={(e) => setSku(e.target.value)}>
          {SKUS_DISPONIBLES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
      </label>

      <label>
        Cantidad
        <input
          type="number"
          min="1"
          max="100"
          value={cantidad}
          onChange={(e) => setCantidad(e.target.value)}
          required
        />
      </label>

      <button type="submit" disabled={enviando}>
        {enviando ? "Creando..." : "Crear pedido"}
      </button>

      {error && <p className="error-mensaje">{error}</p>}
    </form>
  );
}

export default FormularioPedido;