namespace movies_mvc.Models
{
    public class Favorito
    {
        //Puedo usar Fluent API para la clase intermedia, pero como le agregue la Fecha a la clase Favorito,
        //no puedo usarla como tabla intermedia, por lo que tengo que agregarle un Id a la clase Favorito
        //para que sea una tabla normal y no una tabla intermedia 
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int PeliculaId { get; set; }
        public Pelicula? Pelicula { get; set; }
        public DateTime Fecha { get; set; }
    }
}
