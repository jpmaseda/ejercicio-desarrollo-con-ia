using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movies_mvc.Data;
using movies_mvc.Models;

namespace movies_mvc.Controllers
{
    public class ReviewController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly MovieDbContext _context;
        public ReviewController(UserManager<Usuario> userManager, MovieDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]

        // GET: ReviewController
        public async Task<ActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var reviews = await _context.Reviews
                .Include(r => r.Pelicula)
                .Where(r => r.UsuarioId == userId)
                .ToListAsync();

            return View(reviews);
        }

        // GET: ReviewController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReviewController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReviewController/Create
        [Authorize]
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
        [Authorize]
        // GET: ReviewController/Edit/5
        public ActionResult Edit(int Id)
        {
            var userId = _userManager.GetUserId(User);
            var review = _context.Reviews
                .Include(r => r.Pelicula)
                .FirstOrDefault(r => r.Id == Id && r.UsuarioId == userId);

            if (review == null)            
                return NotFound();
            
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
        public ActionResult Edit(ReviewCreateViewModel review)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var reviewEntity = _context.Reviews.Find(review.Id);
                    if (reviewEntity == null)
                        return NotFound();
                    var userId = _userManager.GetUserId(User);
                    if (reviewEntity.UsuarioId != userId)
                        return Forbid();
                    reviewEntity.Rating = review.Rating;
                    reviewEntity.Comentario = review.Comentario;
                    _context.Reviews.Update(reviewEntity);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Review");
                }
                return View(review);
            }
            catch
            {
                return View(review);
            }
        }

        // GET: ReviewController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReviewController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
