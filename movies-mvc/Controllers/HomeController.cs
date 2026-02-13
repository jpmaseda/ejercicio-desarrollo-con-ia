using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using movies_mvc.Data;
using movies_mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace movies_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly MovieDbContext _context;
        public HomeController(MovieDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var peliculas = await _context.Peliculas
                .Include(p => p.Genero) // Incluye la información del género relacionado
                .ToListAsync();
            return View(peliculas);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
