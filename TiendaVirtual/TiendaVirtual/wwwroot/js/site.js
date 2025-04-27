// Animación simple cuando el documento esté cargado
document.addEventListener("DOMContentLoaded", function () {
    const formContainers = document.querySelectorAll('.form-container, .register-container');
    formContainers.forEach(container => {
        container.style.opacity = 0;
        container.style.transform = 'translateY(40px)';
        setTimeout(() => {
            container.style.transition = 'opacity 1s ease, transform 1s ease';
            container.style.opacity = 1;
            container.style.transform = 'translateY(0)';
        }, 100);
    });
});

// Función para mostrar una alerta bonita
function showAlert(message, type = "success") {
    // Crear div
    const alert = document.createElement("div");
    alert.className = `custom-alert ${type}`;
    alert.textContent = message;

    // Agregar al body
    document.body.appendChild(alert);

    // Animar entrada
    setTimeout(() => {
        alert.classList.add("show");
    }, 100);

    // Quitar después de 3 segundos
    setTimeout(() => {
        alert.classList.remove("show");
        setTimeout(() => alert.remove(), 500);
    }, 3000);
}
