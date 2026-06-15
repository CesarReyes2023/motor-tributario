# Historial de Fases y Resúmenes (LibroFiscal)

Este documento consolida el registro histórico de las fases de desarrollo y los resúmenes técnicos de cada módulo implementado en la aplicación **LibroFiscal**, garantizando la trazabilidad de la arquitectura y las decisiones de diseño.

---

## Fase 15 (Paso 1): Libros de Ventas (IVA)

En este paso, dotamos a nuestra aplicación de la capacidad fiscal de generar, separar y procesar de forma automatizada los **Libros de Ventas a Contribuyentes** y los **Libros de Ventas a Consumidor Final**, los cuales son un requisito mandatorio ante el Ministerio de Hacienda para cualquier empresa.

### 1. Motor de Extracción CQRS
Se crearon en la Capa `Application` dos flujos (Queries) que leen de los DTE (Documentos Tributarios Electrónicos) y los transforman a los formatos legales para los anexos de IVA:
* **Libro de Ventas (Contribuyentes)**: Extrae DTEs tipo `03` (Crédito Fiscal), mapeando de forma detallada al receptor (NIT/NRC) e identificando las operaciones exentas, gravadas locales y los montos de Impuestos generados (Débito Fiscal).
* **Libro de Ventas (Consumidor Final)**: Extrae DTEs tipo `01` (Facturas) y `11` (Facturas de Exportación), agrupándolos por día para crear los folios legales desde el documento Inicial hasta el documento Final, desglosando la venta neta y reteniendo el detalle de Exportaciones de forma transparente.

### 2. ViewModel Inteligente (UI Dinámica)
Aprovechando `VatBooksViewModel`, convertimos la pantalla en un *Hub Fiscal* donde la UI es reactiva.
Se agregó un combo `SelectedBookType` con tres opciones. Al cambiar de tipo de Libro, el motor dispara un comando al `IMediator` respectivo, vacía la memoria previa y carga instantáneamente las operaciones exactas del libro solicitado. 

### 3. Vista XAML de Alto Nivel
Con la implementación en `VatBooksView.xaml`, logramos que la pantalla responda mediante Triggers (Disparadores de Datos) que cambian radicalmente la UI:
* **DataGrids Alternables**: Un simple `ListView` se encoge, oculta o muestra y transforma sus columnas (Ej. En Consumidor final muestra *Exportaciones* y Folios Del/Al, mientras en Contribuyente muestra *NIT* y *NRC*).
* **Tarjetas Dinámicas**: Los "Cards" que muestran la suma de totales adaptaron su lenguaje. Pasaron de decir "Total Crédito Fiscal" a "Impuestos (Crédito / Débito)" de tal manera que aplican para Ventas y Compras al mismo tiempo de manera "Senior" y profesional sin duplicar código.

---

## Fase 16 (Paso 2): Exportación Oficial CSV (Ministerio de Hacienda)

La aplicación ahora permite que la información consolidada de los libros (Compras, Ventas Contribuyentes, Ventas Consumidor Final) se exporte con un solo clic a un archivo CSV plano que puede ser consumido por el Ministerio de Hacienda (Anexo F-07).

### 1. Inyección de Servicio (CsvExportService)
Se creó el archivo `CsvExportService.cs` y se inyectó en el contenedor de dependencias (`App.xaml.cs`) como un Singleton para no consumir memoria innecesaria. Este servicio cuenta con 3 métodos:
- `ExportPurchasesAsync`
- `ExportSalesTaxpayerAsync`
- `ExportSalesConsumerAsync`

Los métodos utilizan `StringBuilder` para recorrer miles de líneas al instante sin trabar la computadora, y separan con punto y coma (`;`) los datos, asegurando usar `CultureInfo.InvariantCulture` para que los separadores decimales (puntos en lugar de comas) no rompan el estándar internacional independientemente de cómo el usuario tenga configurado su Windows.

### 2. ViewModel Inteligente
Se modificó `VatBooksViewModel.cs` inyectando el servicio. Se programó el comando `ExportToCsvAsync` que invoca un diálogo nativo de Windows (`SaveFileDialog`), sugiriendo ya el nombre del archivo (Ej. *"Libro de Compras - JUNIO 2026.csv"*). Si el usuario selecciona una ruta, el programa lee qué libro tiene seleccionado en pantalla y ejecuta la exportación.

### 3. Vista XAML de Alto Nivel
A la par del botón de **"Generar Reporte"**, agregamos un moderno y sutil botón delineado de **"Exportar CSV"**. Es visible, amigable al tacto (`Cursor="Hand"`) y reacciona correctamente a los estados de carga.

---

## Fase 17 (Paso 3): Dashboard Analítico

