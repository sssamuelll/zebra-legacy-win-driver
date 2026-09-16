# ADR-0001: reutilizar ZDesigner v5 y el spooler de Windows

- Estado: aceptada
- Fecha: 2026-09-16

## Contexto

GC420d y TLP 2844 son impresoras legacy. El objetivo es mantener una ruta de impresión controlable en Windows 10/11 x64 sin desarrollar ni distribuir un driver. Un driver kernel exige especialización, firma, mantenimiento de seguridad y una matriz de compatibilidad desproporcionada.

## Preflight de alternativas

Se revisaron brevemente las opciones existentes:

1. **ZDesigner v5 oficial + spooler Windows:** solución del fabricante para estas generaciones; mantiene cola, puerto y compatibilidad del SO.
2. **Zebra Setup Utilities:** útil para instalación/diagnóstico, pero no sustituye una interfaz automatizable y testeable del proyecto.
3. **SDKs/Browser Print o librerías genéricas:** añaden dependencias y no resuelven la instalación del driver legacy; para un payload EPL2/ZPL pequeño, la API RAW del spooler ya es suficiente.
4. **Driver propio, CUPS o rediseño kernel:** coste y riesgo altos, sin ventaja para el alcance Windows solicitado.

La disponibilidad y licencia del paquete exacto deben verificarse en el portal oficial de Zebra al desplegar; el proyecto no fija ni copia binarios externos.

## Decisión

Reutilizar el driver **ZDesigner v5 oficial** instalado por un administrador y el spooler de Windows. Generar EPL2/ZPL en un núcleo portable y enviar bytes RAW solo por petición explícita. No descargar, empaquetar, modificar ni redistribuir binarios Zebra.

## Consecuencias

- Menor superficie de código privilegiado y mantenimiento.
- La instalación y compatibilidad final dependen de Zebra y Windows.
- La cola debe usar el modelo, puerto y procesador RAW correctos.
- Cada equipo requiere inventario y prueba física supervisada.
- No se promete soporte del fabricante ni firma de este proyecto.
