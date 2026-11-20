using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.RecycleSmart)]
    public class FacturacionController:Controller
    {
        private readonly FacturaBL _facuraBL;
        private readonly EmpresaLogisticaBLL _empresaLogisticaBLL;

        public FacturacionController(IConfiguration config)
        {
            string conect = config.GetConnectionString("BDTFI")!;
            _facuraBL = new FacturaBL(conect);
            _empresaLogisticaBLL=new EmpresaLogisticaBLL(conect);
        }

        public IActionResult Index(int empresaId=0,string? estado=null)
        {
            var facturas=_facuraBL.ListarFacturaXEmpresa(empresaId);
            facturas = estado switch
            {
                "Pendiente" => facturas.Where(f => f.EstadoFactura == TipoEstado.EstadoFactura.Pendiente && f.Vencida).ToList(),
                "Vencida" => facturas.Where(f=>f.Vencida).ToList(),
                "Pagada" => facturas.Where(f => f.EstadoFactura == TipoEstado.EstadoFactura.Pagada).ToList(),
                _=>facturas
            };

            ViewBag.Empresa = _empresaLogisticaBLL.ListarEmpresa();
            ViewBag.EmpresaId = empresaId;
            ViewBag.Estado=estado;
            return View(facturas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Emitir(int anio, int mes)
        {
            try
            {
                int emitidas = _facuraBL.EmitirMes(anio, mes);
                TempData["Mensaje"] = emitidas == 0
                    ? $"Todas las empresas activas ya tenían la factura de {mes:D2}/{anio}."
                    : $"Se emitieron {emitidas} facturas del período {mes:D2}/{anio}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrarPago(int id, string? returnUrl)
        {
            try
            {
                _facuraBL.RegistarPago(id);
                TempData["Mensaje"] = "Pago registrado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
