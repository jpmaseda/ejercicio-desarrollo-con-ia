using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace movies_mvc.Models
{
    public class Pelicula
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El título no puede exceder los 100 caracteres.")]
        public string Titulo { get; set; }
        [DataType(DataType.Date)]
        public DateTime FechaLanzamiento { get; set; }
        [Required]
        [Range(1, 500, ErrorMessage = "La duración debe ser entre 1 y 500 minutos.")]
        public int MinutosDuracion { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "La sinopsis no puede exceder los 1000 caracteres.")]
        public string Sinopsis { get; set; }
        [Required]
        [Url(ErrorMessage = "La URL del póster no es válida.")]
        public string PosterUrlPortada { get; set; }
        public int GeneroId { get; set; }
        public Genero? Genero { get; set; }
        public int PlataformaId { get; set; }
        public Plataforma? Plataforma { get; set; }
        [NotMapped]
        public int PromedioRating { get; set; }
        public List<Review>? ListaReviews { get; set; }
        public List<Favorito>? UsuariosFavorito { get; set; }
    }
}
