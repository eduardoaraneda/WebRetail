(() => {
 const lineas=document.getElementById('compra-lineas');
 function nombres(){[...lineas.children].forEach((l,i)=>{l.querySelector('select').name=`Lineas[${i}].VarianteId`;l.querySelector('.cantidad').name=`Lineas[${i}].Cantidad`;l.querySelector('.costo').name=`Lineas[${i}].CostoUnitario`;});}
 function agregar(){if(lineas.children.length>=100)return;const f=document.getElementById('compra-template').content.cloneNode(true);f.querySelector('.quitar').addEventListener('click',e=>{e.target.closest('.fields').remove();nombres();});lineas.append(f);nombres();}
 document.getElementById('agregar-compra').addEventListener('click',agregar);agregar();
})();
