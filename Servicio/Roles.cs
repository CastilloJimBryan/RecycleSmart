using System;
using System.Collections.Generic;
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
    }
}
