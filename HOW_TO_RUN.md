# Cómo ejecutar la aplicación LibroFiscal.Desktop

La aplicación principal es una aplicación de escritorio desarrollada en WPF (Windows Presentation Foundation) bajo la plataforma .NET 8.0.

Sigue estos pasos para compilar y ejecutar el proyecto desde la línea de comandos o usando herramientas de desarrollo.

## Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) o superior instalado en tu sistema.
- Sistema Operativo Windows (WPF es exclusivo de Windows).

## Paso 1: Abrir la terminal

Abre tu terminal preferida (Command Prompt, PowerShell o la terminal integrada de tu IDE) y navega hasta el directorio raíz del proyecto:

```bash
cd "c:\Users\Grupo B\Desktop\motor\motor-tributario"
```

## Paso 2: Navegar al directorio del proyecto de escritorio

La aplicación de escritorio se encuentra dentro de la carpeta `src\Clients\LibroFiscal.Desktop`. Debes moverte a este directorio:

```bash
cd src\Clients\LibroFiscal.Desktop
```

## Paso 3: Compilar y ejecutar

Para ejecutar la aplicación, simplemente utiliza el comando de la CLI de .NET:

```bash
dotnet run
```

Este comando se encargará de restaurar los paquetes NuGet necesarios, compilar todos los proyectos dependientes (Core, Infrastructure, etc.) y finalmente abrir la ventana de la aplicación.

### Alternativa: Especificar el archivo de proyecto

Si prefieres ejecutarlo sin cambiar de directorio (desde la raíz `motor-tributario`), puedes hacerlo pasando la ruta relativa del archivo `.csproj`:

```bash
dotnet run --project src\Clients\LibroFiscal.Desktop\LibroFiscal.Desktop.csproj
```

## Solución de problemas comunes

**Error de compilación "IDE0161: Convert to file-scoped namespace"**

Si al intentar compilar obtienes errores relacionados con los espacios de nombres (namespaces) en el proyecto `LibroFiscal.Persistence`, puedes solucionarlo ejecutando el formateador automático de .NET:

1. Navega a la carpeta de infraestructura:
   ```bash
   cd "c:\Users\Grupo B\Desktop\motor\motor-tributario\src\Infrastructure\LibroFiscal.Persistence"
   ```
2. Ejecuta el formateador de estilo para corregir el error:
   ```bash
   dotnet format style --diagnostics IDE0161
   ```
3. Vuelve a ejecutar la aplicación.

## Uso desde Visual Studio / Rider

Si prefieres utilizar un IDE:
1. Abre el archivo de solución `LibroFiscal.sln` ubicado en la raíz.
2. Haz clic derecho sobre el proyecto `LibroFiscal.Desktop` en el explorador de soluciones.
3. Selecciona **"Set as Startup Project"** (Establecer como proyecto de inicio).
4. Presiona `F5` o haz clic en el botón de "Start" para compilar y ejecutar.