using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Servicio
{
    public static class Roles
    {
        public const string Administrador= "Administrador";
        public const string Operario= "Operario";
        public const string Responsable= "Responsable";
        public const string Auditor= "Auditor";

        public static readonly string[] Todos = {Administrador, Operario, Responsable,Auditor};

        public const string Gestion = Administrador + "," + Responsable;
        public const string Operacion = Administrador + "," + Responsable + "," + Operario;
        public const string Control = Administrador + "," + Responsable + "," + Auditor;


        
            // Igual que [Authorize(Roles = "A,B")]: ¿tiene alguno de estos roles?
            public static bool TieneRol(this ClaimsPrincipal usuario, string roles)
            {
                return roles.Split(',').Any(r => usuario.IsInRole(r.Trim()));
            }
        
    }
}
