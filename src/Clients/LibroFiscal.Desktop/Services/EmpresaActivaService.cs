using System;
using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Desktop.Services;

public sealed class EmpresaActivaService : IEmpresaActivaService
{
    public event EventHandler<Guid>? EmpresaCambiadaEvent;

    public Guid? EmpresaActualId { get; private set; }

    public void CambiarEmpresa(Guid companyId)
    {
        if (EmpresaActualId != companyId)
        {
            EmpresaActualId = companyId;
            EmpresaCambiadaEvent?.Invoke(this, companyId);
        }
    }
}
