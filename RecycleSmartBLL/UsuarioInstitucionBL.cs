using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class UsuarioInstitucionBL
    {
        private readonly UsuarioInstitucionDAL _usuarioInstitucionDAL;
        private readonly InstitucionClienteDAL _institucionClienteDAL;

        public UsuarioInstitucionBL(string conectar)
        {
            _institucionClienteDAL=new InstitucionClienteDAL(conectar);
            _usuarioInstitucionDAL=new UsuarioInstitucionDAL(conectar);
        }

        public UsuarioInstitucion?BuscarxId(int id,int empresaid)
        {
            return _usuarioInstitucionDAL.BuscarXId(id,empresaid);
        }

        public List<UsuarioInstitucion>ListarxInstitucion(int institucionid,int empresaid)
        {
            return _usuarioInstitucionDAL.ListarXInstitucion(institucionid,empresaid);
        }

        private void ObtenerInstitucion(int institucionid,int empresaid)
        {
            if(_institucionClienteDAL.BuscarXId(institucionid,empresaid) is not InstitucionCliente institucion)
            {
                throw new Exception("No existe institucion");
            }
            if (institucion.Estados != TipoEstado.Estados.Activo)
            {
                throw new Exception("Solo se pueden Registtrar usuarios a Instituciones activas");
            }
        }
        private static void Validar(UsuarioInstitucion ui)
        {
            if(string.IsNullOrWhiteSpace(ui.Nombre)||string.IsNullOrWhiteSpace(ui.Apellido))
            {
                throw new Exception("Nombre y Apellido Campos obligatorios");
            }

            if(ui.DNI.Length<7||ui.DNI.Length>8)
            {
                throw new Exception("El DNI debe tener 8 digitos");
            }
        }
        private static string NormalizarDni(string? dni)
        {
            var limpio = (dni ?? "").Replace(".", "").Replace(" ", "").Trim();
            return limpio.All(char.IsDigit) ? limpio : "";
        }

        public void AgregarUsuarioInstitucion(UsuarioInstitucion ui,int empresaid)
        {
            ObtenerInstitucion(ui.InstitucionId, empresaid);

            ui.Nombre = (ui.Nombre ?? "").Trim();
            ui.Apellido = (ui.Apellido ?? "").Trim();
            ui.Rol = (ui.Rol ?? "").Trim();
            ui.Sector = string.IsNullOrWhiteSpace(ui.Sector) ? null : ui.Sector.Trim();
            ui.DNI = NormalizarDni(ui.DNI);

            Validar(ui);

            if (_usuarioInstitucionDAL.ExisteDNI(ui.DNI, ui.InstitucionId))
            {
                throw new Exception("Ya Existe DNI");
            }
            ui.FechaRegistro=DateTime.Now;
            ui.Estado = TipoEstado.Estados.Activo;

            _usuarioInstitucionDAL.AltaUsuarioInstitucion(ui);
        }
    }
}
