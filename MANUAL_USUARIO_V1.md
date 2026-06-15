# Manual de Usuario - LibroFiscal V1.0 (Portable)

¡Bienvenido a **LibroFiscal**! Este software ha sido diseñado para automatizar la contabilidad, lectura de Documentos Tributarios Electrónicos (DTE) y generación de los anexos de IVA exigidos por el Ministerio de Hacienda de El Salvador.

Esta es la versión **Portable (V1.0)**, diseñada para ser rápida, segura y no requerir instalaciones complejas.

---

## 1. ¿Cómo Instalar y Ejecutar?

Al ser una versión Portable, **no necesitas instalar nada**. Tampoco necesitas instalar bases de datos (SQL, Postgres), ya que el sistema viene con un motor de base de datos incrustado (SQLite).

1. Extrae (descomprime) el archivo `LibroFiscal_V1_Portable.zip` en una carpeta de tu preferencia (ej. en tus Documentos o en el Escritorio).
2. Entra a la carpeta descomprimida y haz doble clic en el archivo `LibroFiscal.Desktop.exe`.
3. ¡Listo! La aplicación abrirá inmediatamente.
   * *Nota: La primera vez que abras la aplicación, notarás que se crea automáticamente un archivo llamado `librofiscal.db` en la misma carpeta. **NO borres este archivo**, ya que ahí se guarda toda tu información contable de forma segura.*

---

## 2. ¿Qué puede hacer esta versión? (Módulos Disponibles)

El sistema actualmente cuenta con **4 Módulos Principales** completamente funcionales para administrar la contabilidad de tu empresa:

### 🏢 Módulo 1: Configuración de Empresa
Lo primero que debes hacer al abrir el sistema. Ve al menú lateral y haz clic en **Empresas**.
- **¿Qué hace?** Permite registrar el perfil fiscal oficial de tu negocio (Razón Social, NIT, NRC, Giro/Actividad Económica, Teléfono y Dirección).
- **Validaciones Inteligentes:** El sistema no te dejará guardar datos basura. Validará automáticamente que el NIT y el NRC tengan el guion y el formato correcto exigido por Hacienda.

### 📊 Módulo 2: Dashboard Analítico (Resumen Fiscal)
Al iniciar la aplicación, serás recibido por el Centro de Mando.
- **¿Qué hace?** Lee toda tu base de datos en tiempo real y calcula tu **Balance de IVA**. 
- **Tarjetas Dinámicas:** Te muestra el total en dólares de tus Compras Netas, Ventas Netas y si este mes te toca **Pagar Impuestos** (Débito Fiscal) o si tienes **Saldo a Favor** (Crédito Fiscal).
- **Gráfica Visual:** Incluye una barra de progreso nativa que te muestra visualmente qué domina en el mes: tus ventas o tus compras.

### 📚 Módulo 3: Motor de Libros de IVA
Ve al menú lateral y haz clic en **Libros de IVA**. Este es el motor principal de la aplicación.
- **¿Qué hace?** Procesa las facturas y comprobantes, y los clasifica legalmente.
- En la parte superior, tienes un menú desplegable para alternar entre:
  1. **Libro de Compras:** Te desglosa las compras exentas, gravadas e impuestos retenidos.
  2. **Libro de Ventas (Contribuyentes):** Agrupa todos los Comprobantes de Crédito Fiscal (DTE 03) detallando el NIT y NRC de tus clientes.
  3. **Libro de Ventas (Consumidor Final):** Agrupa inteligentemente las Facturas (DTE 01) y Facturas de Exportación (DTE 11) **por día**, creando el Folio Legal (Del documento No. X al documento No. Y) exactamente como lo pide Hacienda.

### 📥 Módulo 4: Exportación Oficial a CSV (Anexos F-07)
Dentro de la pantalla de *Libros de IVA*, encontrarás un botón de **"Exportar CSV"**.
- **¿Qué hace?** En lugar de que pases horas digitando facturas en Excel, este botón toma el libro que estés visualizando en pantalla y lo exporta a un archivo plano `.CSV` configurado con punto y coma (`;`).
- **Listo para subir:** Este archivo cumple con la normativa internacional y los lineamientos informáticos del Ministerio de Hacienda, por lo que puedes cargarlo directamente en el portal web (Anexo F-07) para declarar tus impuestos en segundos.

---

## 3. Respaldo y Seguridad

Debido a que esta es una versión Portable, **tu información te pertenece al 100%** y está físicamente en tu computadora, no en la nube.
Si deseas hacer una copia de seguridad o enviar tu contabilidad a tu contador, simplemente haz una copia del archivo `librofiscal.db` y guárdala en un lugar seguro.

*Software desarrollado bajo Arquitectura "Senior" Clean Architecture y Domain-Driven Design (DDD).*
