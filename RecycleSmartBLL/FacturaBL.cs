using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class FacturaBL
    {
        private readonly FacturaDAL _facturaDAL;

        public const int DiasVencimiento = 10;
        public FacturaBL(string conect)
        {
            _facturaDAL=new FacturaDAL(conect);
        }

        public List<Factura>ListarFacturaXEmpresa(int empresaId=0)
        {
            return _facturaDAL.ListarFacturaXEmpresa(empresaId);
        }

        public int EmitirMes(int anio,int mes)
        {
            if(mes<1||mes>12)
            {
                throw new Exception("Mes Invalido");
            }
            var periodo = new DateTime(anio, mes, 1);
            var mesActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (periodo > mesActual) throw new Exception("No se Puede emitir factura de un mes que no empezo");

            return _facturaDAL.EmitirFacturaxMes(anio, mes, DiasVencimiento);
        }

        public void RegistarPago(int facturaId)
        {
            if(_facturaDAL.RegistrarPago(facturaId)==0)
            {
                throw new Exception("No EXiste Factura");
            }

        }

        public static bool TieneMoraGrave(List<Factura> facturas) => facturas.Any(f => f.MoraGrave);

    }
}
