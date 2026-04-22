## Autor
Juan Sebastian Tovar Estrada
Yeiser Kaleth Mena Martinez

# Sistema de Gestión de Parqueadero — Documentación Técnica Detallada

**Autores:** Juan Sebastian Tovar Estrada · Yeiser Kaleth Mena Martinez  
**Tecnología:** C# · .NET 10 · Aplicación de Consola

---

## Tabla de Contenidos

1. [Descripción general del proyecto](#1-descripción-general-del-proyecto)
2. [Estructura de archivos](#2-estructura-de-archivos)
3. [Arquitectura](#3-arquitectura)
4. [Program.cs — Punto de entrada](#4-programcs--punto-de-entrada)
5. [Vehicle.cs — Modelo de vehículo](#5-vehiclecs--modelo-de-vehículo)
6. [ParkingSpot.cs — Espacio de parqueo](#6-parkingspotcs--espacio-de-parqueo)
7. [Transaction.cs — Transacción de cobro](#7-transactioncs--transacción-de-cobro)
8. [parking_maps.cpp — Mapa del parqueadero](#8-parking_mapscpp--mapa-del-parqueadero)
9. [parking_managers.cpp — Gestor de parqueadero](#9-parking_managerscpp--gestor-de-parqueadero)
10. [Statistics.cs — Estadísticas](#10-statisticscs--estadísticas)
11. [PlateValidator.cs — Validador de placas](#11-platevalidatorcs--validador-de-placas)
12. [FileManager.cs — Gestión de archivos](#12-filemanagercs--gestión-de-archivos)
13. [ConsoleUI.cs — Interfaz de consola](#13-consoleuics--interfaz-de-consola)
14. [MapRenderer.cs — Renderizador del mapa](#14-maprenderercs--renderizador-del-mapa)
15. [MenuHandler.cs — Menú principal](#15-menuhandlercs--menú-principal)
16. [Flujo completo de la aplicación](#16-flujo-completo-de-la-aplicación)
17. [Tarifas del sistema](#17-tarifas-del-sistema)
18. [Cómo ejecutar el proyecto](#18-cómo-ejecutar-el-proyecto)

---

## 1. Descripción general del proyecto

**Parqueo** es una aplicación de consola que simula el sistema de administración de un parqueadero real. Permite:

- Registrar la **entrada** de vehículos (motos, carros, camiones) y asignarles un espacio automáticamente.
- Registrar la **salida** de vehículos y calcular el cobro según el tiempo transcurrido.
- Visualizar un **mapa interactivo** del parqueadero con colores en consola.
- Consultar **estadísticas** en tiempo real: ocupación, ingresos, vehículos por tipo.
- Ver el **historial** de todas las transacciones del día.
- **Exportar** al cerrar: un archivo CSV con el historial y un reporte de cierre en texto.

---

## 2. Estructura de archivos

```
parqueo/
├── Program.cs              ← Punto de entrada de la aplicación
│
├── Models/                 ← Representación de datos (entidades del dominio)
│   ├── Vehicle.cs          ← Modelo de vehículo
│   ├── ParkingSpot.cs      ← Modelo de espacio de parqueo
│   └── Transaction.cs      ← Modelo de transacción de cobro
│
├── Core/                   ← Lógica de negocio
│   ├── parking_maps.cpp    ← Mapa físico del parqueadero (ParkingMap)
│   ├── parking_managers.cpp← Gestión de entradas/salidas (ParkingManager + Rates)
│   └── Statistics.cs       ← Cálculo de métricas y reportes
│
├── UI/                     ← Interfaz de usuario por consola
│   ├── ConsoleUI.cs        ← Utilidades de impresión con colores ANSI
│   ├── MapRenderer.cs      ← Dibuja el mapa en consola
│   └── MenuHandler.cs      ← Menú interactivo y flujos de usuario
│
└── Utils/                  ← Herramientas auxiliares
    ├── PlateValidator.cs   ← Valida y formatea placas colombianas
    └── FileManager.cs      ← Exporta CSV y guarda reportes
```

---

## 3. Arquitectura

El proyecto sigue una separación en capas clara:

| Capa | Namespace | Responsabilidad |
|------|-----------|-----------------|
| Modelos | `Parqueo.Models` | Representan los datos: vehículo, espacio, transacción |
| Núcleo | `Parqueo.Core` | Lógica de negocio: mapa, gestión de parqueo, estadísticas |
| Interfaz | `Parqueo.UI` | Presentación: menú, mapa visual, utilidades de consola |
| Utilidades | `Parqueo.Utils` | Validación de placas, manejo de archivos |

---

## 4. Program.cs — Punto de entrada

```csharp
using Parqueo.Core;
using Parqueo.UI;
```
Importa los namespaces necesarios. `Parqueo.Core` contiene la lógica de negocio y `Parqueo.UI` contiene el menú interactivo.

```csharp
var map = new ParkingMap();
```
Crea el **mapa físico** del parqueadero: una cuadrícula de 18×20 celdas que representa la distribución real de espacios, vías, entrada y salida. Al instanciarse, se construye automáticamente con su disposición de bloques (A–I).

```csharp
var manager = new ParkingManager(map);
```
Crea el **gestor de parqueadero**, que administra todas las operaciones sobre el mapa: registrar entradas, registrar salidas, buscar vehículos y calcular tarifas. Recibe el `map` como dependencia.

```csharp
var stats = new Statistics(map, manager);
```
Crea el **módulo de estadísticas**, que consume datos tanto del mapa (ocupación) como del manager (historial de transacciones) para producir métricas y el reporte de cierre.

```csharp
var menu = new MenuHandler(manager, stats);
```
Crea el **manejador del menú**, que controla toda la interacción con el usuario: muestra las opciones, recibe entradas, y ejecuta los flujos correspondientes.

```csharp
menu.Run();
```
**Inicia el ciclo principal** de la aplicación. El programa permanece aquí hasta que el usuario confirme la salida.

---

## 5. Vehicle.cs — Modelo de vehículo

### Enumeración `VehicleType`

```csharp
public enum VehicleType { Moto, Carro, Camion }
```
Define los tres tipos de vehículos que acepta el parqueadero. Cada tipo tiene su propia tarifa y su propio ícono en el mapa.

### Clase `Vehicle`

```csharp
public string Plate { get; set; }
```
La placa del vehículo, siempre almacenada en **mayúsculas sin guiones** (normalizada).

```csharp
public VehicleType Type { get; set; }
```
El tipo de vehículo (Moto, Carro o Camion), que determina la tarifa a cobrar.

```csharp
public DateTime EntryTime { get; set; }
```
La fecha y hora exacta en que el vehículo entró al parqueadero. Se usa para calcular cuánto tiempo lleva estacionado y cuánto se le debe cobrar al salir.

```csharp
public int SpotId { get; set; }
```
El número de espacio asignado al vehículo dentro del mapa.

```csharp
public Vehicle(string plate, VehicleType type, int spotId)
{
    Plate = plate.ToUpper().Trim();   // Normaliza la placa a mayúsculas
    Type = type;
    EntryTime = DateTime.Now;         // Registra la hora actual como hora de entrada
    SpotId = spotId;
}
```
El constructor registra automáticamente la hora de entrada en el momento en que se crea el objeto. La placa se limpia de espacios y se convierte a mayúsculas.

```csharp
public string GetElapsedTime()
{
    TimeSpan elapsed = DateTime.Now - EntryTime;
    if (elapsed.TotalHours >= 1)
        return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}min";
    return $"{elapsed.Minutes}min {elapsed.Seconds}s";
}
```
Calcula el tiempo transcurrido desde la entrada hasta ahora. Si ha pasado una hora o más, muestra horas y minutos. Si es menos de una hora, muestra minutos y segundos. Se usa para mostrar el tiempo en la lista de vehículos y en las estadísticas.

```csharp
public string GetIcon()
{
    return Type switch
    {
        VehicleType.Moto   => "M",
        VehicleType.Carro  => "C",
        VehicleType.Camion => "T",
        _                  => "?"
    };
}
```
Devuelve un carácter de una sola letra que representa al vehículo en el mapa visual. Las motos muestran "M", los carros "C" y los camiones "T" (de Truck).

```csharp
public override string ToString()
    => $"[{Type}] {Plate} — Entrada: {EntryTime:HH:mm:ss} — Espacio #{SpotId}";
```
Representación en texto del vehículo, útil para depuración y registros.

---

## 6. ParkingSpot.cs — Espacio de parqueo

### Enumeración `SpotStatus`

```csharp
public enum SpotStatus { Free, Occupied }
```
Estado binario de un espacio: libre u ocupado.

### Enumeración `CellType`

```csharp
public enum CellType
{
    Wall,     // Muro o borde — celda bloqueada, no transitable
    Road,     // Vía de acceso — donde los vehículos circulan
    Spot,     // Espacio de parqueo — donde se estacionan los vehículos
    Entry,    // Celda especial marcada como entrada al parqueadero
    Exit,     // Celda especial marcada como salida del parqueadero
    Divider   // Separador visual entre bloques
}
```
Define el rol de cada celda en la cuadrícula del mapa. La clase `ParkingSpot` se usa para **todos** los tipos de celda, no solo para los espacios de parqueo, ya que el mapa completo es un arreglo de esta clase.

### Clase `ParkingSpot`

```csharp
public int Id { get; set; }       // Número de espacio (ej: 1, 2, 42). -1 si no es espacio real
public int Row { get; set; }      // Fila en la cuadrícula (0–17)
public int Col { get; set; }      // Columna en la cuadrícula (0–19)
public CellType CellType { get; set; }  // Qué tipo de celda es
public SpotStatus Status { get; set; } // Si está libre u ocupado
public Vehicle? OccupiedBy { get; set; } // Referencia al vehículo estacionado (null si libre)
public char Block { get; set; }   // Bloque al que pertenece (A, B, C, ... I)
```

```csharp
public ParkingSpot(int row, int col, CellType cellType, int id = -1, char block = ' ')
```
El constructor acepta parámetros opcionales con valores por defecto: `id = -1` indica que la celda no es un espacio de parqueo (es una pared, vía, etc.), y `block = ' '` indica que no pertenece a ningún bloque.

```csharp
public bool IsAvailable => CellType == CellType.Spot && Status == SpotStatus.Free;
```
Una celda está disponible para estacionarse **solo si** es de tipo `Spot` Y está libre. Esto evita que una vía o pared sea considerada como espacio disponible.

```csharp
public bool IsOccupied => CellType == CellType.Spot && Status == SpotStatus.Occupied;
```
Análogamente, un espacio está ocupado solo si es de tipo `Spot` y tiene estado `Occupied`.

```csharp
public void AssignVehicle(ref Vehicle vehicle)
{
    Status = SpotStatus.Occupied;
    OccupiedBy = vehicle;
}
```
Asigna un vehículo al espacio: cambia el estado a `Occupied` y guarda la referencia al vehículo. Se usa `ref` para evitar copiar la estructura si el compilador optimiza el acceso.

```csharp
public Vehicle? FreeSpot()
{
    Vehicle? v = OccupiedBy;
    Status = SpotStatus.Free;
    OccupiedBy = null;
    return v;
}
```
Libera el espacio: guarda temporalmente la referencia al vehículo, limpia el estado y la referencia, y devuelve el vehículo. El llamador puede usar ese vehículo para crear la transacción de cobro.

---

## 7. Transaction.cs — Transacción de cobro

Representa un cobro completado: la historia de un vehículo desde que entró hasta que pagó y salió.

```csharp
public int Id { get; set; }                // Número de transacción (1, 2, 3...)
public string Plate { get; set; }          // Placa del vehículo
public VehicleType VehicleType { get; set; } // Tipo (para estadísticas por categoría)
public DateTime EntryTime { get; set; }    // Hora de entrada (tomada del Vehicle)
public DateTime ExitTime { get; set; }     // Hora de salida (DateTime.Now al salir)
public TimeSpan Duration { get; set; }     // Tiempo total: ExitTime - EntryTime
public decimal AmountPaid { get; set; }    // Cuánto pagó en COP
public int SpotId { get; set; }            // En qué espacio estuvo
```

```csharp
public Transaction(int id, Vehicle vehicle, decimal amountPaid)
{
    Id          = id;
    Plate       = vehicle.Plate;
    VehicleType = vehicle.Type;
    EntryTime   = vehicle.EntryTime;       // Se copia la hora de entrada del vehículo
    ExitTime    = DateTime.Now;            // Se registra la hora actual como salida
    Duration    = ExitTime - EntryTime;    // Se calcula la diferencia automáticamente
    AmountPaid  = amountPaid;
    SpotId      = vehicle.SpotId;
}
```
El constructor toma un `Vehicle` completo y extrae todos sus datos relevantes. Al momento de crear la transacción, el vehículo ya fue eliminado del espacio, por eso se copian sus datos aquí antes de perder la referencia.

```csharp
public string GetDurationString()
{
    if (Duration.TotalHours >= 1)
        return $"{(int)Duration.TotalHours}h {Duration.Minutes}min";
    if (Duration.TotalMinutes >= 1)
        return $"{Duration.Minutes}min {Duration.Seconds}s";
    return $"{Duration.Seconds}s";
}
```
Formatea la duración de manera legible para el historial: puede mostrar "2h 15min", "45min 30s" o simplemente "20s" dependiendo del tiempo total.

```csharp
public string ToCsvLine()
    => $"{Id},{Plate},{VehicleType},{EntryTime:yyyy-MM-dd HH:mm:ss}," +
       $"{ExitTime:yyyy-MM-dd HH:mm:ss},{GetDurationString()},{AmountPaid},{SpotId}";
```
Convierte la transacción a una línea de texto en formato CSV para ser exportada al archivo del día.

```csharp
public static string CsvHeader()
    => "Id,Placa,TipoVehiculo,Entrada,Salida,Duracion,ValorPagado,Espacio";
```
Encabezado del CSV. Es `static` porque no necesita una instancia específica para generarse.

---

## 8. parking_maps.cpp — Mapa del parqueadero

> *Nota: a pesar de la extensión `.cpp`, el archivo contiene código C# válido.*

Este archivo define la clase `ParkingMap`, que es el núcleo físico del parqueadero.

### Constantes del mapa

```csharp
public const int ROWS = 18;
public const int COLS = 20;
```
El mapa tiene **18 filas × 20 columnas = 360 celdas** en total. Las constantes son `public` para que el renderizador pueda iterar sobre el mapa sin hardcodear estos valores.

### Campos privados

```csharp
private readonly ParkingSpot[,] _grid;
```
Cuadrícula bidimensional que contiene **todas** las celdas: paredes, vías, espacios, entrada y salida. El índice es `[fila, columna]`.

```csharp
private readonly List<ParkingSpot> _spots;
```
Lista separada que contiene **únicamente los espacios de parqueo** (celdas de tipo `Spot`). Esto permite buscar espacios libres sin recorrer toda la cuadrícula.

```csharp
private int _nextSpotId = 1;
```
Contador autoincremental que asigna IDs únicos a cada espacio de parqueo a medida que se construye el mapa.

### Propiedades calculadas

```csharp
public int TotalSpots    => _spots.Count;
public int FreeSpots     => _spots.Count(s => s.Status == SpotStatus.Free);
public int OccupiedSpots => _spots.Count(s => s.Status == SpotStatus.Occupied);
```
Cuentan los espacios usando LINQ sobre la lista `_spots`. Se calculan en tiempo real cada vez que se acceden.

### Métodos de búsqueda

```csharp
public ParkingSpot? FindFirstFreeSpot()
    => _spots.FirstOrDefault(s => s.IsAvailable);
```
Devuelve el **primer espacio libre** disponible (en orden de creación) o `null` si el parqueadero está lleno. Este es el método que llama `RegisterEntry` para asignar automáticamente un espacio.

```csharp
public ParkingSpot? FindSpotByPlate(string plate)
    => _spots.FirstOrDefault(s => s.IsOccupied &&
           s.OccupiedBy!.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase));
```
Busca un vehículo por placa sin importar mayúsculas/minúsculas. Devuelve el espacio donde está estacionado, o `null` si la placa no está en el parqueadero.

### Método `InitializeMap()`

Este método construye toda la disposición del parqueadero. Se llama en el constructor y define dónde va cada elemento:

```csharp
for (int r = 0; r < ROWS; r++)
    for (int c = 0; c < COLS; c++)
        _grid[r, c] = new ParkingSpot(r, c, CellType.Wall);
```
**Primero**, todo el mapa se llena de paredes (`Wall`). Luego, en los pasos siguientes, se "abre" espacio para vías, bloques y entradas.

```csharp
SetRoad(0, 0, COLS - 1);
```
La **fila 0** completa se convierte en vía: es la carretera principal de acceso en la parte superior.

```csharp
_grid[0, 0]  = new ParkingSpot(0, 0,  CellType.Entry);
_grid[0, 19] = new ParkingSpot(0, 19, CellType.Exit);
```
En la fila 0, la esquina izquierda es la **Entrada (EN)** y la esquina derecha es la **Salida (SA)**.

```csharp
for (int r = 1; r < ROWS; r++)
{
    _grid[r, 0]  = new ParkingSpot(r, 0,  CellType.Road);
    _grid[r, 19] = new ParkingSpot(r, 19, CellType.Road);
}
```
Las columnas 0 y 19 (extremos izquierdo y derecho) son vías verticales en todas las filas. Son los carriles laterales del parqueadero.

```csharp
SetRoad(9, 0, COLS - 1);   // Vía horizontal central (fila 9)
SetRoad(17, 0, COLS - 1);  // Vía horizontal inferior (fila 17)
```
Se crean dos vías horizontales adicionales: una en el centro del mapa (separa la zona superior de la inferior) y otra al fondo.

```csharp
for (int r = 0; r < ROWS; r++)
    if (_grid[r, 10].CellType == CellType.Wall)
        _grid[r, 10] = new ParkingSpot(r, 10, CellType.Road);
```
La columna 10 se convierte en vía vertical central, pero solo en las celdas que todavía sean pared. Esto crea un pasillo vertical que divide el mapa en mitad izquierda y mitad derecha.

```csharp
SetRoad(5, 1, 9);    // Vía horizontal entre bloques A/B y C/D (izquierda)
SetRoad(5, 11, 18);  // Ídem para la mitad derecha
SetRoad(13, 1, 9);   // Vía horizontal entre bloques C/D y F/G (izquierda)
SetRoad(13, 11, 18); // Ídem para la mitad derecha
```
Vías horizontales adicionales que separan las filas de bloques entre sí.

```csharp
AddBlock('A', 1, 4, 1, 4);     // Bloque A: filas 1–4, columnas 1–4
AddBlock('B', 1, 4, 6, 9);     // Bloque B: filas 1–4, columnas 6–9
AddBlock('C', 6, 8, 1, 4);     // Bloque C: filas 6–8, columnas 1–4
AddBlock('D', 6, 8, 6, 9);     // Bloque D: filas 6–8, columnas 6–9
AddBlock('E', 1, 4, 12, 15);   // Bloque E: filas 1–4, columnas 12–15
AddBlock('F', 10, 12, 1, 4);   // Bloque F: filas 10–12, columnas 1–4
AddBlock('G', 10, 12, 6, 9);   // Bloque G: filas 10–12, columnas 6–9
AddBlock('H', 10, 12, 12, 15); // Bloque H: filas 10–12, columnas 12–15
AddBlockWide('I', 14, 16, 1, 17); // Bloque I: zona inferior ancha
```
Se crean los **bloques de parqueo A–I**. Cada bloque es un rectángulo de espacios. El bloque I es más ancho porque abarca casi toda la anchura del mapa en las filas inferiores.

### Método `AddBlock`

```csharp
private void AddBlock(char blockName, int r1, int r2, int c1, int c2)
{
    for (int r = r1; r <= r2; r++)
        for (int c = c1; c <= c2; c++)
        {
            var spot = new ParkingSpot(r, c, CellType.Spot, _nextSpotId++, blockName);
            _grid[r, c] = spot;   // Reemplaza la pared en la cuadrícula
            _spots.Add(spot);     // Agrega a la lista de espacios buscables
        }
}
```
Itera sobre el rectángulo definido y crea un `ParkingSpot` de tipo `Spot` en cada celda, asignándole un ID único y el nombre del bloque. Cada espacio se agrega tanto a la cuadrícula como a la lista de búsqueda.

---

## 9. parking_managers.cpp — Gestor de parqueadero

> *Nota: a pesar de la extensión `.cpp`, el archivo contiene código C# válido. Define dos clases: `Rates` y `ParkingManager`.*

### Clase `Rates` — Tarifas

```csharp
public const decimal MotoHourly   = 1_500m;
public const decimal CarroHourly  = 3_000m;
public const decimal CamionHourly = 5_000m;
```
Tarifas por hora completa en pesos colombianos (COP). El sufijo `m` indica que son literales de tipo `decimal`, que es más preciso que `double` para dinero.

```csharp
public const decimal MotoFraction   = 750m;
public const decimal CarroFraction  = 1_500m;
public const decimal CamionFraction = 2_500m;
```
Tarifas por **fracción** (cualquier porción de hora incompleta). Por ejemplo, si un carro estuvo 1 hora y 20 minutos, paga $3,000 (la hora completa) + $1,500 (la fracción de los 20 minutos).

```csharp
public static decimal GetHourlyRate(VehicleType type) => type switch { ... };
public static decimal GetFractionRate(VehicleType type) => type switch { ... };
```
Métodos de consulta que devuelven la tarifa correspondiente al tipo de vehículo usando expresiones `switch`.

### Clase `ParkingManager`

```csharp
private readonly ParkingMap        _map;
private readonly List<Transaction> _history;
private int                        _transactionCounter;
```
Los tres estados internos del manager: el mapa (para buscar y asignar espacios), el historial de cobros completados, y un contador que asigna IDs únicos a las transacciones.

```csharp
public IReadOnlyList<Transaction> History => _history.AsReadOnly();
```
El historial se expone como `IReadOnlyList` para que el código externo pueda **leerlo pero no modificarlo** directamente.

### Método `RegisterEntry`

```csharp
public (bool success, string message, ParkingSpot? spot) RegisterEntry(
    string plate, VehicleType type)
```
Retorna una tupla con tres valores: si tuvo éxito, un mensaje descriptivo, y el espacio asignado (si hubo éxito).

```csharp
var existing = _map.FindSpotByPlate(plate);
if (existing != null)
    return (false, $"La placa {plate} ya está registrada...", null);
```
**Primera validación:** evita que el mismo vehículo entre dos veces. Si la placa ya existe en el parqueadero, la operación falla.

```csharp
ParkingSpot? spot = _map.FindFirstFreeSpot();
if (spot == null)
    return (false, "No hay espacios disponibles...", null);
```
**Segunda validación:** si el parqueadero está lleno, la operación falla.

```csharp
var vehicle = new Vehicle(plate, type, spot.Id);
spot.AssignVehicle(ref vehicle);
```
Se crea el vehículo (registrando la hora de entrada automáticamente) y se asigna al espacio encontrado.

### Método `RegisterExit`

```csharp
ParkingSpot? spot = _map.FindSpotByPlate(plate);
if (spot == null)
    return (false, null, $"No se encontró...");
```
Busca el vehículo por placa. Si no existe, la operación falla.

```csharp
Vehicle vehicle = spot.OccupiedBy!;
decimal amount  = CalculateFee(ref vehicle);
spot.FreeSpot();
```
Guarda la referencia al vehículo, calcula el cobro, y **libera el espacio** (en ese orden, porque `FreeSpot()` limpia `OccupiedBy`).

```csharp
var transaction = new Transaction(_transactionCounter++, vehicle, amount);
_history.Add(transaction);
```
Crea la transacción con los datos del vehículo (ya desconectado del mapa) y la agrega al historial.

### Método `CalculateFee`

```csharp
TimeSpan elapsed      = DateTime.Now - vehicle.EntryTime;
double   totalMinutes = elapsed.TotalMinutes;

if (totalMinutes < 1)
    return 0m;
```
Si el vehículo estuvo menos de un minuto, no se cobra nada (tolerancia mínima).

```csharp
int     fullHours  = (int)Math.Floor(elapsed.TotalHours);
int     remainMin  = (int)(elapsed.TotalMinutes % 60);

decimal total = fullHours * hourlyRate;
if (remainMin > 0)
    total += fractionRate;
```
Se calculan las horas completas y los minutos restantes por separado. Las horas completas se cobran a tarifa hora, y cualquier fracción (aunque sea 1 minuto) se cobra a tarifa fracción.

---

## 10. Statistics.cs — Estadísticas

Clase que centraliza todos los cálculos de métricas del parqueadero. No tiene lógica de negocio propia; **lee** datos del `ParkingMap` y del `ParkingManager`.

```csharp
public double OccupancyPercent =>
    TotalSpots == 0 ? 0 : Math.Round((double)OccupiedSpots / TotalSpots * 100, 1);
```
Porcentaje de ocupación con un decimal. El operador ternario evita división por cero si el mapa está vacío.

```csharp
public (string plate, string time)? GetLongestStay()
{
    var parked = _manager.GetAllParked();
    if (parked.Count == 0) return null;

    var longest = parked.OrderByDescending(p => DateTime.Now - p.vehicle.EntryTime).First();
    return (longest.vehicle.Plate, longest.vehicle.GetElapsedTime());
}
```
Encuentra el vehículo que lleva más tiempo estacionado ordenando por tiempo transcurrido de mayor a menor y tomando el primero. Devuelve `null` si no hay vehículos.

```csharp
public decimal AverageTransaction =>
    TotalTransactions == 0 ? 0 : TotalCollected / TotalTransactions;
```
Promedio de cobro por transacción. Evita división por cero.

```csharp
public decimal CollectedByType(VehicleType type)
    => _manager.History.Where(t => t.VehicleType == type).Sum(t => t.AmountPaid);
```
Filtra el historial por tipo de vehículo y suma los montos pagados. Permite saber cuánto aportaron las motos, los carros y los camiones por separado.

### Método `GenerateDayClosingReport`

```csharp
public string[] GenerateDayClosingReport()
```
Genera un array de strings con el reporte de cierre del día, que incluye: fecha, hora, total de vehículos por tipo, ingresos por tipo, promedio, espacios libres, y el vehículo con mayor permanencia. Este array se muestra en consola y también se guarda en disco.

---

## 11. PlateValidator.cs — Validador de placas

Valida y formatea placas colombianas según los formatos reales del país.

```csharp
private static readonly Regex _carPattern  =
    new(@"^[A-Z]{3}[-]?[0-9]{3}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
```
Patrón para **carros y camiones**: 3 letras + guión opcional + 3 dígitos. Ejemplos válidos: `ABC123`, `ABC-123`. Se usa `Compiled` para que la expresión regular sea más rápida al ejecutarse repetidamente.

```csharp
private static readonly Regex _motoPattern =
    new(@"^[A-Z]{3}[-]?[0-9]{2}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
```
Patrón para **motos**: 3 letras + guión opcional + 2 dígitos. Ejemplos válidos: `ABC12`, `ABC-12`.

```csharp
public static string Normalize(string plate)
    => plate.Trim().ToUpper().Replace(" ", "").Replace("-", "");
```
Limpia la placa antes de validarla: elimina espacios, guiones y convierte a mayúsculas. `ABC-123` y `abc 123` se normalizan a `ABC123`.

```csharp
public static string Format(string plate)
{
    string n = Normalize(plate);
    if (n.Length == 6) return $"{n[..3]}-{n[3..]}";   // Carro: ABC-123
    if (n.Length == 5) return $"{n[..3]}-{n[3..]}";   // Moto: ABC-12
    return n;
}
```
Aplica el formato "oficial" con guión al mostrar placas al usuario, aunque internamente se almacenen sin guión.

```csharp
public static string GetValidationError(string plate)
```
Devuelve un mensaje de error específico y descriptivo según qué parte de la placa es inválida: si está vacía, si la longitud es incorrecta, si las letras no son letras, o si los números no son dígitos.

---

## 12. FileManager.cs — Gestión de archivos

Maneja la persistencia de datos en disco. Todos los archivos se guardan en una carpeta `data/` junto al ejecutable.

```csharp
private static readonly string _dataDir =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
```
Ruta absoluta a la carpeta de datos, calculada a partir de la ubicación del ejecutable. Esto funciona independientemente de desde dónde se ejecute el programa.

```csharp
private static string TransactionsFile =>
    Path.Combine(_dataDir, $"historial_{DateTime.Now:yyyy-MM-dd}.csv");
```
El archivo CSV del historial incluye la **fecha actual** en su nombre. Esto significa que cada día se genera un archivo diferente automáticamente: `historial_2025-06-15.csv`.

```csharp
private static string DayReportFile =>
    Path.Combine(_dataDir, $"reporte_{DateTime.Now:yyyy-MM-dd}.txt");
```
Análogamente, el reporte de cierre tiene la fecha en su nombre: `reporte_2025-06-15.txt`.

### Método `ExportTransactions`

```csharp
public static (bool success, string path) ExportTransactions(IReadOnlyList<Transaction> transactions)
```
Exporta todas las transacciones del día a CSV. Devuelve una tupla con el resultado (éxito/fallo) y la ruta del archivo o el mensaje de error.

```csharp
using StreamWriter writer = new(TransactionsFile, append: false);
writer.WriteLine(Transaction.CsvHeader());
for (int i = 0; i < transactions.Count; i++)
    writer.WriteLine(transactions[i].ToCsvLine());
```
`append: false` significa que **sobreescribe** el archivo si ya existe, en lugar de agregar. Esto asegura que el CSV exportado sea siempre la versión más reciente y completa.

### Método `AppendTransaction`

```csharp
public static bool AppendTransaction(Transaction transaction)
```
Variante que agrega una sola transacción al CSV de forma incremental (útil si se quisiera guardar en tiempo real). `append: true` significa que agrega al final sin borrar lo anterior. Si el archivo no existe todavía, escribe primero el encabezado.

### Método `SaveDayReport`

```csharp
using StreamWriter writer = new(DayReportFile, append: false);
writer.WriteLine($"REPORTE DE CIERRE — PARQUEADERO");
writer.WriteLine(new string('=', 50));
for (int i = 0; i < reportLines.Length; i++)
    writer.WriteLine(reportLines[i]);
writer.WriteLine(new string('=', 50));
writer.WriteLine($"Generado: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
```
Escribe el reporte añadiendo encabezado decorativo, las líneas del reporte, separador de cierre y marca de tiempo de generación.

---

## 13. ConsoleUI.cs — Interfaz de consola

Librería de utilidades para dibujar texto con colores ANSI en la consola. Todos los métodos son `static`.

### Códigos de color ANSI

```csharp
public const string Reset   = "\u001b[0m";    // Resetea todos los estilos
public const string Bold    = "\u001b[1m";    // Texto en negrilla
public const string Dim     = "\u001b[2m";    // Texto tenue/apagado
```
Cada constante es una **secuencia de escape ANSI**: caracteres especiales que la terminal interpreta como instrucciones de formato en lugar de mostrarlos como texto.

```csharp
public const string Green   = "\u001b[32m";   // Verde (texto)
public const string BgGreen = "\u001b[42m";   // Verde (fondo)
```
Los colores de texto usan códigos 30–37 y 90–97 (brillantes). Los fondos usan 40–47 y 100–107.

### Métodos de impresión

```csharp
public static void Print(string text, string color = "")
    => Console.Write(color + text + Reset);
```
Imprime texto con un color y luego aplica `Reset` para que el siguiente texto no herede el color.

```csharp
public static void DrawBox(string title, string content, string color = "", int width = 60)
```
Dibuja un cuadro con bordes de doble línea Unicode (`╔═╗`, `║`, `╚═╝`), un título centrado, y contenido línea por línea. Si una línea del contenido es más larga que el ancho del cuadro, se trunca.

```csharp
public static void DrawSectionTitle(string title, string color = "")
{
    int pad = (68 - title.Length) / 2;
    string line = new string('═', pad) + $"[ {title} ]" + new string('═', pad);
    PrintLine(line, Bold + color);
}
```
Dibuja un título de sección centrado rodeado de signos `═`. Se centra calculando cuántos caracteres de relleno poner a cada lado.

```csharp
public static void PrintSuccess(string msg) =>
    PrintLine($"  ✔  {msg}", BrightGreen);

public static void PrintError(string msg) =>
    PrintLine($"  ✘  {msg}", BrightRed);

public static void PrintWarning(string msg) =>
    PrintLine($"  ⚠  {msg}", BrightYellow);

public static void PrintInfo(string msg) =>
    PrintLine($"  ℹ  {msg}", BrightCyan);
```
Mensajes estandarizados con íconos Unicode y colores semánticos: verde para éxito, rojo para error, amarillo para advertencia, cian para información.

```csharp
public static void SpinnerAnimation(string message, int durationMs = 1200)
{
    char[] spinner = { '|', '/', '─', '\\' };
    int    frames  = durationMs / 100;

    for (int i = 0; i < frames; i++)
    {
        Console.Write($"\r  {BrightYellow}{spinner[i % 4]}{Reset}  {message}  ");
        Thread.Sleep(100);
    }
}
```
Muestra una animación de "cargando" en consola. Usa `\r` (retorno de carro) para sobreescribir la misma línea en cada frame. Los 4 caracteres del spinner (`|`, `/`, `─`, `\`) crean la ilusión de rotación al alternarse cada 100ms.

### Métodos de tabla

```csharp
public static void PrintTableHeader(params (string label, int width)[] cols)
```
Imprime la cabecera de una tabla con bordes Unicode. Acepta un número variable de columnas, cada una con su etiqueta y ancho en caracteres.

```csharp
public static void PrintTableRow(string color = "", params (string value, int width)[] cols)
```
Imprime una fila de datos de la tabla. Si el valor es más largo que el ancho de la columna, se trunca.

---

## 14. MapRenderer.cs — Renderizador del mapa

Dibuja el mapa del parqueadero en la consola usando colores y caracteres Unicode.

### Método `RenderMap`

```csharp
public static void RenderMap(ParkingMap map)
{
    for (int row = 0; row < ParkingMap.ROWS; row++)
    {
        ConsoleUI.Print($"  {row,2} ", ConsoleUI.Dim + ConsoleUI.White);  // Número de fila

        for (int col = 0; col < ParkingMap.COLS; col++)
        {
            ref ParkingSpot cell = ref map.GetCell(row, col);
            RenderCell(ref cell);
        }
        Console.WriteLine();
    }
    PrintLegend(map);
}
```
Itera sobre toda la cuadrícula fila por fila. Para cada celda llama a `RenderCell`. Usa `ref` para evitar copiar el struct al accederlo.

### Método `RenderCell`

```csharp
case CellType.Entry:
    ConsoleUI.Print($"{ConsoleUI.Bold}{ConsoleUI.BgGreen}{ConsoleUI.Black} EN {ConsoleUI.Reset}");
    break;
case CellType.Exit:
    ConsoleUI.Print($"{ConsoleUI.Bold}{ConsoleUI.BgRed}{ConsoleUI.White} SA {ConsoleUI.Reset}");
    break;
case CellType.Road:
    ConsoleUI.Print($"{ConsoleUI.BrightYellow}░░░{ConsoleUI.Reset}");
    break;
case CellType.Wall:
    ConsoleUI.Print($"{ConsoleUI.Dim}███{ConsoleUI.Reset}");
    break;
case CellType.Spot:
    RenderSpot(ref cell);
    break;
```
Cada tipo de celda tiene su propio aspecto visual. Las paredes son bloques grises, las vías son puntos amarillos, la entrada es verde con texto "EN" y la salida es roja con "SA".

### Método `RenderSpot`

```csharp
if (spot.Status == SpotStatus.Free)
    ConsoleUI.Print($"{ConsoleUI.BgGreen}{ConsoleUI.Black} D {ConsoleUI.Reset}");
else
{
    string icon = spot.OccupiedBy?.GetIcon() ?? " X ";
    ConsoleUI.Print($"{ConsoleUI.BgBrightRed}{ConsoleUI.White} {icon} {ConsoleUI.Reset}");
}
```
Los espacios libres se muestran en verde con "D" (disponible). Los ocupados en rojo con el ícono del vehículo (M, C o T).

### Método `PrintLegend`

Imprime debajo del mapa una leyenda explicando cada color/símbolo, y también muestra las estadísticas de disponibilidad: cuántos espacios están libres, ocupados y el porcentaje de ocupación. El porcentaje cambia de color según el nivel: verde si es bajo, amarillo si supera el 50%, rojo si supera el 80%.

### Método `RenderMiniMap`

```csharp
public static void RenderMiniMap(ParkingMap map)
```
Versión simplificada del mapa, enmarcada en un cuadro pequeño. Se usa en la vista de búsqueda de vehículo para mostrar la ubicación sin ocupar toda la pantalla. Cada celda se representa con un solo carácter (`o` libre, `X` ocupado, `.` vía, `#` pared).

---

## 15. MenuHandler.cs — Menú principal

Controla toda la interacción con el usuario. Gestiona el ciclo de vida de la aplicación y los flujos de cada opción.

### Método `Run`

```csharp
public void Run()
{
    bool running = true;
    while (running)
    {
        ShowMainMenu();
        string option = ConsoleUI.ReadLine("Selecciona una opción");
        switch (option.Trim())
        {
            case "1": ShowMapFull();         break;
            case "2": RegisterEntryFlow();   break;
            case "3": RegisterExitFlow();    break;
            case "4": ShowVehicleListFlow(); break;
            case "5": SearchVehicleFlow();   break;
            case "6": ShowStatisticsFlow();  break;
            case "7": ShowHistoryFlow();     break;
            case "8": running = ExitFlow();  break;
            default: /* muestra advertencia */ break;
        }
    }
}
```
Ciclo principal: muestra el menú, lee la opción, ejecuta el flujo correspondiente, y repite. El flag `running` es la única forma de salir del bucle: `ExitFlow` devuelve `false` si el usuario confirma la salida.

### Método `ShowMainMenu`

Limpia la pantalla, muestra el banner del sistema, y muestra en tiempo real el número de espacios libres, ocupados, el porcentaje de ocupación y el total recaudado. Luego lista las 8 opciones disponibles.

### Flujo de entrada (`RegisterEntryFlow`)

1. Verifica que haya espacios disponibles; si no, muestra error y regresa.
2. Pide la placa en un bucle hasta que sea válida según `PlateValidator`.
3. Pide el tipo de vehículo (1=Moto, 2=Carro, 3=Camión) en un bucle hasta obtener una opción válida.
4. Muestra animación de entrada.
5. Llama a `manager.RegisterEntry` y muestra el ticket de entrada con todos los detalles.

### Flujo de salida (`RegisterExitFlow`)

1. Verifica que haya vehículos en el parqueadero.
2. Muestra la lista resumida de vehículos actuales.
3. Pide la placa del vehículo a retirar.
4. Llama a `manager.RegisterExit`.
5. Si tiene éxito, muestra el recibo de cobro con duración y monto. Pregunta si se confirma el cobro. Si el usuario dice "S", la salida se registra definitivamente.
6. Si el usuario dice "N", se muestra un mensaje de cancelación (el vehículo permanece en el parqueadero).

### Flujo de estadísticas (`ShowStatisticsFlow`)

Muestra una barra de ocupación visual (█ para ocupado, ░ para libre), seguida de estadísticas detalladas: espacios por estado, vehículos por tipo, métricas financieras (total, promedio, por tipo), y el vehículo con mayor tiempo de permanencia.

### Flujo de historial (`ShowHistoryFlow`)

Muestra todas las transacciones completadas en formato de tabla con columnas: #, Placa, Tipo, Entrada, Salida, Duración, Valor. Al final muestra el total recaudado.

### Flujo de cierre (`ExitFlow`)

1. Genera el reporte de cierre con `stats.GenerateDayClosingReport()`.
2. Lo muestra en pantalla.
3. Exporta el historial CSV y el reporte de texto si hay transacciones.
4. Pide confirmación al usuario. Si confirma, devuelve `false` (termina el bucle `Run`). Si cancela, devuelve `true` (continúa el sistema).

---

## 16. Flujo completo de la aplicación

```
main()
 └── menu.Run()
       └── [bucle principal]
             ├── ShowMainMenu()              ← Muestra estado actual + opciones
             ├── [1] ShowMapFull()           ← Dibuja mapa completo
             ├── [2] RegisterEntryFlow()
             │     ├── Validar placa
             │     ├── Seleccionar tipo
             │     ├── manager.RegisterEntry()
             │     │     ├── FindFirstFreeSpot()
             │     │     ├── new Vehicle(...)
             │     │     └── spot.AssignVehicle()
             │     └── Mostrar ticket
             ├── [3] RegisterExitFlow()
             │     ├── Mostrar lista
             │     ├── manager.RegisterExit()
             │     │     ├── FindSpotByPlate()
             │     │     ├── CalculateFee()
             │     │     ├── spot.FreeSpot()
             │     │     └── new Transaction(...)
             │     └── Mostrar recibo
             ├── [4] ShowVehicleListFlow()   ← Tabla de vehículos actuales
             ├── [5] SearchVehicleFlow()     ← Buscar por placa + mini mapa
             ├── [6] ShowStatisticsFlow()    ← Métricas y financiero
             ├── [7] ShowHistoryFlow()       ← Historial de cobros
             └── [8] ExitFlow()
                   ├── GenerateDayClosingReport()
                   ├── FileManager.ExportTransactions()  → historial_YYYY-MM-DD.csv
                   ├── FileManager.SaveDayReport()       → reporte_YYYY-MM-DD.txt
                   └── running = false  →  [fin del programa]
```

---

## 17. Tarifas del sistema

| Tipo de vehículo | Tarifa por hora | Tarifa fracción |
|-----------------|----------------|----------------|
| Moto | $1,500 COP | $750 COP |
| Carro | $3,000 COP | $1,500 COP |
| Camión | $5,000 COP | $2,500 COP |

**Regla de cobro:** Se cobran las horas completas a tarifa hora. Cualquier fracción de hora adicional (aunque sea 1 minuto) se cobra a tarifa fracción. Si el vehículo estuvo menos de 1 minuto, no se cobra nada.

**Ejemplo:** Un carro que estuvo 2 horas y 35 minutos paga:
- 2 horas × $3,000 = $6,000
- 1 fracción × $1,500 = $1,500
- **Total: $7,500 COP**

---

## 18. Cómo ejecutar el proyecto

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Comandos

```bash
# Clonar el repositorio
git clone <url-del-repo>
cd parqueo

# Compilar
dotnet build

# Ejecutar
dotnet run

# Publicar como ejecutable independiente
dotnet publish -c Release -r win-x64 --self-contained   # Windows
dotnet publish -c Release -r linux-x64 --self-contained  # Linux
dotnet publish -c Release -r osx-x64 --self-contained    # macOS
```

### Archivos generados al cerrar

Al salir del sistema, se crean automáticamente en `./data/`:

| Archivo | Contenido |
|---------|-----------|
| `historial_YYYY-MM-DD.csv` | Todas las transacciones del día en formato CSV |
| `reporte_YYYY-MM-DD.txt` | Reporte de cierre con métricas y totales |
