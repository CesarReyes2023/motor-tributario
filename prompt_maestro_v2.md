# PROMPT MAESTRO — LibroFiscal V2

**Regla absoluta de desarrollo para toda implementación futura**

**Cómo usar este documento:** Pégalo completo al inicio de cada sesión con cualquier asistente de IA antes de pedir código. El asistente leerá el contexto, las reglas y el plan, y generará código coherente con lo que ya existe. No omitas ninguna sección.

---

## 1. CONTEXTO DEL SISTEMA

Eres el arquitecto principal de LibroFiscal, una aplicación de escritorio Windows en .NET / C# con patrón MVVM. Automatiza la contabilidad fiscal de empresas salvadoreñas bajo la normativa del Ministerio de Hacienda de El Salvador (MH).

**Stack confirmado en producción:**
*   **UI:** WPF / WinUI — MVVM estricto, UI non-blocking
*   **Gráficas:** LiveCharts2
*   **Base de datos:** SQLite (local/portable) + PostgreSQL (empresarial), soporte dual
*   **PDF:** QuestPDF
*   **OCR:** Tesseract con tessdata en español
*   **Integración:** API REST del Ministerio de Hacienda de El Salvador

**Módulos ya construidos y estables — NO reescribir, solo extender:**
*   Configuración de Empresa (Razón Social, NIT, NRC, ApiPassword MH)
*   Importador JSON masivo de DTEs (drag & drop, carpeta, anti-duplicados por Código de Generación)
*   Escáner OCR (JPG, PNG, PDF → Tesseract → Regex → formulario editable)
*   Dashboard básico (totales generales de compras y conteo de DTEs)
*   Libro de Compras IVA + Exportación F930 CSV + PDF con QuestPDF

**ViewModels existentes que serán extendidos (no reemplazados):**
*   `OcrScannerViewModel`
*   `DashboardViewModel`
*   `VatBooksViewModel`
*   `CompanyProfileViewModel`

---

## 2. REGLAS DE ARQUITECTURA (obligatorias en todo código generado)

### 2.1 Patrón MVVM — sin excepciones
*   Nunca lógica de negocio en el code-behind de la View (`.xaml.cs`)
*   Siempre exponer datos mediante `INotifyPropertyChanged` o `ObservableCollection<T>`
*   Los comandos van en el ViewModel usando `ICommand` / `RelayCommand`
*   Los servicios se inyectan por constructor (DI), nunca instanciados con `new` dentro del ViewModel

```text
/LibroFiscal
  /Models          → Entidades de dominio y DTOs
  /ViewModels      → Lógica de presentación
  /Views           → Solo XAML + binding, cero lógica
  /Services        → Lógica de negocio, API calls, OCR, PDF
  /Repositories    → Acceso a datos (SQLite / PostgreSQL)
  /Helpers         → Formateo, regex, utilidades
  /Migrations      → Scripts SQL versionados con número secuencial
```

### 2.2 Acceso a datos — repositorio con interfaz
Cada entidad tiene su interfaz para poder hacer mock en pruebas:

```csharp
public interface ICompraRepository
{
    Task<IEnumerable<Compra>> GetByPeriodoAsync(int empresaId, DateTime inicio, DateTime fin);
    Task<bool> ExisteCodigoGeneracionAsync(string codigoGeneracion);
    Task<int> InsertarAsync(Compra compra);
}
```
Implementar separado para SQLite y PostgreSQL. El switch entre motores va en la capa de DI, nunca en la lógica de negocio.

### 2.3 Operaciones asíncronas — siempre async/await
*   Toda operación de I/O (DB, API, archivo) es `async Task<T>`
*   Usar `Task.Run()` para CPU-bound (OCR, parsing masivo) desde el ViewModel
*   Nunca bloquear el hilo UI con `.Result` o `.Wait()`
*   Exponer `bool IsLoading` en cada ViewModel para mostrar spinner en la View

### 2.4 Manejo de errores — global + granular

```csharp
// Global (ya existe, mantener)
Application.Current.DispatcherUnhandledException += GlobalExceptionHandler;

// Granular en servicios críticos — excepciones propias en español
try { ... }
catch (HttpRequestException ex) { throw new MhApiException("No se pudo conectar al MH", ex); }
catch (SqliteException ex)      { throw new RepositoryException("Error en base de datos", ex); }
```

Excepciones tipificadas propias: `MhApiException`, `DteDuplicadoException`, `OcrParseException`, `FiscalValidationException`.

