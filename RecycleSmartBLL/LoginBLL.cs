using RecycleSmartBE;
using RecycleSmartDAL;
using Servicio;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class LoginBLL
    {
        private readonly UsuarioRecycleSmartDAL _usuarioRecycleSmart;
        private readonly UsuarioEmpresaLogisticaDAL _usuarioEmpresaLogistica;

        public LoginBLL(string conectar)
        {
            _usuarioEmpresaLogistica=new UsuarioEmpresaLogisticaDAL(conectar);
            _usuarioRecycleSmart= new UsuarioRecycleSmartDAL(conectar);
        }

        public Login? Autenticar(string correo,string clave)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            {
                return null;
            }

            correo= correo.Trim();

            var admin = _usuarioRecycleSmart.BuscarXCorreo(correo);
            if(admin!=null && Encriptacion.Verificar(clave, admin.Clave))
            {
                VerificarEstado(admin.Estados);
                return new Login { usuarioRecycleSmart = admin };
            }
            var usuario = _usuarioEmpresaLogistica.BuscarXCorreo(correo);
            if(usuario!=null && Encriptacion.Verificar(clave,usuario.Clave))
            {
                VerificarEstado(usuario.Estados);
                return new Login { usuarioEmpresaLogistica = usuario };
            }

            return null;
        }

        private void VerificarEstado(TipoEstado.Estados estado)
        {
            if(estado!=TipoEstado.Estados.Activo)
            {
                throw new Exception("Cuenta Suspendida");
            }
        }
    }
}
