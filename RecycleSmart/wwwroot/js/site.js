(function () {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    const icon = document.getElementById('icon');
    const boton = document.querySelector('.toggle-btn');


    function aplicar(colapsado) {
        sidebar.classList.toggle('colapsado', colapsado);
        icon.className = colapsado ? 'bx bx-chevrons-right' : 'bx bx-chevrons-left';
        localStorage.setItem('sidebarColapsado', colapsado);
    }
    aplicar(localStorage.getItem('sidebarColapsado') == 'true');

    boton.addEventListener('click', function () {
        aplicar(!sidebar.classList.contains('colapsado'));
    });

    sidebar.querySelectorAll('.has-dropdown').forEach(function (link) {
        link.addEventListener('click', function () {
            if (sidebar.classList.contains('colapsado')) {
                aplicar(false);
            }
        })
    })


    const btnMovil = document.getElementById('btn-menu-movil');
    const overlay = document.getElementById('sidebar-overlay');

    function abrirMovil(abrir) {
        sidebar.classList.toggle('abierto-movil', abrir);
        overlay.classList.toggle('visible', abrir);
    }

    if (btnMovil) {
        btnMovil.addEventListener('click', function () {
            abrirMovil(true);
        });
    }

    if (overlay) {
        overlay.addEventListener('click', function () {
            abrirMovil(false);
        });
    }

})();