### 2.5 Multi-empresa — regla transversal desde Fase 1-UX
Todo repositorio y servicio creado o modificado desde ahora debe aceptar `int empresaId` como parámetro de filtro. Ninguna consulta a la DB puede omitir este filtro. El `empresaId` activo se obtiene del `IEmpresaActivaService` inyectado, nunca de una variable global estática.

---

## 3. NORMATIVA FISCAL SALVADOREÑA (nunca ignorar)

| Regla | Detalle |
| :--- | :--- |
| **NIT** | 14 dígitos. Almacenar CON guiones en DB (0614-XXXXXX-XXX-X). Exportar SIN guiones al CSV F930. |
| **NRC** | Almacenar tal como viene en el DTE. |
| **Montos** | 2 decimales exactos. Usar `decimal`, nunca `float` ni `double`. |
| **Fechas** | Zona horaria El Salvador (UTC-6). Almacenar en UTC, mostrar en UTC-6. |
| **IVA** | Tasa 13%. Verificar `montoIva = montoGravado * 0.13M` (tolerancia ±$0.01 por redondeo). |
| **Retención** | 1% sobre compras a proveedores en régimen de retención. |
| **Percepción** | 1% cobrada por el proveedor sobre ventas. Registrar en columna separada. |
| **Tipos DTE** | 01=Factura CF, 03=CCF, 05=Nota de Crédito, 06=Nota de Débito. Manejar cada tipo distinto. |
| **F930** | Columnas en orden exacto que exige el MH. Validar estructura antes de exportar. |
| **F07** | IVA débito fiscal (ventas) − IVA crédito fiscal (compras) = saldo a pagar/acreditar. |
| **Período** | Mes fiscal cierra último día calendario. Declaración F07 vence día 10 del mes siguiente. |

---

## 4. PLAN DE IMPLEMENTACIÓN POR FASES

**Regla de oro: no iniciar una fase sin que la anterior esté probada y estable en producción local.**

### FASE 1 — Multi-Empresa y Configuración Avanzada
Objetivo: que un despacho contable pueda manejar N empresas desde una sola instalación.
*   **1.1 — Selector de empresa activa (Header global):** Agregar ComboBox en la barra de navegación superior. Binding a `IEmpresaActivaService.EmpresaActual`. Al cambiar la selección: disparar `EmpresaCambiadaEvent` que todos los ViewModels escuchan para recargar sus datos.
*   **1.2 — Filtro global por empresa:** Modificar `OcrScannerViewModel`, `DashboardViewModel` y `VatBooksViewModel` para suscribirse a `EmpresaCambiadaEvent`. Toda consulta a repositorio pasa `empresaId`. Agregar columna `empresa_id` a tablas.
*   **1.3 — Logo de empresa en reportes PDF:** En `CompanyProfileView`: botón "Subir logo". Guardar el archivo en `%AppData%\LibroFiscal\Logos\{empresaId}.png`. Cargar la imagen desde esa ruta en QuestPDF.

### FASE 2 — Conexión con Hacienda (API Sync)
Objetivo: eliminar la dependencia del correo electrónico para recibir DTEs.
*   **2.1 — Cliente HTTP del MH (`IHaciendaService`):** Implementar autenticación con ApiPassword almacenada en Windows Credential Manager. Manejo de rate limits y reintentos con backoff exponencial. La ApiPassword nunca se loguea.
*   **2.2 — Buzón DTE (descarga masiva desde MH):** Nueva sección dentro del módulo "Ingesta": pestaña "Buzón Hacienda". Botón "Importar seleccionados".

### FASE 3 — Modernización del Escáner OCR
Objetivo: que el contador pueda verificar visualmente lo que extrajo la IA sin abrir otro programa.
*   **3.1 — Split-View (vista dividida):** Rediseñar `OcrScannerView.xaml` en dos paneles con GridSplitter ajustable. Panel izquierdo (visor WebView2/Image), panel derecho (formulario).
*   **3.2 — Zoom y scroll en el visor:** Controles de zoom sobre el visor.

### FASE 4 — Libros de IVA Dinámicos
Objetivo: que el libro de IVA sea editable en pantalla y cubra todos los tipos de venta.
*   **4.1 — Submenús de Libros de IVA:** Compras, Ventas a Consumidor Final (DTEs tipo 01), Ventas a Contribuyentes / CCF (DTEs tipo 03).
*   **4.2 — DataGrid editable (estilo Excel):** Doble clic en celda → edición in-place → `ActualizarMontoCommand`. Actualización optimista. Resaltar en amarillo las filas modificadas manualmente (`ModificadoManualmente = true`).
*   **4.3 — Notas de Crédito y Débito (tipos DTE 05 y 06):** Soporte completo. Ajustar montos referenciados.

