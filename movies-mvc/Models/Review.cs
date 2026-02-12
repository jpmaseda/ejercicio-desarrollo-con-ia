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
}
