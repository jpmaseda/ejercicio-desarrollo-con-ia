using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using movies_mvc.Data;
using movies_mvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace movies_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieDbContext _context;
        private const int PAGE_SIZE = 8;

        public HomeController(MovieDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string txtBusqueda = "", int generoId = 0, int plataformaId = 0)
        {
            if (page < 1)
                page = 1;

            var consulta = _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Plataforma)
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrEmpty(txtBusqueda))
            {
                consulta = consulta.Where(p => p.Titulo.Contains(txtBusqueda));
            }

            if (generoId > 0)
            {
                consulta = consulta.Where(p => p.GeneroId == generoId);
            }

            if (plataformaId > 0)
            {
                consulta = consulta.Where(p => p.PlataformaId == plataformaId);
            }

            int totalPeliculas = await consulta.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalPeliculas / PAGE_SIZE);
            int skip = (page - 1) * PAGE_SIZE;

            //Agrupar las reviews por PeliculaId y calcular el promedio de rating para cada película
            var ratings = _context.Reviews
                .GroupBy(r => r.PeliculaId)
                .Select(g => new
                {
                    PeliculaId = g.Key,
                    Promedio = (double?)g.Average(r => r.Rating)
                });

            //en el GroupBy conviene aplicar Skip/Take antes del join para que SQL no procese todo el catálogo
            var peliculasPagina = consulta
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(PAGE_SIZE);

            //Realizar un left join entre las películas y los promedios de rating, ordenando por Id de película
            //DefaultIfEmpty() hace el LEFT JOIN, así las películas sin reviews quedan con rating 0.
            // ?? operador de coalescencia nula: “Si el valor de la izquierda es null, usa el de la derecha.”
            var peliculas = await (
                from p in peliculasPagina
                join r in ratings
                    on p.Id equals r.PeliculaId into ratingGroup
                from rg in ratingGroup.DefaultIfEmpty()
                select new Pelicula
                {
                    Id = p.Id,
                    Titulo = p.Titulo,
                    PosterUrlPortada = p.PosterUrlPortada,
                    FechaLanzamiento = p.FechaLanzamiento,
                    Genero = p.Genero,
                    Plataforma = p.Plataforma,
                    PromedioRating = rg.Promedio ?? 0
                })
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Busqueda = txtBusqueda;

            var generos = await _context.Generos.OrderBy(g => g.Descripcion).ToListAsync();
            generos.Insert(0, new Genero { Id = 0, Descripcion = "Géneros" });

            ViewBag.GeneroId = new SelectList(
                generos,
                "Id",
                "Descripcion",
                generoId);

            var plataformas = await _context.Plataformas.OrderBy(p => p.Nombre).ToListAsync();
            plataformas.Insert(0, new Plataforma { Id = 0, Nombre = "Plataformas" });
            ViewBag.PlataformaId = new SelectList(
               plataformas,
               "Id",
               "Nombre",
                plataformaId);

            return View(peliculas);
        }

        public async Task<IActionResult> Details(int Id)
        {
            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .Include(p => p.Plataforma)
                .Include(p => p.ListaReviews)
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (pelicula == null)
                return NotFound();

            // Calcular el promedio de rating para la película
            pelicula.PromedioRating = pelicula.ListaReviews?.Any() == true
                   ? pelicula.ListaReviews.Average(r => r.Rating)
                   : 0;

            ViewBag.UserReview = false;

            // Si el usuario está autenticado, verificamos si ya ha dejado una review para esta película
            if (User?.Identity?.IsAuthenticated == true && pelicula.ListaReviews != null)
            {
                //alternativa a usar el UserManager
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               // ViewBag.UserReview = !(pelicula.ListaReviews.FirstOrDefault(r => r.UsuarioId == userId) == null);

                ViewBag.UserReview = pelicula.ListaReviews.Any(r => r.UsuarioId == userId);
            }

            return View(pelicula);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
