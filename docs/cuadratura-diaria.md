# Cuadratura diaria de ventas e ingresos

## Consulta

En Caja se selecciona la fecha civil de Chile. El vendedor consulta sus operaciones; el administrador puede elegir un cajero o todos. Se agrupan todas las sesiones del día, sin duplicar documentos por pagos combinados ni contar guías como ventas adicionales.

Se muestran cantidad y total de boletas y facturas, sus sumas, devoluciones/notas de crédito y venta neta. La tabla de ingresos separa efectivo, tarjetas, transferencias y otros medios, con operaciones de pago, ingresos de ventas, cobros de débito, reembolsos y total neto.

Los documentos continúan simulados: estos totales no son declaraciones al SII.

## Comparación

```
Venta neta = total boletas + total facturas - notas de crédito
Total neto del sistema = venta neta + cobros efectivos de notas de débito
Total bruto declarado = efectivo + tarjetas + transferencias + otros
Total neto declarado = total bruto declarado - fondo inicial - ingresos adicionales + retiros
Diferencia total = total neto declarado - total neto del sistema
```

No se cuenta como ingreso una nota de débito todavía impaga. Los saldos de cambios de talla no son dinero nuevo: la nota de crédito y nueva venta compensan el movimiento interno, dejando solo la diferencia efectivamente cobrada o reembolsada.

Sin fondos, movimientos ni devoluciones, la comparación es directamente entre el total vendido y la suma ingresada por el cajero.

## Cierre

El cajero declara los cuatro medios. La pantalla calcula en vivo la suma, el importe ajustado y la diferencia; el servidor vuelve a calcularlos dentro de la transacción antes de guardar. Una diferencia, total o por medio, requiere observación y se conserva como faltante o sobrante: nunca se cambia el conteo para forzar una igualdad.

Si el total general coincide pero hay diferencias que se compensan entre medios, la cuadratura avisa que hay que revisar los medios; no la presenta como completamente cuadrada. Si los documentos no coinciden con los pagos internos, el cierre se rechaza para revisar las operaciones.

El resumen del día permanece pendiente mientras haya sesiones abiertas o declaraciones históricas incompletas. No se mezclan importes declarados de otros días. Cada nueva sesión tiene fecha operativa; una caja de un día anterior debe cerrarse antes de registrar ventas, cobros, reembolsos o movimientos de hoy.

El detalle de una caja incluye el mismo desglose e impresión. Los cierres anteriores conservan sus importes: no se inventan declaraciones de Otros cuando ese medio todavía no existía.

## Migración y pruebas

Se agrega FechaOperativa a partir de la apertura convertida a hora de Chile y se habilita Otros en pagos/reembolsos. No se borran ventas ni se alteran los conteos históricos.

Las pruebas con SQL real cubren boletas/facturas con cantidades distintas de unidades vendidas, pago combinado, guía sin doble venta, nota de crédito parcial, débito parcialmente cobrado, cuatro medios, fondo/ingreso/retiro, varias sesiones, faltante de $1, separación por cajero, total de todos los cajeros, fecha de Chile y bloqueo de venta con caja del día anterior.
