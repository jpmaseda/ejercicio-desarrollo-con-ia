using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using movies_mvc.Models;
using movies_mvc.Service;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace movies_mvc.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ImageStorage _imageStorage;
        public UsuarioController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, ImageStorage imageStorage)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _imageStorage = imageStorage;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _signInManager.PasswordSignInAsync(usuario.Email, usuario.Password, usuario.RememberMe, lockoutOnFailure: false);
                if (resultado.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Inicio de sesión inválido.");
                }
            }

            return View(usuario);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                var nuevoUsuario = new Usuario
                {
                    UserName = usuario.Email,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    ImagenUrlPerfil = "/images/default-profile.png"
                };
                var resultado = await _userManager.CreateAsync(nuevoUsuario, usuario.Password);
                if (resultado.Succeeded)
                {
                    await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(usuario);
        }
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            var perfilVM = new PerfilViewModel
            {
                Nombre = usuarioActual.Nombre,
                Apellido = usuarioActual.Apellido,
                Email = usuarioActual.Email,
                FechaNacimiento = usuarioActual.FechaNacimiento,
                ImagenUrlPerfil = usuarioActual.ImagenUrlPerfil
            };
            return View(perfilVM);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel perfilVM)
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid && perfilVM.FechaNacimiento.HasValue)
            {
                try
                {
                    if (perfilVM.ImagenPerfil is not null && perfilVM.ImagenPerfil.Length > 0)
                    {
                        // opcional: borrar la anterior (si no es placeholder)
                        if (!string.IsNullOrWhiteSpace(usuarioActual.ImagenUrlPerfil))
                            await _imageStorage.DeleteAsync(usuarioActual.ImagenUrlPerfil);

                        var nuevaRuta = await _imageStorage.SaveImageAsync(usuarioActual.Id, perfilVM.ImagenPerfil);
                        usuarioActual.ImagenUrlPerfil = nuevaRuta;
                        perfilVM.ImagenUrlPerfil = nuevaRuta;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(perfilVM);
                }

                usuarioActual.Nombre = perfilVM.Nombre;
                usuarioActual.Apellido = perfilVM.Apellido;
                if(perfilVM.FechaNacimiento.HasValue)
                    usuarioActual.FechaNacimiento = perfilVM.FechaNacimiento.Value;

                var resultado = await _userManager.UpdateAsync(usuarioActual);

                if (resultado.Succeeded)
                {
                    TempData["Mensaje"] = "Perfil actualizado correctamente.";
                    return RedirectToAction(nameof(Perfil));
                }
                else
                {
                    foreach (var error in resultado.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }                    
                }
            }

            //para evitar mostratr datos vacios en caso de modelo no valido, ya que perfilVM tiene email null y puede tener otros campos vacios,
            //entonces se vuelven a cargar los datos del usuario actual

            perfilVM.Email = usuarioActual.Email;
            perfilVM.Nombre = usuarioActual.Nombre;
            perfilVM.Apellido = usuarioActual.Apellido;
            perfilVM.ImagenUrlPerfil = usuarioActual.ImagenUrlPerfil;
            //perfilVM.FechaNacimiento = usuarioActual.FechaNacimiento;
            if (!perfilVM.FechaNacimiento.HasValue)
                ModelState.AddModelError(string.Empty, "La fecha de nacimiento es obligatoria.");
                        
            return View(perfilVM);
        }
    }
}
