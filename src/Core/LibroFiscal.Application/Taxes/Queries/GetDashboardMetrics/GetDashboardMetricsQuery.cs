using System;
using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Taxes.Queries.GetDashboardMetrics;

public sealed record GetDashboardMetricsQuery(
    Guid CompanyId,
    int Year,
    int Month) : IQuery<DashboardMetricsDto>;
