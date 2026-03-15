using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace movies_mvc.Service
{
    public class LlmService
    {
        private readonly string _model;
        private readonly Client _client;

        public LlmService(IConfiguration configuration)
        {
            var apiKey = configuration["GEMINI_API_KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("La clave 'GEMINI_API_KEY' no se encontró en User Secrets ni en la configuración.");
            }
            _model = "gemini-2.5-flash";

            // Inicializamos el cliente y el modelo una sola vez para mayor eficiencia
            // Pasamos null al primero (porque no usamos Vertex AI) y la key al segundo.
            _client = new Client(vertexAI: false, apiKey: apiKey);
        }

        public async Task<string> ObtenerSpoilerAsync(string tituloPelicula)
        {
            if (string.IsNullOrWhiteSpace(tituloPelicula))
                throw new ArgumentException("El título de la película no puede estar vacío.", nameof(tituloPelicula));

            string prompt = $@"Genera un pequeño spoiler (máximo 2-3 oraciones) sobre la película ""{tituloPelicula}"". 
                        El spoiler debe revelar algún giro interesante de la trama sin arruinar completamente la experiencia. 
                        Sé conciso y cautivador.";

            return await ConsultarLlmAsync(prompt);
        }


        public async Task<string> ObtenerResumenAsync(string tituloPelicula)
        {
            if (string.IsNullOrWhiteSpace(tituloPelicula))
                throw new ArgumentException("El título de la película no puede estar vacío.", nameof(tituloPelicula));

            string prompt = $@"Proporciona un resumen breve (máximo 3-4 oraciones) de la película ""{tituloPelicula}"". 
                        Incluye el género, la premisa principal y por qué es relevante o interesante. 
                        No incluyas spoilers importantes.";

            return await ConsultarLlmAsync(prompt);
        }


        private async Task<string> ConsultarLlmAsync(string prompt)
        {
            try
            {
                // Siguiendo el ejemplo de Google AI Studio:
                var response = await _client.Models.GenerateContentAsync(
                    model: _model,
                    contents: prompt
                );

                // Navegamos por la estructura: Candidatos -> Contenido -> Partes -> Texto
                var text = response.Candidates?[0]?.Content?.Parts?[0]?.Text;

                return text?.Trim() ?? "No se pudo obtener una respuesta.";
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Error de conexión con la API de Gemini: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al procesar la solicitud: {ex.Message}", ex);
            }
        }

        public async Task<string> ConsultaImagenAsync(string tituloPelicula)
        {
            if (string.IsNullOrWhiteSpace(tituloPelicula))
                throw new ArgumentException("El título de la película no puede estar vacío.", nameof(tituloPelicula));

            string prompt = $@"Genera una imagen en formato PNG de un póster de película estilo kawaii para ""{tituloPelicula}"". 
                            Colores pasteles, personajes cabezones y fondo minimalista.";
            return await GenImagenLlmAsync(prompt);

        }
        private async Task<string> GenImagenLlmAsync(string prompt)
        {
            try
            {
                // Configuramos la respuesta para que sepa que queremos una imagen
                var config = new GenerateContentConfig
                {
                    // Forzamos a que la respuesta sea una imagen (según la doc de Nano Banana)
                    ResponseModalities = new List<string> { "IMAGE" }
                };

                // Usamos GenerateContentAsync porque es el método que el JSON de la API con mi KEY dice soportar
                var response = await _client.Models.GenerateContentAsync(
                    model: _model + "-image",
                    contents: prompt,
                    config: config
                );

                // En generateContent, la imagen viene en los "Parts" de la respuesta
                // Buscamos la primera parte que contenga InlineData
                var imagePart = response.Candidates?[0]?.Content?.Parts?
                                .FirstOrDefault(p => p.InlineData != null);

                if (imagePart?.InlineData?.Data != null)
                {
                    // Convertimos los bytes a Base64 para que el navegador pueda leerlo como imagen

                    string base64Image = Convert.ToBase64String(imagePart.InlineData.Data);
                    string mimeType = imagePart.InlineData.MimeType ?? "image/png";

                    // Retornamos el string formateado para el atributo 'src' de HTML
                    return $"data:{mimeType};base64,{base64Image}";
                }

                return "No se pudo generar la imagen.";
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Error de conexión con la API de Gemini: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al procesar la solicitud: {ex.Message}", ex);
            }
        }
       
    }
}
