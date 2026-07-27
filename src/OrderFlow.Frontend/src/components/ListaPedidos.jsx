import { useEffect, useState, useCallback, forwardRef, useImperativeHandle } from "react";
import { listarPedidos, listarProductos } from "../services/ordersApi";

const ESTADO_LABELS = {
  0: "Pending",
  1: "Confirmed",
  2: "Rejected",
};

const ESTADO_CLASES = {
  0: "estado-pending",
  1: "estado-confirmed",
  2: "estado-rejected",
};

const ListaPedidos = forwardRef((_props, ref) => {
  const [pedidos, setPedidos] = useState([]);
  const [nombresPorSku, setNombresPorSku] = useState({});
  const [error, setError] = useState(null);

  const cargarPedidos = useCallback(async () => {
    try {
      const data = await listarPedidos();
      setPedidos(data);
      setError(null);
    } catch (err) {
      setError(err.message);
    }
  }, []);

  useEffect(() => {
    listarProductos()
      .then((productos) => {
        const mapa = Object.fromEntries(productos.map((p) => [p.sku, p.nombre]));
        setNombresPorSku(mapa);
      })
      .catch(() => {});
  }, []);

  useImperativeHandle(ref, () => ({
    refrescar: cargarPedidos,
  }));

  useEffect(() => {
    cargarPedidos();
    const intervalo = setInterval(cargarPedidos, 3000);
    return () => clearInterval(intervalo);
  }, [cargarPedidos]);

  return (
    <div className="lista-pedidos">
      <div className="lista-header">
        <h2>Pedidos</h2>
        <span className="live-indicator">
          <span className="live-dot"></span>
          actualizando cada 3s
        </span>
      </div>

      {error && <p className="error-mensaje">{error}</p>}

      <table>
        <thead>
          <tr>
            <th>Cliente</th>
            <th>Producto</th>
            <th>Cantidad</th>
            <th>Estado</th>
            <th>Detalle</th>
            <th>Stock restante</th>
            <th>Creado</th>
          </tr>
        </thead>
        <tbody>
          {pedidos.map((p) => (
            <tr key={p.id}>
              <td>{p.clienteNombre}</td>
              <td>
                {nombresPorSku[p.sku] ?? p.sku}{" "}
                <span style={{ color: "var(--text-muted)", fontSize: "0.8em" }}>({p.sku})</span>
              </td>
              <td>{p.cantidad}</td>
              <td>
                <span className={`badge ${ESTADO_CLASES[p.estado]}`}>
                  {ESTADO_LABELS[p.estado]}
                </span>
              </td>
              <td>{p.detalle ?? "—"}</td>
              <td>{p.stockRestante ?? "—"}</td>
              <td>{new Date(p.creadoEn).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>

      {pedidos.length === 0 && !error && <p>No hay pedidos todavía.</p>}
    </div>
  );
});

export default ListaPedidos;