using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class UsuarioEmpresaLogisticaBL
    {
        private readonly UsuarioEmpresaLogisticaDAL _usuarioEmpresaLogisticaDAL;

        public UsuarioEmpresaLogisticaBL(string conectar)
        {
            _usuarioEmpresaLogisticaDAL = new UsuarioEmpresaLogisticaDAL(conectar);
        }

        public List<UsuarioEmpresaLogistica> ListarUsuarioxEmpresa(int empresaId)
        {
            return _usuarioEmpresaLogisticaDAL.ListarInstitucionXEmpresa(empresaId);
        }

        public UsuarioEmpresaLogistica? BuscarUsuarioxId(int id, int empresid)
        {
            return _usuarioEmpresaLogisticaDAL.BuscarUsuarioEmpresaXId(id, empresid);
        }

        private void ValidarCampos(UsuarioEmpresaLogistica ue)
        {
            ue.Nombre = ue.Nombre?.Trim() ?? "";
            ue.Apellido = ue.Apellido?.Trim() ?? "";
            ue.Correo = ue.Correo?.Trim() ?? "";
            if (ue.Correo == "") throw new Exception("Campo Obligatorio");
            if (ue.Nombre == "" || ue.Apellido == "")
            {
                throw new Exception("Campos Obligatorios");
            }
            if (_usuarioEmpresaLogisticaDAL.ExisteCorreo(ue.Correo, ue.Id))
            {
                throw new Exception("Ya Existe ese Correo en la base");
            }

        }

        public int AgregarUsuarioEmpresa(UsuarioEmpresaLogistica ue, string? clave)
        {
            ValidarCampos(ue);
            if (string.IsNullOrEmpty(clave)) throw new Exception("La Clave es Obligatoria");
            ue.Clave = Servicio.Encriptacion.Hash(clave!);
            return _usuarioEmpresaLogisticaDAL.AgregarUsuarioEmpresa(ue);
        }

        private UsuarioEmpresaLogistica ObtenerNoDadodeBaja(int id, int empresaid)
        {
            if (_usuarioEmpresaLogisticaDAL.BuscarUsuarioEmpresaXId(id, empresaid) is not UsuarioEmpresaLogistica usuarioempresa)
            {
                throw new Exception("El usuario no existe");
            }
            if (usuarioempresa.Estados == TipoEstado.Estados.Baja) { throw new Exception("El usuario se encuentra de baja y no se puede realizar cambios"); }
            return usuarioempresa;
        }

        public void ModificarUsuarioEmpresa(UsuarioEmpresaLogistica ue, int actualId)
        {
            var actual = ObtenerNoDadodeBaja(ue.Id, actualId);

            if (ue.Id == actualId && ue.Rol != actual.Rol) throw new Exception("No se Puede Cambiar su propio rol");

            if (actual.Rol == "Administrador" && ue.Rol != "Administrador" && _usuarioEmpresaLogisticaDAL.AdministradoresActivos(ue.EmpresaLogisticaId, ue.Id) == 0)
            {
                throw new Exception("La Empresa debe tener al menos un administrador");
            }
            ValidarCampos(ue);
            _usuarioEmpresaLogisticaDAL.ModificarUsuarioEmpresa(ue);
        }

        public void CambiarClave(int id, int empresaid, string? nuevaclave)
        {
            ObtenerNoDadodeBaja(id, empresaid);
            if (string.IsNullOrEmpty(nuevaclave)) throw new Exception("La Clave es Obligatoria");
            _usuarioEmpresaLogisticaDAL.CambiarClave(id, empresaid, Servicio.Encriptacion.Hash(nuevaclave));
        }

        public void CambiarEstado(int id, int empresaId, TipoEstado.Estados nuevo, int idActual)
        {
            var actual = ObtenerNoDadodeBaja(id, empresaId);

            if (id == idActual)
                throw new Exception("No puede cambiar el estado de su propio usuario.");

            if (actual.Estados == nuevo)
                throw new Exception($"El usuario ya está en estado {nuevo}.");

            // Suspender o dar de baja al último administrador activo dejaría la empresa sin acceso
            if (actual.Rol == "Administrado" && nuevo != TipoEstado.Estados.Activo
                && _usuarioEmpresaLogisticaDAL.AdministradoresActivos(empresaId, id) == 0)
                throw new Exception("No se puede: es el único administrador activo de la empresa.");
        }
    }
}
