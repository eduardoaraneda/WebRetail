(() => {
 const form=document.getElementById('cierre-caja');if(!form)return;
 const esperado=Number(form.dataset.sistema),fondo=Number(form.dataset.fondo),ingresos=Number(form.dataset.ingresos),retiros=Number(form.dataset.retiros);
 const entradas=[...form.querySelectorAll('[data-declaracion]')];
 const moneda=n=>'$'+n.toLocaleString('es-CL');
 function calcular(){
   const completo=entradas.every(x=>x.value.trim()!==''&&x.validity.valid);
   const bruto=entradas.reduce((s,x)=>s+Number(x.value||0),0),neto=bruto-fondo-ingresos+retiros,diferencia=neto-esperado;
   document.getElementById('declarado-total').textContent=completo?moneda(bruto):'Completa los cuatro medios';
   document.getElementById('declarado-neto').textContent=completo?moneda(neto):'Pendiente';
   document.getElementById('declarado-diferencia').textContent=completo?moneda(diferencia):'Pendiente';
   const aviso=document.getElementById('resultado-cierre');
   aviso.textContent=!completo?'Ingresa lo recaudado por cada medio de pago.':diferencia<0?'Faltante de '+moneda(-diferencia):diferencia>0?'Sobrante de '+moneda(diferencia):'El total coincide con el sistema. Se verificará también cada medio al guardar.';
   aviso.classList.toggle('success',completo&&diferencia===0);
 }
 entradas.forEach(x=>['input','change'].forEach(evento=>x.addEventListener(evento,()=>queueMicrotask(calcular))));calcular();
})();
