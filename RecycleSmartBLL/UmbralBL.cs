using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class UmbralBL
    {
        private readonly UmbralDAL _umbralDAl;
        private readonly ContenedorDAL _contenedorDAl;
        public UmbralBL( string conecta)
        {
            _umbralDAl= new UmbralDAL( conecta );
            _contenedorDAl=new ContenedorDAL( conecta );
        }

        private static void ValidarUmbral(Umbral u)
        {
            if(u.NivelAdvertencia>=u.NivelCritico)
            {
                throw new Exception("El Nivel de Advertencia debe ser menor al Nivel Critico");
            }
            if(u.NivelAdvertencia<=0 || u.NivelCritico<=0)
            {
                throw new Exception("Los Valores de Nivel deben ser mayores a 1");
            }
            if(u.HumedadMaxima<=0)
            {
                throw new Exception("La Humedad maxima debe estar entre 1% y 100%");
            }
            if(u.TemperaturaMaxima<-10 || u.TemperaturaMaxima>80)
            {
                throw new Exception("Error de valores de temperatura maxima");
            }

            if(u.MinutosSinConexion<1 || u.MinutosSinConexion>1440)
            {
                throw new Exception("Error de Valores");
            }

        }

        private Contenedor ObtenerContenedor(int contendorid,int empresaid)
        {
            if(_contenedorDAl.BuscarContenedorXId(contendorid,empresaid) is not Contenedor contenedor)
            {
                throw new Exception("No existe Contenedor!");
            }
            if(contenedor.Estado==TipoEstado.Estados.Baja)
            {
                throw new Exception("Contendor dado de baja");
            }
            return contenedor;
        }

        public Umbral Obtener(int contendorid,int empresaid)
        {
            ObtenerContenedor(contendorid,empresaid);
            return _umbralDAl.BuscarxContenedor(contendorid, empresaid) ?? new Umbral { ContenedorId = contendorid };
        }

        public bool Guardar(Umbral u, int empresaid)
        {
            ObtenerContenedor(u.ContenedorId,empresaid);
            ValidarUmbral(u);
            u.EstadoSync = TipoEstado.EstadoSync.Pendiente;
            if(_umbralDAl.BuscarxContenedor(u.ContenedorId, empresaid)==null)
            {
                u.UltimaActualizacion = DateTime.Now;
                _umbralDAl.AgregarUmbral(u);
                return true;
            }
            if(_umbralDAl.ModificarUmbral(u,empresaid)==0)
            {
                throw new Exception("No se Pudo Guardar Configuracion");
            }
            return false;
        }

        public HashSet<int> ContenedorContieneUmbral(int institucionid, int empresaid)
        {
            return _umbralDAl.ContenedorConUmbral(institucionid, empresaid).Select(u => u.ContenedorId).ToHashSet();
        }
        
        public Dictionary<int,Umbral> UmbralxInstitucion(int institucionid,int empresaid)
        {
            return _umbralDAl.ContenedorConUmbral(institucionid, empresaid).ToDictionary(u => u.ContenedorId);
        }

        public static string ClasificarLectura(Telemetria lectura, Umbral? umbral)
        {
            var u = umbral?? new Umbral();
            if((DateTime.Now-lectura.FechaHora).TotalMinutes>u.MinutosSinConexion)
            {
                return "sin datos";
            }
            if (lectura.Nivel >= u.NivelCritico)
            {
                return "Critico";
            }
            if(lectura.Nivel>=u.NivelAdvertencia)
            {
                return "Advertencia";
            }
            return "Normal";
        }
    }
}
