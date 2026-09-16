# Inventario y plan de prueba física

No envíe trabajos hasta completar y revisar este documento para cada unidad.

## 1. Identificación inequívoca

Fotografíe/transcriba sin datos sensibles:

- Modelo exacto de la placa trasera/inferior: `GC420d`, `TLP 2844` o `TLP 2844-Z`.
- Número de serie (guárdelo en el inventario corporativo, no en Git).
- Tensión de la fuente y estado del cable.
- Firmware indicado por la etiqueta de configuración/autodiagnóstico.

**TLP 2844 ≠ TLP 2844-Z.** No infiera el lenguaje por la carcasa o por una cola existente. En este proyecto:

- `Tlp2844`: EPL2 solamente.
- `Tlp2844Z`: EPL2 o ZPL, después de confirmar placa/configuración.
- `Gc420d`: EPL2 o ZPL; prefiera el lenguaje ya estandarizado en el entorno.

## 2. Conectores y puerto

Marque lo observado, sin conectar dos interfaces simultáneamente:

- [ ] USB-B directo, sin hub.
- [ ] Serie DB9: cable/pinout, COM, baudios, bits, paridad, stop y flow control.
- [ ] Paralelo Centronics: LPT o modelo exacto del adaptador.
- [ ] Ethernet interno/servidor externo: MAC, IP, DHCP/reserva y protocolo.

Correlacione el conector físico con `Get-Printer ... PortName` y con `Inventory-Zebra.ps1`. Un nombre “USB” o “Zebra” no demuestra que sea la unidad correcta.

## 3. Autodiagnóstico sin host

1. Apague la impresora y retire trabajos/cola del host.
2. Cargue consumible correcto y compruebe sensores/cierre del cabezal.
3. Siga el manual oficial específico del modelo para imprimir la etiqueta de configuración mediante el botón **FEED** (la secuencia varía). No improvise una secuencia de otro modelo.
4. Capture modelo/firmware, lenguaje activo, dpi, sensores y parámetros de comunicación.
5. Cancele el modo diagnóstico según el manual y reinicie si procede.

El autodiagnóstico consume una etiqueta, pero no usa el PC y ayuda a separar fallos de hardware de fallos de cola.

## 4. Plan de smoke test supervisado

- [ ] Inventario revisado por dos personas o por el responsable del equipo.
- [ ] Cola vacía y no compartida; ventana de prueba aprobada.
- [ ] Modelo, lenguaje, ancho/alto en dots y consumible coinciden.
- [ ] CLI `validate` devuelve 0.
- [ ] CLI `smoke` sin `--send` muestra `DRY-RUN`, longitud y SHA-256.
- [ ] Operador está junto a la impresora y puede apagarla/cancelar la cola.
- [ ] Ejecutar una única vez con `--send`.
- [ ] Verificar una etiqueta: texto completo, orientación, no avance continuo.
- [ ] Registrar resultado externamente sin commitear seriales ni payloads.

## 5. Criterios de parada

Apague/cancele y no reintente automáticamente si: avance continuo, caracteres/comandos impresos como texto, cola bloqueada, modelo ambiguo, tamaño incorrecto, temperatura/ruido anormal o más de una etiqueta. Revise lenguaje, puerto, driver, dimensiones y calibración antes de otro intento.
