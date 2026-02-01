using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using ProductsSales.Application.Interfaces;

namespace ProductsSales.Infrastructure.Services;

/// <summary>
/// Implementación de IBlobStorageService usando Azure Blob Storage.
/// Requiere AzureStorage:SasUrl en appsettings.json.
/// </summary>
public class AzureBlobService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    /// <summary>Conecta al contenedor usando la URL SAS de configuración.</summary>
    public AzureBlobService(IConfiguration configuration)
    {
        var sasUrl = configuration["AzureStorage:SasUrl"]
            ?? throw new InvalidOperationException("Azure Storage SAS URL not configured");

        _containerClient = new BlobContainerClient(new Uri(sasUrl));
    }

    /// <summary>Sube la imagen a la carpeta productssales/ y devuelve su URL.</summary>
    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var uniqueFileName = $"productssales/{Guid.NewGuid()}_{fileName}"; // Nombre único para evitar colisiones
        var blobClient = _containerClient.GetBlobClient(uniqueFileName);

        await blobClient.UploadAsync(imageStream, new BlobHttpHeaders
        {
            ContentType = GetContentType(fileName)
        });

        return blobClient.Uri.ToString();
    }

    /// <summary>Elimina la imagen en Blob a partir de su URL. No lanza si falla.</summary>
    public async Task DeleteImageAsync(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return;

        try
        {
            var uri = new Uri(imageUrl);
            var segments = uri.Segments;
            if (segments.Length >= 2)
            {
                var blobName = string.Join("", segments.Skip(segments.Length - 2));
                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();
            }
        }
        catch (Exception)
        {
           
        }
    }

    /// <summary>Obtiene el Content-Type según la extensión del archivo.</summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
