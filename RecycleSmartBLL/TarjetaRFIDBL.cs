using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class TarjetaRFIDBL
    {
        private readonly TarjeraRFIDDAL _tarjetaRFIDDAL;
        private readonly UsuarioInstitucionDAL _usuarioInstitucionDAL;
        private readonly ContenedorDAL _contenedorDAL;
        public TarjetaRFIDBL(string conec)
        {
            _tarjetaRFIDDAL=new TarjeraRFIDDAL(conec);
            _contenedorDAL=new ContenedorDAL(conec);
            _usuarioInstitucionDAL=new UsuarioInstitucionDAL(conec);
        }

        private static ValidarDatos(TarjetaRFID tarjeta)
        {
            if(tarjeta.UID=="")
            {
                throw new Exception("El UID Debe tener 8 caracteres");
            }
            if(tarjeta.FechaVecimiento.Date<=DateTime.Today)
            {
                throw new Exception("La fecha de vencimiento debe ser Distinta a la fecha de hoy");
            }
            if(tarjeta.DiasHabilitados==DiasSemana.DiaSemana.Ninguno)
            {
                throw new Exception("Debe seleccionar algun dia valido");
            }
            if(tarjeta.HoraFin<=tarjeta.HoraInicio)
            {
                throw new Exception("La hora de fin debe ser posterior a la hora de inicio ");
            }    
        }

    }
}
