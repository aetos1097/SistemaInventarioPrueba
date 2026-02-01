using ProductsSales.Application.Interfaces;

namespace ProductsSales.Infrastructure.Services;

/// <summary>
/// Implementación vacía de IBlobStorageService. Usada cuando AzureStorage:SasUrl no está configurado.
/// UploadImage lanza; DeleteImage no hace nada.
/// </summary>
public class NoOpBlobStorageService : IBlobStorageService
{
    /// <summary>Lanza excepción indicando que Blob Storage no está configurado.</summary>
    public Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        throw new InvalidOperationException("Azure Blob Storage no está configurado. Configure AzureStorage:SasUrl en appsettings.json para subir imágenes.");
    }

    /// <summary>No hace nada. Evita errores al eliminar producto sin imagen.</summary>
    public Task DeleteImageAsync(string imageUrl) => Task.CompletedTask;
}
