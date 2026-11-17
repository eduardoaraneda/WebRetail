# Punto de Venta Retail

Sistema de ventas e inventario para tiendas retail, independiente de los proyectos anteriores. C# / .NET 11, ASP.NET Core MVC, Razor, Identity, EF Core, SQL Server y exportación XLSX con ClosedXML.

**Los documentos siguen siendo simulados. No hay integración con SII ni se requiere certificado tributario para probar estos módulos.**

## Módulos disponibles

- Catálogo: marcas, modelos, tallas y sistemas, color, SKU, código de barras, precio y estado. Edición de modelos/variantes, búsqueda y paginación. El SKU se conserva como identificador.
- Ofertas: marca/talla/SKU, filtros combinables, fechas de Chile, porcentaje o monto fijo, mejor precio sin acumular ofertas. Precio y descuento histórico guardados en cada venta.
- Proveedores: altas, edición, contacto, RUT validado y estado activo.
- Compras: referencia única por proveedor, varias líneas, costo unitario, ingreso transaccional y costo promedio ponderado por variante/bodega. Reintentos no duplican el ingreso. No incluye pago de cuentas a proveedores ni validación tributaria de sus documentos.
- Inventario: bodegas asociadas a una tienda, ingresos/ajustes, existencias, mínimos, alertas de reposición, traspasos con costo, conteos físicos y movimientos trazables. Mercadería en revisión y clasificación como disponible o merma. Búsqueda y filtros en existencias.
- Clientes: alta, edición, búsqueda, paginación y validación del RUT. Las ventas conservan los antecedentes del cliente al momento de confirmar.
- POS: carrito por SKU/modelo/talla, stock protegido contra sobreventa, pagos simples o combinados entre efectivo/tarjeta/transferencia/otros, validación del total en el servidor y confirmación idempotente.
- Caja: apertura por vendedor, fondo, ingresos/retiros, cuadratura de cuatro medios, observación ante diferencias, revisión de administrador e impresión. Pagos y reembolsos se contabilizan por su medio y sesión.
- Cuadratura diaria: selección de fecha de Chile y cajero (o todos para el administrador), cantidad y total de boletas/facturas, ingresos en efectivo/tarjetas/transferencias/otros medios y comparación del total del sistema con lo declarado. Agrega varias sesiones del día y muestra faltante, sobrante o coincidencia con diferencias por medio.
- Documentos internos: boletas, facturas, guías, notas de crédito y débito simuladas, referencias e impresión. No tienen folios autorizados ni validez tributaria.
- Devoluciones: cantidades parciales, control de unidades pendientes, reparto de reembolsos entre medios pagados y reingreso opcional o revisión. Un saldo interno procedente de un cambio se reembolsa en efectivo; exige fondo suficiente.
- Cambios de talla/color: una operación atómica entre variantes del mismo modelo, con devolución y nueva venta; solo cobra/reembolsa la diferencia. Si falta stock se revierte toda la operación. La unidad devuelta se reingresa disponible; para un artículo dañado usar devolución a revisión.
- Cobros: notas de débito internas con cobros parciales hasta completar el saldo. Cada cobro entra en la caja correspondiente sin duplicarse al reintentar.
- Reportes: rango de fechas de Chile, vendedor, marca, tienda, devolución atribuida a la venta original, ingreso neto, costo histórico y utilidad bruta estimada. Exportación Excel con hasta 100.000 líneas por consulta.
- Usuarios: creación, activación/desactivación y cambio de rol. Se protege al último administrador activo y se revocan sesiones al cambiar permisos o contraseña.
- Acceso renovado: login adaptable, acceso de equipo/administración, creación pública de cuentas pendientes de aprobación y opción protegida de crear administradores. Menú principal lateral izquierdo agrupado por área, sección activa y apertura móvil con teclado. Validación, mensajes de Identity y límites de acceso en español.
- Recuperación de acceso: enlace de un solo uso; correo local .eml en Development y transportador SMTP TLS configurable en producción. No se muestran tokens en la respuesta ni se enumeran cuentas.
- Auditoría: fecha, actor, entidad, registro y nombres de campos modificados; consulta paginada. No registra hashes, contraseñas ni tokens.
- Respaldos: COPY_ONLY/CHECKSUM, VERIFYONLY y prueba real de restauración/CHECKDB en una base aislada que se elimina al terminar. Nunca sobrescribe la base de trabajo.
- Operación: salud de SQL en `/salud`, configuración Production, scripts de publicación y respaldo, y paquete publicable.

Los medios de pago y reembolsos se registran manualmente. No hay conexión con terminal bancaria. La utilidad es bruta estimada: excluye impuestos, gastos generales, pérdidas por merma y ajustes documentales. Si el costo histórico no se conoce se muestra vacío, sin inventar utilidad.

## Ejecutar

Abrir `RetailPOS.slnx`, seleccionar Web como inicio y perfil https. Desde esta carpeta:

```powershell
dotnet tool restore
dotnet restore RetailPOS.slnx
dotnet build RetailPOS.slnx
dotnet run --project src/RetailPOS.Web -- --crear-base
dotnet run --project src/RetailPOS.Web
```

