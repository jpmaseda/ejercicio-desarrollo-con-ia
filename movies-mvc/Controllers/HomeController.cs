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
        private const int PAGE_SIZE = 8;

        public HomeController(MovieDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            if (page < 1)            
                page = 1;

            int totalPeliculas = await _context.Peliculas.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalPeliculas / PAGE_SIZE);

            int skip = (page - 1) * PAGE_SIZE;
            var peliculas = await _context.Peliculas
                .Include(p => p.Genero)
                .OrderBy(p => p.Id)
                .Skip(skip)
                .Take(PAGE_SIZE)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
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
