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
            _tarjetaRFIDDAL = new TarjeraRFIDDAL(conec);
            _contenedorDAL = new ContenedorDAL(conec);
            _usuarioInstitucionDAL = new UsuarioInstitucionDAL(conec);
        }

        private static void ValidarDatos(TarjetaRFID tarjeta)
        {
            if (tarjeta.UID == "")
            {
                throw new Exception("El UID Debe tener 8 caracteres");
            }
            if (tarjeta.FechaVecimiento.Date <= DateTime.Today)
            {
                throw new Exception("La fecha de vencimiento debe ser Distinta a la fecha de hoy");
            }
            if (tarjeta.DiasHabilitados == DiasSemana.DiaSemana.Ninguno)
            {
                throw new Exception("Debe seleccionar algun dia valido");
            }
            if (tarjeta.HoraFin <= tarjeta.HoraInicio)
            {
                throw new Exception("La hora de fin debe ser posterior a la hora de inicio ");
            }
        }

        public static string NormalizarUID(string? uid)
        {
            var limpio = new string((uid ?? "").Where(c => !char.IsWhiteSpace(c) && c != ':' && c != '-').ToArray()).ToUpperInvariant();
            return limpio.Length == 8 && limpio.All(Uri.IsHexDigit) ? limpio : "";
        }

        public UsuarioInstitucion ObtnerUsuario(int usuarioid, int empresaid)
        {
            if (_usuarioInstitucionDAL.BuscarXId(usuarioid, empresaid) is not UsuarioInstitucion usuario)
            {
                throw new Exception("El usuario no existe");
            }
            if (usuario.Estado != TipoEstado.Estados.Activo)
            {
                throw new Exception("Usuario de baja");
            }
            if (_tarjetaRFIDDAL.TarjetaEnUso(usuarioid))
            {
                throw new Exception("Ya tiene una tarjeta asociada");
            }
            return usuario;
        }

        public List<Contenedor> ContenedoresDisponibles(int institucionid, int empresaid)
        {
            return _contenedorDAL.ListasContenedorXInstitucion(institucionid, empresaid).Where(c => c.Estado == TipoEstado.Estados.Activo).ToList();
        }

        public Dictionary<int, TarjetaRFID> TarjetaXInstitucion(int institucionid, int empresaid)
        {
            return _tarjetaRFIDDAL.ListarXInstitucion(institucionid, empresaid).ToDictionary(t => t.UsuarioInstitucionId);
        }

        public void AsignarTarjeta(TarjetaRFID t, int empresaid)
        {
            var usuario = ObtnerUsuario(t.UsuarioInstitucionId, empresaid);
            t.UID = NormalizarUID(t.UID);
            ValidarDatos(t);

            t.ContenedoresId = t.ContenedoresId.Distinct().ToList();
            if (t.ContenedoresId.Count == 0)
            {
                throw new Exception("Seleccione al menos un contendor");
            }

            var permitido = ContenedoresDisponibles(usuario.InstitucionId, empresaid).Select(c => c.Id).ToHashSet();

            if(t.ContenedoresId.Any(id=>!permitido.Contains(id)))
            {
                throw new Exception("Uno de los contenedores seleccionado no es valido para esta institucion");
            }

            if(_tarjetaRFIDDAL.ExisteUID(t.UID))
            {
                throw new Exception("El UID Ya esta asignado a otra tarjeta");
            }

            t.estadoSync = TipoEstado.EstadoSync.Pendiente;
            t.TipoEstado= TipoEstado.Estados.Activo;
            t.FechaRegistro=DateTime.Now;

            _tarjetaRFIDDAL.AgregarTarjeta(t);

        }

    }
}
