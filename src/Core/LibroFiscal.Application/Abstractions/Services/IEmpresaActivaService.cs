using System;

namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Mantiene el estado global de la empresa actualmente seleccionada en la aplicación.
/// Siguiendo la arquitectura MVVM, los ViewModels se suscribirán a EmpresaCambiadaEvent 
/// para recargar sus datos cuando el usuario cambie de empresa en el selector global.
/// </summary>
public interface IEmpresaActivaService
{
    /// <summary>
    /// Evento que se dispara cuando cambia la empresa seleccionada.
    /// Recibe el nuevo CompanyId (Guid) como parámetro.
    /// </summary>
    event EventHandler<Guid> EmpresaCambiadaEvent;

    /// <summary>
    /// Devuelve el ID (Guid) de la empresa actualmente activa. 
    /// Puede ser null si la aplicación acaba de iniciar y aún no hay empresas.
    /// </summary>
    Guid? EmpresaActualId { get; }

    /// <summary>
    /// Cambia la empresa activa en el sistema y dispara el evento EmpresaCambiadaEvent.
    /// </summary>
    void CambiarEmpresa(Guid companyId);
}
