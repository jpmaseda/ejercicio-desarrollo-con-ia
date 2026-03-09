using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movies_mvc.Data;
using movies_mvc.Models;

namespace movies_mvc.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly MovieDbContext _context;
        public ReviewController(UserManager<Usuario> userManager, MovieDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }


        //Mis reseñas

        // GET: ReviewController
        public async Task<ActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reviews = new List<Review>();
            if (User.IsInRole("Admin"))
            {
                reviews = await _context.Reviews
                    .Include(r => r.Pelicula)
                    .ToListAsync();
            }
            else
            {
                reviews = await _context.Reviews
                    .Include(r => r.Pelicula)
                    .Where(r => r.UsuarioId == userId)
                    .ToListAsync();
            }

            return View(reviews);
        }
             

        // POST: ReviewController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewCreateViewModel review)
        {
            try
            {
                review.UsuarioId = _userManager.GetUserId(User);

                //validación de review única por usuario
                var reviewExiste = _context.Reviews
                    .FirstOrDefault(r => r.PeliculaId == review.PeliculaId && r.UsuarioId == review.UsuarioId);
                if (reviewExiste != null)
                {
                    TempData["ReviewExiste"] = "Ya realizaste una reseña para esta película.";
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }

                if (ModelState.IsValid)
                {
                    var reviewEntity = new Review
                    {
                        PeliculaId = review.PeliculaId,
                        UsuarioId = review.UsuarioId,
                        Rating = review.Rating,
                        Comentario = review.Comentario,
                        FechaReview = DateTime.Now
                    };
                    _context.Reviews.Add(reviewEntity);
                    _context.SaveChanges();
                    return RedirectToAction("Details", "Home", new { id = review.PeliculaId });
                }

                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

        // GET: ReviewController/Edit/5
        public async Task<ActionResult> Edit(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            var review = await _context.Reviews
                .Include(r => r.Pelicula)
                .FirstOrDefaultAsync(r => r.Id == Id);

            if (review == null)
                return NotFound();

            if (user.Id != review.UsuarioId && !_userManager.IsInRoleAsync(user, "Admin").Result)
                return Forbid();
            

            var reviewViewModel = new ReviewCreateViewModel
            {
                Id = review.Id,
                PeliculaId = review.PeliculaId,
                PeliculaTitulo = review.Pelicula?.Titulo,
                UsuarioId = review.UsuarioId,
                Rating = review.Rating,
                Comentario = review.Comentario
            };

            return View(reviewViewModel);
        }

        // POST: ReviewController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ReviewCreateViewModel review)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var reviewEntity = await _context.Reviews.FindAsync(review.Id);

                    if (reviewEntity == null)
                        return NotFound();

                    var user = await _userManager.GetUserAsync(User);
                    if (reviewEntity.UsuarioId != user.Id && !_userManager.IsInRoleAsync(user, "Admin").Result)
                        return Forbid();
                    
                    reviewEntity.Rating = review.Rating;
                    reviewEntity.Comentario = review.Comentario;
                    _context.Reviews.Update(reviewEntity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Review");
                }
                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

    }
}
