(() => {
    const form = document.getElementById('venta-form');
    const lineas = document.getElementById('lineas');
    const bodega = document.getElementById('bodega');
    const buscar = document.getElementById('buscar');
    function actualizar() {
        let total = 0;
        [...lineas.children].forEach((linea, i) => {
            const select = linea.querySelector('select'), cantidad = linea.querySelector('input');
            select.name = `Lineas[${i}].VarianteId`; cantidad.name = `Lineas[${i}].Cantidad`;
            for (const option of select.options) {
                if (!option.value) continue;
                const disponible = option.dataset.bodega === bodega.value;
                option.disabled = !disponible;
                option.hidden = !disponible || (option.value !== select.value && !option.dataset.busqueda.toLowerCase().includes(buscar.value.toLowerCase()));
            }
            if (select.selectedOptions[0]?.disabled) select.value = '';
            total += Number(select.selectedOptions[0]?.dataset.precio || 0) * Number(cantidad.value || 0);
        });
        document.getElementById('total').textContent = total.toLocaleString('es-CL');
    }
    function agregar() {
        if (lineas.children.length >= 100) return;
        const fragment = document.getElementById('linea-template').content.cloneNode(true);
        fragment.querySelector('.quitar').addEventListener('click', e => { e.target.closest('.sale-line').remove(); actualizar(); });
        lineas.append(fragment); actualizar();
    }
    document.getElementById('agregar').addEventListener('click', agregar);
    lineas.addEventListener('input', actualizar); buscar.addEventListener('input', actualizar);
    bodega.addEventListener('change', () => { for (const select of lineas.querySelectorAll('select')) select.value = ''; actualizar(); });
    form.addEventListener('submit', e => {
        const contenedor=document.getElementById('pagos-hidden');contenedor.replaceChildren();
        [...document.querySelectorAll('.pago-combinado')].filter(x=>Number(x.value)>0).forEach((x,i)=>{
            for(const [campo,valor] of [['Medio',x.dataset.medio],['Importe',x.value]]){const hidden=document.createElement('input');hidden.type='hidden';hidden.name=`Pagos[${i}].${campo}`;hidden.value=valor;contenedor.append(hidden);}
        });
        if (!lineas.children.length) { e.preventDefault(); agregar(); return; }
        const boton = document.getElementById('confirmar'); boton.disabled = true; boton.textContent = 'Guardando…';
    });
    agregar();
})();
