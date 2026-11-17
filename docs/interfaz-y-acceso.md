# Interfaz de acceso y menú lateral

El acceso usa una presentación dividida con ilustración SVG propia y formulario adaptable. Incluye selección de equipo o administración, mostrar/ocultar contraseña, recuperación, creación de cuenta y creación protegida de administrador. En pantalla pequeña se simplifica el panel visual.

## Creación de cuentas

- La primera cuenta es Administrador, exclusivamente mediante la configuración local inicial y con bloqueo transaccional de SQL. No hay contraseñas predeterminadas.
- Con la tienda configurada, el registro público crea únicamente Vendedor inactivo. El administrador activa esa cuenta en Usuarios y permisos.
- El formulario público no acepta Activo ni Rol; enviar esos campos no concede permisos.
- Crear otro administrador exige iniciar sesión con una cuenta Administrador y utilizar la gestión de usuarios.
- El acceso administrativo no acepta vendedores. El acceso general mantiene los roles existentes.
- Los destinos de retorno solo se aceptan si son locales a la aplicación.

## Menú

Barra izquierda fija en escritorio con navegación agrupada, indicador de sección activa, perfil y cierre de sesión. El contenido puede desplazarse de forma independiente de la barra. En móvil el menú se abre con un botón, cierra mediante fondo o Escape, controla el foco y queda fuera de la navegación por teclado cuando está cerrado. Se respetan permisos por rol y las vistas de impresión ocultan la navegación.

## Español

Se traducen las validaciones DataAnnotations, errores de Identity, mensajes de enlace/contraseña, conversiones de formularios y exceso de intentos. La validación HTML del navegador tiene mensajes propios en español, incluso si el sistema está configurado en otro idioma. No se cambió la interpretación invariante de los importes numéricos.

## Verificación

Pruebas HTTP de registro pendiente, intento de autoasignación de administrador, bloqueo de cuenta pendiente, validación en español, acceso administrativo, destino local y permisos. Revisión visual de login y menú en escritorio y a 390 px; captura del panel con datos ficticios, sin tokens, en artifacts/qa. Se mantienen las pruebas comerciales existentes.

No se modificaron tablas ni se agregaron cuentas a la base de trabajo como parte de la revisión visual. No se cambió el alcance simulado de los documentos.
