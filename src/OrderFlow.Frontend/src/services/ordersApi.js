const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:7000";

export async function crearPedido({ clienteNombre, sku, cantidad }) {
  const response = await fetch(`${API_BASE_URL}/orders`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ clienteNombre, sku, cantidad }),
  });

  const data = await response.json();

  if (!response.ok) {
    // Orders API devuelve { errores: [...] } en el 400
    const mensajes = data.errores?.join(" | ") ?? "Error al crear el pedido.";
    throw new Error(mensajes);
  }

  return data;
}

export async function listarPedidos() {
  const response = await fetch(`${API_BASE_URL}/orders`);

  if (!response.ok) {
    throw new Error("No se pudo obtener la lista de pedidos.");
  }

  return response.json();
}

export async function listarProductos() {
  const response = await fetch(`${API_BASE_URL}/products`);
  if (!response.ok) {
    throw new Error("No se pudo obtener el catálogo de productos.");
  }
  return response.json();
}