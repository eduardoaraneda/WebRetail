# Cuadraturas, marcas, tallas y ofertas

## Marcas y tallas

El administrador crea y activa/desactiva los registros desde **Marcas y tallas**. Marca es un nombre único; talla se identifica por valor y sistema (EU/US/UK/CM). El catálogo los selecciona en vez de introducir textos libres. Las referencias en SQL impiden usar registros inexistentes. Desactivar bloquea nuevas ventas de esas variantes y conserva el historial.

La migración `MarcasTallasOfertasCuadratura` convierte los valores de productos/variantes anteriores en maestros y copia el precio histórico a PrecioLista. No elimina registros comerciales ni usuarios.

## Ofertas

- Solo el administrador crea y activa/desactiva promociones.
- Requieren al menos un filtro: marca, talla con sistema o SKU. Varios filtros se combinan con AND y debe existir al menos una variante activa coincidente al crear la oferta.
- Descuento por porcentaje (hasta dos decimales) o monto fijo por unidad en pesos enteros.
- Vigencia inclusiva por fecha civil en America/Santiago. Se respetan ofertas programadas, vencidas o desactivadas.
- Si coinciden varias ofertas, se elige el precio final menor; empate por ID más antiguo. No se acumulan descuentos.
- Redondeo del precio final al peso más próximo, con mitades alejándose de cero. Precio mínimo de $1, también cuando el monto descontado alcanza o supera el precio base.
- El POS muestra el precio calculado; al confirmar se vuelve a calcular en el servidor, dentro de la transacción. La oferta puede cambiar entre abrir la pantalla y confirmar.
- La línea guarda precio lista, precio final, descuento unitario, ID y nombre de oferta. Cambiar precios o desactivar promociones no altera ventas previas. Las devoluciones usan el precio final histórico.

## Caja y cuadratura

Cada vendedor tiene una sesión abierta como máximo. Un administrador puede ver todas las sesiones y marcar cierres como revisados; el vendedor solo consulta las propias. La marca de revisión registra usuario y fecha, sin cambiar los conteos.

Por cada medio:

`Esperado = Fondo + Ventas - Reembolsos + Ingresos - Retiros`

Solo efectivo lleva fondo inicial, ingresos y retiros adicionales. Los movimientos son positivos, con tipo Ingreso/Retiro y motivo; no representan ventas. Su clave de solicitud impide duplicar reintentos. Un retiro no puede exceder el efectivo esperado registrado.

Se declaran tres importes al cerrar: efectivo físico contado, tarjeta verificada y transferencias verificadas. Estos dos últimos son declaraciones manuales netas, no una integración bancaria. Pueden ser negativos si se reembolsa en esta sesión una venta de una sesión anterior. El efectivo contado no puede ser negativo.

`Diferencia = Declarado - Esperado`

Negativo significa faltante; positivo, sobrante. Si algún medio difiere, el cierre exige una observación. El cierre registra los tres resúmenes y no se recalcula con movimientos de sesiones posteriores. El formulario y detalle permiten imprimir la cuadratura.

El cierre, venta, devolución, apertura y movimientos adicionales coordinan un bloqueo de caja por usuario dentro de transacciones SQL. El cierre no puede tomar una fotografía parcial mientras se confirma una operación de la misma caja. La vista de una caja abierta es orientativa y puede cambiar hasta su cierre.

Los cierres previos al módulo no reciben conteos ficticios de tarjeta/transferencia: se muestran los valores de efectivo originales y un aviso de cierre anterior.

## Validación ejecutada

Compilación completa y verificación en SQL real con bases aisladas. Se comprueban migración de datos anteriores; filtros por marca/talla/SKU y sistema de talla; fechas, desactivación, mejor precio, redondeo; precio ofertado histórico y reembolso; ingresos/retiros idempotentes y retiro excesivo; obligación de observación; cierre con faltante; saldo negativo por devolución; creación por formularios HTTPS y restricciones por rol.

No se amplía el alcance tributario: los documentos continúan simulados y sin envío al SII.
