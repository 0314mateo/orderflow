import { useState, useEffect } from "react";
import { crearPedido, listarProductos } from "../services/ordersApi";

function FormularioPedido({ onPedidoCreado }) {
  const [productos, setProductos] = useState([]);
  const [clienteNombre, setClienteNombre] = useState("");
  const [sku, setSku] = useState("");
  const [cantidad, setCantidad] = useState(1);
  const [error, setError] = useState(null);
  const [enviando, setEnviando] = useState(false);

  useEffect(() => {
    listarProductos()
      .then((data) => {
        setProductos(data);
        if (data.length > 0) setSku(data[0].sku);
      })
      .catch((err) => setError(err.message));
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setError(null);
    setEnviando(true);

    try {
      await crearPedido({ clienteNombre, sku, cantidad: Number(cantidad) });
      setClienteNombre("");
      setCantidad(1);
      onPedidoCreado();
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
        Producto
        <select value={sku} onChange={(e) => setSku(e.target.value)}>
          {productos.map((p) => (
            <option key={p.sku} value={p.sku}>
              {p.nombre}
            </option>
          ))}
        </select>
      </label>

      {sku && (
        <p className="producto-seleccionado">
          SKU: <strong>{sku}</strong>
        </p>
      )}

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

      <button type="submit" disabled={enviando || productos.length === 0}>
        {enviando ? "Creando..." : "Crear pedido"}
      </button>

      {error && <p className="error-mensaje">{error}</p>}
    </form>
  );
}

export default FormularioPedido;