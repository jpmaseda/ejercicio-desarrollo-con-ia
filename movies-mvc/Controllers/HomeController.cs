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
            var peliculas = await consulta
                .Include(p => p.Genero)
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(PAGE_SIZE)
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

            ViewBag.UserReview =false;
            if (User?.Identity?.IsAuthenticated == true && pelicula.ListaReviews != null)
            {
                //alternativa a usar el UserManager
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.UserReview = !(pelicula.ListaReviews.FirstOrDefault(r => r.UsuarioId == userId) == null);
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
