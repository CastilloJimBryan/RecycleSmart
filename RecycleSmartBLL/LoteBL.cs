using RecycleSmartBE;
using RecycleSmartDAL;
using Servicio;
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

        public List<Lote> ListarLote(int empresaid,int institucionid,bool cerrada)
        {
            return _loteDal.ListarLote(empresaid,institucionid,cerrada);
        }

        public Lote ObtenerLote(int loteid,int empresaid)
        {
            return _loteDal.BuscarxId(loteid, empresaid)??throw new Exception("Lote no existe");
        }
        public List<(DateTime fecha,string usuario,decimal volumen)> Deposito(int loteid)
        {
            return _loteDal.Depositos(loteid);
        }

        public (bool integro,string mensaje) VerificarIntegridad(Lote lote)
        {
            string anterior = Integridad.HashInicial;
            foreach (var item in lote.Hitos)
            {
                if(item.HashAnterior!=anterior)
                {
                    return(false,$"La Cadena Esta Ropta en el hito {item.TipoHito} : No Continua ");
                }
                if (Integridad.hashito(lote.Codigo, item) != item.Hash)
                {
                    return (false, $"El Hito {item.TipoHito} del {item.Fecha: dd/MM/yyyy HH:mm} fue alterado: los datos no coinciden con la firma");
                }
                anterior = item.Hash;
            }
            if(lote.HashFinal!=null && lote.HashFinal!=anterior)
            {
                return (false, $"El sello de cierre del lote no coincide con sus hitos");

            }
            return (true, $"Integro: se verificaroon {lote.Hitos.Count} hitos y la cadena esta completa");
        }

        // Qué plazo corre ahora y cuánto se consumió (para la barra)
        public static (string Etapa, DateTime? Limite, int Porcentaje, bool Vencido) PlazoActual(Lote l)
        {
            DateTime desde, limite;
            string etapa;

            if (l.EstadoLote is TipoEstado.EstadoLote.EnCurso or TipoEstado.EstadoLote.RecoleccionSolicitadas)
            {
                etapa = "Recolección";
                desde = l.FechaGeneracion;
                limite = l.FechaRecoleccion;
            }
            else if (l.EstadoLote == TipoEstado.EstadoLote.Recolectado && l.FechaTratamiento != null)
            {
                etapa = "Tratamiento";
                limite = l.FechaTratamiento.Value;
                desde = limite.AddHours(-HorasTratamiento);
            }
            else
            {
                return ("", null, 0, false);   // tratado o cerrado: no corre ningún plazo de la ley
            }

            double total = (limite - desde).TotalMinutes;
            double usado = (DateTime.Now - desde).TotalMinutes;
            int porcentaje = (int)Math.Clamp(usado / total * 100, 0, 100);
            return (etapa, limite, porcentaje, DateTime.Now > limite);
        }
    }
}
