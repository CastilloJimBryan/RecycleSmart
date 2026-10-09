using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginBLL _loginBL;
        public LoginController(IConfiguration config)
        {
            string conexion = config.GetConnectionString("BDTFI")!;
            _loginBL = new LoginBLL(conexion);
        }

        public IActionResult Index(string returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return IrAPanel();
            }
            if(!string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.MensajeAcceso = "Debe Iniciar Sesion Para Acceder ";
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Index(string correo,string clave,string? returnUrl)
        {
            try
            {
                var resultado = _loginBL.Autenticar(correo, clave);
                if(resultado?.usuarioEmpresaLogistica is UsuarioEmpresaLogistica usuarioempresa)
                {
                    await ManejosSesiones.IniciarSesionEmpresa(HttpContext, usuarioempresa);
                    return Volver(returnUrl, "Dashboard");
                }
                if(resultado?.usuarioRecycleSmart is UsuarioRecycleSmart usuarioRecycleSmart)
                {
                    await ManejosSesiones.IniciarSesionRecycleSmart(HttpContext, usuarioRecycleSmart);
                    return Volver(returnUrl, "PanelCentral");
                }

                ModelState.AddModelError("", "Error con el Usuario o Clave");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            ViewBag.ReturnUrl= returnUrl;
            return View();
        }
        public async Task<IActionResult> Salir()
        {
            await ManejosSesiones.Cerrar(HttpContext);
            return RedirectToAction("Index");
        }
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        private IActionResult Volver(string? returnUrl, string panel)
        {
            if(string.IsNullOrEmpty(returnUrl)&& Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index",panel);
        }

        private IActionResult IrAPanel()
        {
            return ManejosSesiones.ObtenerTipoCuenta(User) == TipoDeCuenta.EmpresaLogistica ?
                RedirectToAction("Index", "Dashboard"):
                RedirectToAction("Index", "PanelCentral");
        }
    }
}
