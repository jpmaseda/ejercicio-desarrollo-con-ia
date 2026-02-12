using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace movies_mvc.Models
{
    public class Plataforma
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre de la plataforma es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre de la plataforma no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; }
        [Url(ErrorMessage = "La URL no es válida.")]
        public string Url { get; set; }
        [Url(ErrorMessage = "La URL del logo no es válida.")]
        [DisplayName("Logo")]
        public string LogoUrl { get; set; }
        public List<Pelicula>? PeliculasPlataforma { get; set; }
    }
}