Se ha transformado la pantalla principal de bienvenida de "Dashboard" de un simple texto vacío a un **Centro de Mando Fiscal** dinámico que lee el estado del mes en tiempo real.

### 1. Extractor de Métricas (CQRS)
Se construyó la consulta `GetDashboardMetricsQuery` la cual es muy agresiva en rendimiento:
- Ejecuta dos llamadas a la base de datos (una a `DteDocuments` y otra a `Purchases`).
- Calcula sumas consolidadas, separando entre la venta bruta, la venta exenta y su respectivo impuesto.
- Realiza la fórmula del "Balance IVA" (Débito menos Crédito Fiscal) para mostrarle a la empresa, en tiempo real, si este mes le toca pagar impuesto o tiene saldo a favor.
- Todo lo consolida en el `DashboardMetricsDto`.

### 2. ViewModel Inteligente
Se programó el `DashboardViewModel` para que, inmediatamente al cargar la vista, invoque esta nueva consulta sin que el usuario presione ningún botón. Todas sus propiedades son reactivas (`[ObservableProperty]`), por lo que las gráficas y montos se animan o actualizan automáticamente en XAML en el momento en que se procesan. Además localizamos correctamente la fecha (Ej. "RESUMEN FISCAL DE JUNIO 2026").

### 3. Diseño Visual "Full Senior"
Siguiendo las instrucciones de mantener un entorno profesional ("Senior") e impactante visualmente, el `DashboardView.xaml` fue completamente reescrito para utilizar la paleta de colores del diseño:
- **Top Row (Indicadores Clave - KPIs):** 4 tarjetas minimalistas tipo "Glass" que muestran de forma contundente: Ventas Netas, Compras Netas, Balance de IVA, y una tarjeta con alerta naranja de "DTEs Pendientes" (borradores o rechazados).
- **Gráfico Comparativo (100% Nativo):** En lugar de inflar la aplicación instalando paquetes externos de gráficos, construimos una impresionante barra dual tipo "Progress" usando `ControlTemplate` nativo de WPF. Esta gráfica se pinta con los colores acentuados (Verde/Azul) usando la propiedad proporcional calculada desde el backend (`SalesPercentage`), permitiendo ver en una sola barra larga quién domina el mes: las ventas o las compras.

---

## Fase 18 (Paso 4): Módulo de Configuración de Empresa

Se ha construido el **Módulo de Configuración de Empresa (Paso 4)**, el cual permite al usuario (contribuyente) gestionar su información legal y de contacto desde la aplicación de escritorio, conectada directamente a la base de datos local de PostgreSQL para garantizar la persistencia.

### 1. Reutilización Inteligente de Entidades (Dominio)
En lugar de crear una tabla nueva y saturar la base de datos, analizamos la capa de Dominio (`LibroFiscal.Domain.Companies.Entities`) y descubrimos que **ya existía** un modelo `Company` extremadamente robusto con validaciones de ValueObjects como `Nit`, `Nrc` y `DireccionFiscal`.
- Se reutilizó el `DbSet<Company>` en `LibroFiscalDbContext`.
- **Beneficio:** Evitamos crear migraciones innecesarias, mantuvimos el esquema limpio y aprovechamos las validaciones estrictas que ya teníamos programadas.

### 2. Flujo CQRS de Datos (Aplicación)
Se añadieron dos comandos mediante el patrón Mediator para interactuar con la base de datos:
- `GetCompanyProfileQuery`: Consulta que extrae el primer perfil activo de la empresa (pensando en arquitecturas MVP Single-Tenant) y mapea los datos a un `CompanyProfileDto`.
- `UpdateCompanyProfileCommand`: Comando que recibe los datos desde la interfaz, los valida con las reglas de negocio (ej. formato de NIT y NRC) y si la empresa no existía, la inserta. Si ya existía, desactiva la versión vieja e inserta la nueva versión, dejando un rastro de auditoría.

### 3. Interfaz Visual "Senior" (Presentación XAML)
Se rediseñó completamente la pantalla `CompanyView.xaml`:
- **Layout Profesional de 2 Columnas:** Separación visual clara con una línea vertical divisoria. A la izquierda la "Información Legal" (Razón Social, NIT, NRC, Actividad) y a la derecha "Contacto y Dirección" (Teléfono, Correo, Departamento, Municipio).
- **Inyección y Carga Automática:** Usando el evento `Loaded` de WPF, la pantalla invoca a `LoadCompanyAsync()` de manera automática al abrirse. Si el usuario ya guardó sus datos previamente, los verá cargados inmediatamente.
- **Formulario Estilizado:** Todos los `TextBox` tienen anchos y márgenes precisos (`Padding="12,10"` y `Height="42"`) para sentirse holgados, modernos y alineados con las mejores prácticas UI/UX. Los colores se adaptan perfectamente a los Themes dinámicos (Light/Dark).
