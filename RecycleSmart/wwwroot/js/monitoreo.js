// Conexión única a SignalR para todo el sistema (la crea el layout)
window.conexionMonitoreo = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/monitoreo")
    .withAutomaticReconnect()
    .build();

// Evita que un nombre con < o > se interprete como HTML
function esc(texto) {
    const d = document.createElement("div");
    d.textContent = texto ?? "";
    return d.innerHTML;
}

// Contador del sidebar
conexionMonitoreo.on("AlertasAbiertas", cantidad => {
    const badge = document.getElementById("contador-alertas");
    if (!badge) return;
    badge.textContent = cantidad;
    badge.classList.toggle("d-none", cantidad === 0);
});

// Aviso emergente
conexionMonitoreo.on("NuevaAlerta", a => {
    const critica = a.severidad === "Critico";
    const aviso = document.createElement("div");
    aviso.className = `toast border-0 text-bg-${critica ? "danger" : "warning"}`;
    aviso.setAttribute("role", "alert");
    aviso.innerHTML = `
        <div class="toast-header">
            <strong class="me-auto">${critica ? "Alerta crítica" : "Alerta de advertencia"}</strong>
            <small>${esc(a.institucion)}</small>
            <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">
            <strong>${esc(a.contenedor)}</strong>: ${esc(a.mensaje)}
            <div class="mt-2"><a href="/Alertas" class="btn btn-sm btn-light">Ver en Centro de Alertas</a></div>
        </div>`;
    document.getElementById("contenedor-avisos").appendChild(aviso);

    const toast = new bootstrap.Toast(aviso, { delay: critica ? 15000 : 8000 });
    toast.show();
    aviso.addEventListener("hidden.bs.toast", () => aviso.remove());
});

conexionMonitoreo.start().catch(err => console.error("SignalR:", err));