Abrir **https://localhost:7243**. Si no hay usuarios, seleccionar Crear administrador inicial y usar un correo y contraseña propios (12 caracteres, mayúscula, minúscula, número y símbolo). No hay cuentas predeterminadas. La creación inicial solo funciona en Development desde loopback; se deshabilita al existir un usuario y nunca se habilita en Production.

Con la tienda configurada, **Crear cuenta** registra una cuenta Vendedor inactiva, que un administrador activa desde Usuarios y permisos. No permite elegir rol ni activarse desde el formulario público. **Crear administrador** requiere una cuenta administrativa autorizada y lleva a la gestión de usuarios; no habilita el alta pública de administradores. El acceso conserva únicamente destinos locales y rechaza redirecciones externas.

`--crear-base` aplica migraciones y termina; solo permite la base llamada RetailPOS. El arranque normal no crea ni migra tablas. La conexión Development usa SQL local con autenticación Windows; sobrescribir mediante `ConnectionStrings__Tienda` o secretos del host. Las bases anteriores no se modifican.

El SDK elegido es 11.0.100 o una banda estable compatible según global.json. El frontend usa JavaScript/CSS locales y no requiere npm. HTTPS local debe tener certificado de desarrollo de confianza; si es necesario ejecutar personalmente `dotnet dev-certs https --trust`.

## Recorrido de uso

1. Crear administrador, marcas, tallas y modelos/variantes.
2. Crear bodegas y asignar sus tiendas. Registrar proveedores.
3. En Compras registrar mercadería y su costo; se actualiza el inventario. Para existencias anteriores sin costo, el administrador puede registrar el costo conocido y motivo desde Control de inventario. No se modifican costos de ventas pasadas.
4. Abrir caja, configurar ofertas y registrar ventas con uno o varios pagos.
5. Desde la venta tramitar devoluciones o cambios de talla. Desde Cobros registrar los ajustes pendientes.
6. Revisar alertas, hacer conteos/traspasos y resolver artículos en revisión.
7. Cuadrar y cerrar caja. El administrador consulta todas las sesiones y marca su revisión.
8. Filtrar reportes/exportar Excel y consultar auditoría.
9. Crear un respaldo y probar su restauración desde Administración.

## Recuperación de contraseña

En desarrollo los correos están en `.local/correos`, fuera de wwwroot y Git; abrir el .eml y usar el enlace. No se envían por Internet. En producción configurar SMTP y URL pública mediante secretos/variables. El certificado SMTP/HTTPS es independiente del certificado del SII.

## Pruebas

```powershell
dotnet build RetailPOS.slnx
dotnet run --project tests/RetailPOS.Verificacion --no-build
```

La verificación crea bases `RetailPOS_Test_<guid>`, las migra y elimina al finalizar. Requiere permisos SQL de creación/eliminación y, para verificar respaldos, backup/restore. No usa la base operativa. Puede sobrescribirse el servidor con `RETAILPOS_TEST_SQL`; el nombre aislado se genera siempre internamente.

Cubre migración con datos anteriores, concurrencia/idempotencia, ofertas y redondeos, compras/costos, traspasos/conteo/revisión, pagos combinados, cobros y límites, cambios atómicos/rollback, devoluciones, caja, autenticación/permisos/último administrador, recuperación por correo local, archivos Excel válidos, auditoría y restauración SQL real. Los .bak de prueba pueden quedar retenidos en el directorio del servidor SQL si la cuenta Windows no tiene permiso de eliminarlos. No se borran respaldos operativos.

La configuración Web usa `UseSharedCompilation=false` por un fallo inconsistente del compilador Razor observado en este equipo. Los importes de formularios HTML se interpretan con punto decimal independientemente del idioma del servidor.

## Publicación y respaldo

```powershell
./tools/Publicar.ps1
./tools/Respaldo.ps1
./tools/Respaldo.ps1 -ProbarId 1
```

La publicación genera `artifacts/publish-retail` y `artifacts/RetailPOS.zip`. No configura un servidor externo ni publica el sitio automáticamente. Para operar fuera del equipo hay que aportar dominio, HTTPS, SQL, SMTP y credenciales del alojamiento. No se crearon tareas programadas ni se envió correo externo.

Consultar [despliegue y recuperación](docs/despliegue-y-recuperacion.md) para configuración, permisos, respaldo externo y recuperación. Scripts SQL idempotentes en `database/`.

## Arquitectura y alcance

Domain contiene entidades/reglas; Application los contratos/solicitudes; Infrastructure implementa EF, Identity y transacciones; Web presenta formularios y permisos. Los controladores de consultas/catálogos usan EF directamente, y las operaciones que combinan dinero/inventario se concentran en Operaciones. Auditoría y cambios comerciales se guardan en la misma transacción.

No incluye integración SII, XML firmado, certificados tributarios, CAF ni emisión fiscal real. Tampoco se instalan servicios de alojamiento externos o se envían mensajes reales sin su configuración. La guía de despacho simulada se asocia a una venta ya entregada; no implementa despachos parciales ni guías previas a facturación.


## Cambio de nombre

El proyecto y la solución ahora se llaman RetailPOS y la marca visible es Punto de Venta Retail. Se conserva la conexión a la base existente ZapatillasPOS para mantener los datos; una instalación nueva puede usar RetailPOS mediante ConnectionStrings__Tienda. No se renombraron ni eliminaron bases de datos. Los respaldos anteriores siguen disponibles.


