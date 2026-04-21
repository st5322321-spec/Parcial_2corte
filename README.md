## Autor
Juan Sebastian Tovar Estrada
Yeiser Kaleth Mena Martinez

# Parqueo

Sistema de gestión de parqueadero desarrollado en **C# con .NET 10**.

---

## Descripción

Aplicación de consola que permite administrar un parqueadero: registrar entradas y salidas de vehículos, visualizar el mapa del parqueadero y consultar estadísticas de uso.

---

## Estructura del proyecto

```
parqueo/
├── parqueo.sln         # Archivo de solución de Visual Studio
├── parqueo.csproj      # Archivo de proyecto .NET
├── Program.cs          # Punto de entrada de la aplicación
└── ...                 # Clases del proyecto (Core / UI)
```

---

## Configuración del proyecto (`parqueo.csproj`)

```xml
<OutputType>Exe</OutputType>
```
Indica que el proyecto compila como **ejecutable** (aplicación de consola), no como librería.

```xml
<TargetFramework>net10.0</TargetFramework>
```
El proyecto apunta a **.NET 10**, la versión del runtime necesaria para compilar y ejecutar la aplicación.

```xml
<ImplicitUsings>enable</ImplicitUsings>
```
Habilita los **usings implícitos**: directivas `using` comunes (como `System`, `System.Collections.Generic`, etc.) se agregan automáticamente sin necesidad de escribirlas en cada archivo.

```xml
<Nullable>enable</Nullable>
```
Activa el **contexto de referencia nullable**, lo que permite al compilador advertir cuando una variable de tipo referencia podría ser `null`, ayudando a evitar errores en tiempo de ejecución.

```xml
<RootNamespace>Parqueo</RootNamespace>
```
Define `Parqueo` como el **espacio de nombres raíz** del proyecto. Todos los archivos que no declaren namespace explícito lo heredan.

```xml
<AssemblyName>parqueo</AssemblyName>
```
Nombre del **ensamblado** (archivo `.exe` o `.dll`) que se genera al compilar.

```xml
<Optimize>true</Optimize>
```
Activa las **optimizaciones del compilador**, generando un ejecutable más rápido y eficiente en producción.

---

## Punto de entrada (`Program.cs`)

```csharp
using Parqueo.Core;
using Parqueo.UI;
```
Importa los **espacios de nombres** del proyecto:
- `Parqueo.Core` — contiene la lógica de negocio (mapa, manager, estadísticas).
- `Parqueo.UI` — contiene la interfaz de usuario por consola (menú).

```csharp
var map = new ParkingMap();
```
Crea una instancia de `ParkingMap`, que representa el **mapa físico del parqueadero**: la distribución de espacios disponibles y ocupados.

```csharp
var manager = new ParkingManager(map);
```
Crea una instancia de `ParkingManager`, el componente encargado de la **lógica de gestión**: registrar entradas, procesar salidas y controlar la disponibilidad. Recibe el `map` para saber con qué parqueadero trabaja.

```csharp
var stats = new Statistics(map, manager);
```
Crea una instancia de `Statistics`, responsable de **calcular y mostrar métricas** del parqueadero (ocupación, ingresos, historial). Recibe tanto el mapa como el manager para acceder a todos los datos necesarios.

```csharp
var menu = new MenuHandler(manager, stats);
```
Crea una instancia de `MenuHandler`, que administra el **menú interactivo por consola**. Recibe el manager y las estadísticas para poder ejecutar cada opción del menú.

```csharp
menu.Run();
```
**Inicia la aplicación**: muestra el menú y entra en el ciclo principal donde el usuario interactúa con el sistema hasta decidir salir.

---

## Arquitectura

El proyecto sigue una separación en dos capas:

| Capa | Namespace | Responsabilidad |
|------|-----------|-----------------|
| Núcleo (Core) | `Parqueo.Core` | Lógica de negocio: mapa, gestión, estadísticas |
| Interfaz (UI) | `Parqueo.UI` | Presentación: menú y entrada/salida por consola |

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## Cómo ejecutar

```bash
# Clonar el repositorio
git clone <url-del-repo>
cd parqueo

# Compilar
dotnet build

# Ejecutar
dotnet run
```

---

## Cómo publicar (ejecutable independiente)

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Reemplaza `win-x64` por `linux-x64` o `osx-x64` según tu sistema operativo.
