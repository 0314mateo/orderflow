import { useEffect, useState, useCallback, forwardRef, useImperativeHandle } from "react";
import { listarPedidos } from "../services/ordersApi";

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

    // Permite que el componente padre (App.jsx) fuerce un refresco inmediato
    // apenas se crea un pedido, sin esperar al próximo ciclo de polling.
    useImperativeHandle(ref, () => ({
        refrescar: cargarPedidos,
    }));

    useEffect(() => {
        cargarPedidos(); // carga inicial

        const intervalo = setInterval(cargarPedidos, 3000); // polling cada 3s
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
                        <th>SKU</th>
                        <th>Cantidad</th>
                        <th>Estado</th>
                        <th>Creado</th>
                    </tr>
                </thead>
                <tbody>
                    {pedidos.map((p) => (
                        <tr key={p.id}>
                            <td>{p.clienteNombre}</td>
                            <td>{p.sku}</td>
                            <td>{p.cantidad}</td>
                            <td>
                                <span className={`badge ${ESTADO_CLASES[p.estado]}`}>
                                    {ESTADO_LABELS[p.estado]}
                                </span>
                            </td>
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