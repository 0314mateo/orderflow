import { useRef, useState, useEffect } from "react";
import FormularioPedido from "./components/FormularioPedido";
import ListaPedidos from "./components/ListaPedidos";

function obtenerTemaInicial() {
  const guardado = localStorage.getItem("theme");
  if (guardado) return guardado;
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function App() {
  const listaRef = useRef(null);
  const [tema, setTema] = useState(obtenerTemaInicial);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", tema);
    localStorage.setItem("theme", tema);
  }, [tema]);

  function handlePedidoCreado() {
    listaRef.current?.refrescar();
  }

  return (
    <div className="app-container">
      <div className="app-header">
        <h1>OrderFlow — Panel de operaciones</h1>
        <button
          className="theme-toggle"
          onClick={() => setTema((t) => (t === "dark" ? "light" : "dark"))}
        >
          {tema === "dark" ? "☀ Claro" : "☾ Oscuro"}
        </button>
      </div>
      <div className="app-layout">
        <FormularioPedido onPedidoCreado={handlePedidoCreado} />
        <ListaPedidos ref={listaRef} />
      </div>
    </div>
  );
}

export default App;