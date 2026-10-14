using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class DispositivoIoTBL
    {
        private readonly DispositivoIoTDAL _dispositivoIoTDAL;
        private readonly ContenedorDAL _contenedorDAL;

        public DispositivoIoTBL(string conectar)
        {
            _dispositivoIoTDAL=new DispositivoIoTDAL(conectar);   
            _contenedorDAL=new ContenedorDAL(conectar);
        }


        public Dictionary<int, DispositivoIoT> DispositivosXInstitucion(int institucionid, int empresaid)
        {
            return _dispositivoIoTDAL.ListarDispositivoXInstitucion(institucionid, empresaid).ToDictionary(d => d.ContenedorId);
        }
        public DispositivoIoT? BuscarXContenedor(int contenedorid,int empresaid)
        {
           return _dispositivoIoTDAL.BuscarXContenedor(contenedorid, empresaid);
        }
        public static string NormalizarMac(string? mac)
        {
            var limpio = new string((mac ?? "").Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
            if (limpio.Length != 12)
            {
                throw new Exception("La MAC debe tener 12 caracteres hexadecimales (0-9, A-F). Ej: AA:BB:CC:DD:EE:FF");
            }
            return string.Join(":", Enumerable.Range(0, 6).Select(i => limpio.Substring(i * 2, 2)));
        }
        public int AgregarDispositivo(int contenedorid,string? mac,string? firmware,int empresaid)
        {
            if (_contenedorDAL.BuscarContenedorXId(contenedorid, empresaid) is not Contenedor contenedor)
                throw new Exception("No Existe Contenedor"); 
            if(contenedor.Estado!=TipoEstado.Estados.Activo)
            {
                throw new Exception("Solo se puede asignar a contenedores Activos");
            }
            if(_dispositivoIoTDAL.BuscarXContenedor(contenedorid,empresaid)!=null)
            {
                throw new Exception("Contenedor ya asignado a un dispositivo IoT");
            }

            var macNormalizada = NormalizarMac(mac);

            if(_dispositivoIoTDAL.ExisteMAC(macNormalizada))
            {
                throw new Exception("MacAddress ya asignado a un Contenedor");

            }

            var dispositivo = new DispositivoIoT
            {
                ContenedorId = contenedorid,
                MacAdress = macNormalizada,
                Mqtt = $"RecycleSmart/{macNormalizada}",
                EstadoDispositivo = TipoEstado.EstadoDispositivo.Pendiente,
                Firmware = string.IsNullOrWhiteSpace(firmware) ? null : firmware.Trim()
            };

            return _dispositivoIoTDAL.AgregarDispositivoIoT(dispositivo);
        }

        public void DarBaja(int id, int empresaid)
        {
            if (_dispositivoIoTDAL.CambiarEstado(TipoEstado.EstadoDispositivo.Baja.ToString(), id, empresaid)==0)
            {
                throw new Exception("No se pudo dar de baja");
            }
        }
    }
}
