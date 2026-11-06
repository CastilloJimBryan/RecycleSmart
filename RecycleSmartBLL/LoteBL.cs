using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class LoteBL
    {
        public const int DiasRecoleccion = 5;
        public const int HorasTratamiento = 72;
        private readonly LoteDAL _loteDal;
        public LoteBL(string contectar)
        {
            _loteDal=new LoteDAL(contectar);
        }
        public(int Loteid,string codigo,bool creado)  RegistrarDeposito(long EventoAccesoId)
        {
            return _loteDal.RegistrarDeposito(EventoAccesoId,DiasRecoleccion);
        }
    }
}
