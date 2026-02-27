using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace movies_mvc.Service
{
    public class ImageStorage
    {
        private readonly IWebHostEnvironment _environment;
        private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
        };
        public ImageStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        //cancellation token para poder cancelar la operación si es necesario, por ejemplo, si el usuario decide cancelar la carga de la imagen antes de que se complete.
        //Si el usuario cierra la ventana del navegador o navega a otra página, el token de cancelación se puede activar para detener la operación de carga y liberar recursos.
        //al estar en default, el token de cancelación no se activará a menos que se cancele explícitamente desde el código que llama a este método.
        public async Task<string> SaveImageAsync(string userId, IFormFile imageFile, CancellationToken ct = default)
        {
            //validaciones
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("No se ha proporcionado un archivo de imagen válido.");
            }
            if (imageFile.Length > 2 * 1024 * 1024)
            {
                throw new ArgumentException("El tamaño del archivo de imagen no debe exceder los 2 MB.");
            }
            if (!_allowedExtensions.Contains(imageFile.ContentType))
            {
                throw new ArgumentException("El formato de imagen no es permitido. Solo se permiten JPEG, PNG y WEBP.");
            }
            //carga de la imagen utilizando la biblioteca ImageSharp, que es una biblioteca de procesamiento de imágenes en .NET.
            //Esta biblioteca permite cargar, manipular y guardar imágenes de manera eficiente. Evita problemas de seguridad asociados con la carga de archivos,
            //como ataques de deserialización o ejecución de código malicioso, al no depender de las bibliotecas de procesamiento de imágenes del sistema operativo.
            using var image = await Image.LoadAsync(imageFile.OpenReadStream(), ct);

            //recortar y redimensionar (512x512)
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop,
                Size = new Size(512, 512)
            }));

            //normalizar la imagen a un formato específico (webp) para garantizar la compatibilidad y reducir el tamaño del archivo.
            var ext = ".webp";
            var folderRel = $"uploads/avatar/{userId}";
            var folderAbs = Path.Combine(_environment.WebRootPath, folderRel);

            Directory.CreateDirectory(folderAbs);

            var uniqueFileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(folderAbs, uniqueFileName);
            var relPath = $"/{folderRel}/{uniqueFileName}".Replace("\\", "/");
            
            await image.SaveAsWebpAsync(absPath, ct); //necesita sixlabors.imagesharp.formats.webp

            return relPath;
        }

        public Task DeleteAsync(string? relativePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }
            
            var abs = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            
            if(File.Exists(abs))
            {
                File.Delete(abs);
            }
            
            return Task.CompletedTask;
        }
    }
}
