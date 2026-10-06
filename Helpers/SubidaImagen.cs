namespace RestauranteMonster.Helpers;

public static class SubidaImagen
{
    private static readonly string[] TiposPermitidos = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxTamano = 2 * 1024 * 1024;

    public static async Task<string?> GuardarAsync(IFormFile? archivo, IWebHostEnvironment env)
    {
        if (archivo == null || archivo.Length == 0) return null;
        if (archivo.Length > MaxTamano)
            throw new InvalidOperationException("La imagen no puede superar 2 MB.");
        if (!TiposPermitidos.Contains(archivo.ContentType))
            throw new InvalidOperationException("Solo se permiten imágenes JPEG, PNG o WEBP.");

        var extension    = Path.GetExtension(archivo.FileName);
        var nombreArchivo = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
        var carpeta      = Path.Combine(env.WebRootPath, "resources", "productos");
        Directory.CreateDirectory(carpeta);

        await using var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create);
        await archivo.CopyToAsync(stream);

        return $"resources/productos/{nombreArchivo}";
    }
}
