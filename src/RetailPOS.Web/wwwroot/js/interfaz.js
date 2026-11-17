(() => {
    const menu=document.getElementById('menu-toggle'),sidebar=document.getElementById('sidebar'),overlay=document.getElementById('sidebar-overlay');
    const movil=window.matchMedia('(max-width: 900px)');
    if(sidebar)sidebar.inert=movil.matches;
    function cerrar(){document.body.classList.remove('menu-open');menu?.setAttribute('aria-expanded','false');menu?.setAttribute('aria-label','Abrir menú principal');if(sidebar)sidebar.inert=movil.matches;}
    menu?.addEventListener('click',()=>{const abrir=!document.body.classList.contains('menu-open');document.body.classList.toggle('menu-open',abrir);menu.setAttribute('aria-expanded',String(abrir));menu.setAttribute('aria-label',abrir?'Cerrar menú principal':'Abrir menú principal');sidebar.inert=!abrir&&movil.matches;if(abrir)sidebar.querySelector('a')?.focus();});
    overlay?.addEventListener('click',()=>{cerrar();menu.focus();});
    document.addEventListener('keydown',e=>{
        if(!document.body.classList.contains('menu-open'))return;
        if(e.key==='Escape'){cerrar();menu.focus();}
        if(e.key==='Tab'){
            const nodos=[...sidebar.querySelectorAll('a,button')].filter(x=>x.getClientRects().length),primero=nodos[0],ultimo=nodos.at(-1);
            if(e.shiftKey&&document.activeElement===primero){e.preventDefault();ultimo.focus();}else if(!e.shiftKey&&document.activeElement===ultimo){e.preventDefault();primero.focus();}
        }
    });
    movil.addEventListener('change',()=>cerrar());
    document.querySelectorAll('[data-password]').forEach(boton=>boton.addEventListener('click',()=>{
        const input=document.getElementById(boton.dataset.password),mostrar=input.type==='password';input.type=mostrar?'text':'password';boton.textContent=mostrar?'Ocultar':'Ver';boton.setAttribute('aria-label',mostrar?'Ocultar contraseña':'Mostrar contraseña');boton.setAttribute('aria-pressed',String(mostrar));
    }));
    document.addEventListener('invalid',e=>{
        const campo=e.target;if(!(campo instanceof HTMLInputElement||campo instanceof HTMLSelectElement||campo instanceof HTMLTextArea))return;
        campo.setCustomValidity('');const v=campo.validity;let mensaje='';
        if(v.valueMissing)mensaje='Completa este campo para continuar.';
        else if(v.typeMismatch)mensaje=campo.type==='email'?'Ingresa un correo electrónico válido.':'Ingresa un valor válido.';
        else if(v.tooShort)mensaje=`Escribe al menos ${campo.minLength} caracteres.`;
        else if(v.tooLong)mensaje=`Escribe como máximo ${campo.maxLength} caracteres.`;
        else if(v.rangeUnderflow)mensaje=`El valor mínimo permitido es ${campo.min}.`;
        else if(v.rangeOverflow)mensaje=`El valor máximo permitido es ${campo.max}.`;
        else if(v.stepMismatch)mensaje='Revisa el número de decimales permitido.';
        else if(v.patternMismatch)mensaje='El formato ingresado no es válido.';
        else if(v.badInput)mensaje='Ingresa un número válido.';
        if(mensaje)campo.setCustomValidity(mensaje);
    },true);
    ['input','change'].forEach(evento=>document.addEventListener(evento,e=>{if(typeof e.target.setCustomValidity==='function')e.target.setCustomValidity('');}));
})();
