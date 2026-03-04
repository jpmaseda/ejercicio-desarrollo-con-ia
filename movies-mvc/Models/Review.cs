using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace movies_mvc.Models
{
    public class Review
    {
        public int Id { get; set; }
        [DisplayName("Película")]
        public int PeliculaId { get; set; }
        public Pelicula? Pelicula { get; set; }
        [DisplayName("Usuario")]
        public string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        [StringLength(500)]
        public string Comentario { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Fecha")]
        public DateTime FechaReview { get; set; }
        //rowversion for concurrency control
        [Timestamp]
        public byte[] RowVersion { get; set; }

    }
    public class ReviewCreateViewModel
    {
        public int? Id { get; set; }
        
        public int PeliculaId { get; set; }
        public string? PeliculaTitulo { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        [Required(ErrorMessage = "La calificación es obligatoria.")]
        [Range(1, 5, ErrorMessage ="La calificación debe ser entre 1 y 5 estrellas.")]
        public int Rating { get; set; }
        [Required(ErrorMessage ="El comentario es requerido.")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres.")]
        public string Comentario { get; set; } = string.Empty;
    }
}
