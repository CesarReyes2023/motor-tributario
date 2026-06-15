# LibroFiscal — Plataforma Tributaria Enterprise para El Salvador

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License](https://img.shields.io/badge/License-Proprietary-red)]()

## 🏛️ Visión

Plataforma tributaria enterprise de nueva generación para El Salvador, comparable arquitectónicamente con SAP Business One, Odoo Enterprise y QuickBooks.

**Capacidades principales:**
- 📄 **DTE** — Facturación electrónica completa (Facturas, Créditos Fiscales, Notas C/D, Retenciones, Exportaciones)
- 🔐 **Firma Electrónica** — JWS (JSON Web Signature) con certificados de Hacienda
- 📚 **Libros IVA** — Compras, Ventas Contribuyentes, Ventas Consumidor Final
- 🏛️ **Integración Hacienda** — API MH con retry, circuit breaker, contingencia
- 🔥 **Motor Fiscal** — Reglas tributarias versionables y enchufables
- 🏢 **Multi-empresa** — Aislamiento por CompanyId desde día uno
- 🔍 **Auditoría** — Trail inmutable, trazabilidad completa

## 🏗️ Arquitectura

```
Clean Architecture + Hexagonal + DDD ligero + CQRS parcial + Event-Driven
```

**Evolución planificada:**
1. ✅ Desktop WPF (actual)
2. 🔜 ASP.NET Core API
3. 🔜 Web SaaS multi-tenant
4. 🔜 Cloud-ready + Workers distribuidos

## 📁 Estructura

```
src/
├── Core/           → Domain, Application, SharedKernel, Contracts
├── Infrastructure/ → Persistence (EF Core), Observability (Serilog)
├── Modules/        → Bounded contexts (DTE, FiscalEngine, Books, etc.)
├── Clients/        → WPF Desktop
├── Workers/        → Background processing
└── Gateway/        → API (future)

tests/
├── Architecture/   → NetArchTest dependency rules
├── Unit/           → Domain + Application tests
├── Integration/    → Database + API tests
└── Domain/         → Fiscal scenario tests
```

## 🚀 Quick Start

```bash
# Prerequisitos
# .NET 8 SDK, SQL Server (LocalDB o instancia)

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar tests de arquitectura
dotnet test tests/LibroFiscal.Tests.Architecture/
```

## 🧪 Testing

| Tipo | Proyecto | Propósito |
|------|----------|-----------|
| Arquitectura | `Tests.Architecture` | Reglas de dependencia (Clean Architecture) |
| Unitario | `Tests.Unit` | Lógica de dominio, value objects, state machine |
| Integración | `Tests.Integration` | Base de datos, API clients |
| Dominio | `Tests.Domain` | Escenarios fiscales El Salvador |

## 📋 Stack Tecnológico

| Categoría | Tecnología |
|-----------|-----------|
| Runtime | .NET 8 LTS |
| ORM | EF Core 8 |
| CQRS | MediatR 12 |
| Validación | FluentValidation 11 |
| Logging | Serilog 4 |
| PDF | QuestPDF |
| Excel | ClosedXML |
| UI Desktop | WPF + MaterialDesign |
| MVVM | CommunityToolkit.Mvvm |
| Testing | xUnit + FluentAssertions + NSubstitute |
| Arquitectura | NetArchTest |

## 📐 Principios de Diseño

1. **SOLID** — Single Responsibility, Open/Closed, Liskov, Interface Segregation, Dependency Inversion
2. **Clean Architecture** — Dependencias apuntan hacia adentro (Domain es el centro)
3. **DDD** — Bounded Contexts, Aggregates, Value Objects, Domain Events
4. **Result Pattern** — Sin excepciones para lógica de negocio
5. **Strongly-Typed IDs** — CompanyId ≠ DteId (seguridad en compilación)
6. **Specification Pattern** — Reglas fiscales composables
7. **Event-Driven** — Domain events para comunicación entre módulos
