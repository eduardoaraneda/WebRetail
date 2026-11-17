# Despliegue y recuperación

## Paquete preparado

`tools/Publicar.ps1` restaura, compila, ejecuta la verificación y publica Web en `artifacts/publish-retail`; genera `artifacts/RetailPOS.zip`. `-OmitirPruebas` solo evita repetir pruebas que ya se ejecutaron sobre la misma versión. El paquete no despliega ni abre puertos automáticamente.

En el servidor se requiere runtime ASP.NET Core 11; si se elige IIS, instalar el Hosting Bundle oficial correspondiente y configurar su sitio HTTPS. También se puede ejecutar con Kestrel detrás de un proxy HTTPS. El dominio, servidor y proveedor de correo reales dependen del entorno del propietario: este proyecto deja su configuración preparada y no configura un servidor externo inexistente.

## Configuración obligatoria de producción

- `ASPNETCORE_ENVIRONMENT=Production`.
- `ConnectionStrings__Tienda`: SQL propio con una cuenta de privilegios mínimos y certificado SQL válido. El nombre operativo es RetailPOS. No usar credenciales de administrador de SQL para la operación cotidiana.
- `AllowedHosts`: dominio real del sitio. La configuración base solo admite localhost/127.0.0.1.
- Configurar certificado HTTPS del dominio y la URL del sitio. El certificado HTTPS es independiente del certificado tributario del SII, que no se usa en este alcance.
- `Correo__Modo=Smtp`, `Correo__Servidor`, `Correo__Puerto` (587 por defecto), `Correo__Usuario`, `Correo__Password`, `Correo__Remitente`, `Correo__UrlPublica=https://dominio-real`.
- Mantener las claves de ASP.NET Data Protection en almacenamiento persistente con permisos adecuados. Al escalar a varias instancias, compartirlas y configurar límites de solicitudes distribuidos.

Los secretos se suministran mediante el almacén del host o variables de entorno; nunca en el repositorio. No publicar `.local`, bases SQL ni backups. La configuración inicial de administrador está cerrada en Production; preparar la cuenta desde desarrollo local sobre la base elegida, antes de exponer el sitio. No hay contraseñas predeterminadas.

## Migraciones

Detener la aplicación antes de reemplazar sus binarios. Tomar un respaldo previo y aplicar el SQL idempotente de `database/003_operacion_completa.sql` o ejecutar el comando `--crear-base` con el entorno y conexión correctos. El proceso normal no migra automáticamente la base.

La migración conserva usuarios activos, pagos y reembolsos anteriores, descripción del cliente, marcas y precios. No atribuye costos a ventas históricas cuando nunca fueron registrados. Los movimientos posteriores llevan auditoría; no se inventan actores de operaciones anteriores.

Probar `/salud`: devuelve 200 cuando SQL responde y 503 si no está disponible. Comprobar login, permisos, apertura de caja, venta y cierre con datos propios de prueba antes del uso comercial. En este entorno ya se probaron los flujos contra bases aisladas.

## Correo local y recuperación de contraseña

En Development los correos se generan como archivos `.eml` en `.local/correos`, fuera de wwwroot y de Git. Abrir el correo local y usar su enlace permite probar la recuperación sin proveedor externo. No es un envío por Internet.

En Production el transportador exige SMTP TLS configurado. Las solicitudes muestran la misma respuesta para cuentas inexistentes o desactivadas. Los tokens no se imprimen ni se devuelven en la respuesta. Cambiar la contraseña revoca los accesos anteriores; no se puede desactivar o degradar al último administrador activo.

## Respaldo y restauración comprobada

Desde Administración se crea un respaldo completo COPY_ONLY con CHECKSUM en el directorio de respaldo de SQL Server. `RESTORE VERIFYONLY` comprueba el archivo. Se registran fecha, actor, ruta y resultado.

La acción de probar restauración crea exclusivamente `RetailPOS_Recovery_<guid>`, restaura con nombres de archivos nuevos, ejecuta CHECKDB, verifica tablas esenciales y elimina esa base aislada. Nunca restaura sobre la base de trabajo ni usa REPLACE. SQL necesita permisos de restauración y creación de bases para esta acción.

También se puede ejecutar:

```powershell
./tools/Respaldo.ps1
./tools/Respaldo.ps1 -ProbarId 1
```

No se creó una tarea programada ni se elimina el historial de respaldos. Definir frecuencia y retención según el volumen y las necesidades reales; mantener otra copia fuera del equipo bajo control del propietario. Un respaldo ubicado solo en el mismo disco no protege de pérdida del equipo.

Ante un incidente, conservar el archivo, detener las escrituras y probar primero una restauración aislada. La sustitución de una base operativa es una decisión administrativa y no está expuesta como botón web.

## Límites de alcance

La aplicación registra pagos y reembolsos manuales: no captura tarjetas ni opera una terminal bancaria. Los documentos son internos simulados. No se conectan SII, certificados tributarios, folios ni emisión legal.

La utilidad reportada es bruta estimada y usa el costo promedio histórico de las unidades netas; excluye impuestos, gastos generales, merma y ajustes documentales. Los costos desconocidos se muestran vacíos.


