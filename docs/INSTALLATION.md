# Instalación en Windows 10/11 x64

> Este repositorio **no descarga ni incluye** drivers. Use únicamente fuentes oficiales y respete la licencia de Zebra.

## 1. Obtener el software oficial

1. Abra el portal de soporte oficial: <https://www.zebra.com/us/en/support-downloads.html>.
2. Busque exactamente el modelo de la placa: **GC420d**, **TLP 2844** o **TLP 2844-Z**.
3. En la sección Drivers, localice **ZDesigner v5** compatible con Windows x64. Compruebe editor/firma digital, versión, SO declarado y condiciones de licencia.
4. Si el portal ya no ofrece un paquete compatible, deténgase y escale a Zebra o al responsable de seguridad; no use mirrors ni paquetes reempaquetados.

No se automatiza esta descarga para evitar redistribución, URLs caducas y aceptación implícita de licencias.

## 2. Antes de instalar

- Complete `docs/INVENTORY_AND_PHYSICAL_PLAN.md`.
- Desconecte USB hasta que el instalador lo solicite.
- Cree un punto de restauración o siga el procedimiento corporativo.
- Verifique que no haya una cola homónima usada en producción.

## 3. Instalar y crear cola

Ejecute el instalador oficial como administrador siguiendo su asistente. Seleccione el **modelo exacto** y el puerto inventariado:

- USB: puerto `USB00x` creado para el dispositivo correcto.
- Serie: COM correcto y parámetros coincidentes en host/impresora.
- Paralelo: LPT/adaptador validado.
- Red: Standard TCP/IP con IP reservada; documente RAW 9100 u otro protocolo autorizado.

Asigne un nombre que contenga `ZDesigner`, porque la validación evita colas genéricas por accidente. En Propiedades de impresora → Avanzadas, confirme el driver ZDesigner esperado y un procesador/tipo RAW. **No pulse “Imprimir página de prueba” todavía.**

## 4. Instalar la CLI

Instale .NET 8 SDK desde <https://dotnet.microsoft.com/download/dotnet/8.0> y ejecute:

```powershell
dotnet restore .\ZebraLegacy.sln
dotnet build .\ZebraLegacy.sln -c Release --no-restore
dotnet test .\ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration"
Copy-Item .\profiles.example.json .\profiles.local.json
```

Edite `profiles.local.json` con la cola y el modelo comprobados. Primero valide y haga dry-run:

```powershell
dotnet run --project .\src\ZebraLegacy.Cli -- validate --config .\profiles.local.json
dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile almacen-gc420d
```

`--send` queda reservado al paso aprobado del plan físico.

## 5. Desinstalación

Elimine la CLI como archivos normales. La retirada de cola/driver es una acción administrativa separada: compruebe dependencias de otras colas y siga el procedimiento corporativo. No elimine el paquete del almacén de drivers a ciegas.
