using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class UsuarioInstitucionDAL
    {
        private readonly string _conectar;
        public UsuarioInstitucionDAL(string conectar)
        {
            _conectar=conectar;
        }

        public UsuarioInstitucion?BuscarXId(int id,int empresaid)
        {

        }
        public List<UsuarioInstitucion> ListarXInstitucion(int institucionid,int empresaid)
        {

        }

        public bool ExisteDNI(string DNI,int institucionid)
        {

        }

        public int AltaUsuarioInstitucion(UsuarioInstitucion ui)
        {

        }
    }
}
