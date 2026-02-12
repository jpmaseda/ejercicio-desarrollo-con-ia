using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace movies_mvc.Models
{
    [DisplayName("Película")]
    public class Pelicula
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
        [DisplayName("Título")]
        public string Titulo { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Lanzamiento")]
        public DateTime FechaLanzamiento { get; set; }
        [Required]
        [Range(1, 500, ErrorMessage = "La duración debe ser entre 1 y 500 minutos.")]
        [DisplayName("Duración (minutos)")]
        public int MinutosDuracion { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "La sinopsis no puede exceder los 1000 caracteres.")]
        public string Sinopsis { get; set; }
        [Required]
        [Url(ErrorMessage = "La URL del póster no es válida.")]
        [DisplayName("URL del Póster")]
        public string PosterUrlPortada { get; set; }
        [DisplayName("Género")]
        public int GeneroId { get; set; }
        public Genero? Genero { get; set; }
        [DisplayName("Plataforma")]
        public int PlataformaId { get; set; }
        public Plataforma? Plataforma { get; set; }
        [NotMapped]
        [DisplayName("Rating")]
        public int PromedioRating { get; set; }
        public List<Review>? ListaReviews { get; set; }
        public List<Favorito>? UsuariosFavorito { get; set; }
    }
}
