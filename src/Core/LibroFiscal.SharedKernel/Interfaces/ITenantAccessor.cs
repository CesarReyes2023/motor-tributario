namespace LibroFiscal.SharedKernel.Interfaces;

/// <summary>
/// Provides access to the current company (tenant) context.
/// In the WPF desktop client, this is set when the user selects/switches a company.
/// In the future API, this will be resolved from JWT claims or request headers.
/// 
/// Used by:
/// - EF Core global query filters (automatic WHERE CompanyId = ...)
/// - Application services (to scope operations to the current company)
/// - Audit trail (to tag entries with the operating company)
/// </summary>
public interface ITenantAccessor
{
    /// <summary>
    /// The currently active company ID. Null if no company is selected.
    /// </summary>
    Guid? CurrentCompanyId { get; }

    /// <summary>
    /// Sets the active company for the current scope.
    /// </summary>
    void SetCurrentCompany(Guid companyId);

    /// <summary>
    /// Clears the active company (e.g., returning to company selection screen).
    /// </summary>
    void ClearCurrentCompany();
}