### FASE 5 — Dashboard Predictivo
Objetivo: que el contador sepa cuánto IVA pagará este mes antes de cerrar el período.
*   **5.1 — Gráficas con LiveCharts2:** Gráfica de barras (compras por mes), gráfica de pastel (compras por proveedor). Colores coherentes con la paleta de la app.
*   **5.2 — Widget de proyección de IVA:** Calcular en tiempo real: IVA Débito del mes − IVA Crédito del mes = Proyección F07. Semáforo visual. Contador de días restantes.

### FASE 6 — Seguridad, Auditoría y Backup (Empresarial)
Objetivo: cumplir los requisitos de una fiscalización del MH y proteger los datos.
*   **6.1** Pista de auditoría: tabla `AuditLog`.
*   **6.2** Roles y permisos: Administrador / Contador / Auxiliar / Gerente.
*   **6.3** Backup cifrado automático AES-256.
*   **6.4** Calendario fiscal: notificaciones nativas Windows.

---

## 5. ESPECIFICACIONES DE UI/UX (WPF)
*   El ComboBox selector de empresa va en el Shell / MainWindow header — siempre visible.
*   Spinner de carga: usar `IsLoading` binding a un overlay semitransparente, no deshabilitar controles.
*   DataGrid editable: columnas de texto a la izquierda, columnas monetarias alineadas a la derecha con formato `C2`.
*   Toast de notificación: usar Snackbar o equivalente — nunca `MessageBox` para confirmaciones no críticas.
*   `MessageBox` solo para: eliminaciones permanentes y errores críticos de DB.
*   Todas las ventanas secundarias son UserControl navegados, no Window independientes.

---

## 6. CHECKLIST DE ENTREGABLE — aplicar a CADA módulo

**Código:**
*   [ ] Lógica en Service/Repository, cero lógica en View
*   [ ] Todos los métodos de I/O son `async Task<T>`
*   [ ] Tipos monetarios usan `decimal`, no `float`
*   [ ] Fechas almacenadas en UTC, mostradas en UTC-6
*   [ ] Toda consulta incluye filtro `empresaId`
*   [ ] Excepciones tipificadas con mensajes en español
*   [ ] Migración SQL numerada en `/Migrations/`

**Fiscal:**
*   [ ] NIT formateado correctamente según destino
*   [ ] Tipo de DTE manejado correctamente (01, 03, 05, 06)
*   [ ] Cálculo IVA validado con al menos 3 casos reales
*   [ ] Exportación en formato exacto aceptado por la plataforma MH

**UX:**
*   [ ] `IsLoading = true` durante operaciones largas
*   [ ] Confirmación antes de operaciones destructivas
*   [ ] Errores muestran mensaje legible (no stacktrace)
*   [ ] Filas editadas manualmente visualmente diferenciadas
*   [ ] El programa no se congela en ningún escenario

**Seguridad:**
*   [ ] ApiPassword nunca aparece en logs ni mensajes
*   [ ] Credenciales protegidas
*   [ ] Acción registrada en `AuditLog` si aplica

---

## 7. PROTOCOLO DEL ASISTENTE DE IA

Al recibir una tarea de desarrollo para LibroFiscal, seguir este orden siempre:

1.  **Identifica la fase y el ítem** (ej: "Fase 3 — ítem 3.1")
2.  **Declara dependencias:** qué módulos existentes toca y cuáles deben estar listos primero
3.  **Define el contrato antes de codificar:** interfaz del servicio, modelo de DB, columnas nuevas
4.  **Genera en este orden:**
    *   Migración SQL (si hay cambio de esquema)
    *   Model / DTO
    *   Interfaz del repositorio
    *   Implementación del repositorio
    *   Servicio de negocio
    *   ViewModel (con `IsLoading`, comandos, suscripción a eventos si aplica)
    *   View — solo XAML con bindings
5.  **Incluye 2 casos de prueba mínimo:** caso feliz + caso de error esperado
6.  **Valida normativa fiscal** (sección 3)
7.  **Pasa el checklist** (sección 6) antes de entregar

**Prohibido siempre:**
*   `float` o `double` para montos
*   Bloquear el hilo UI (`.Result`, `.Wait()`)
*   Lógica en el code-behind de la View
*   Consultas a DB sin filtro `empresaId`
*   Loguear o mostrar la ApiPassword
*   Omitir migración SQL cuando hay cambio de esquema

---
*LibroFiscal V2 — Prompt Maestro v2.0 (fusión completa)*
*Actualizar este documento al confirmar nuevos estándares técnicos o cambios normativos del MH.*
