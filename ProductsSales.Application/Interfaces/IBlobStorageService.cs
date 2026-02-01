namespace ProductsSales.Application.Interfaces;

/// <summary>
/// Servicio para subir y eliminar imágenes en almacenamiento en la nube (ej. Azure Blob Storage).
/// Permite mockear en tests sin conectar a Azure.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>Sube una imagen y devuelve la URL pública.</summary>
    Task<string> UploadImageAsync(Stream imageStream, string fileName);

    /// <summary>Elimina una imagen por su URL.</summary>
    Task DeleteImageAsync(string imageUrl);
}
