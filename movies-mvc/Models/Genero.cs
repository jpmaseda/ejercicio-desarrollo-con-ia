using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace movies_mvc.Models
{
    [DisplayName("Género")]
    public class Genero
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
        public List<Pelicula>? PeliculasGenero { get; set; }
    }
}
