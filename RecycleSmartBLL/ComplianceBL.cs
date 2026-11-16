using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class ComplianceBL
    {
        private readonly LoteBL _lotebl;
        public ComplianceBL(string conectar)
        {
            _lotebl = new LoteBL(conectar);
        }
        public static TipoEstado.EstadoPlazo Clasificar(int porcentaje, bool vencido)
        {
            if (vencido) return TipoEstado.EstadoPlazo.Vencido;
            if (porcentaje >= 90) return TipoEstado.EstadoPlazo.PorVencer;
            if (porcentaje >= 70) return TipoEstado.EstadoPlazo.Atencion;

            return TipoEstado.EstadoPlazo.EnPlazo;
        }
        public List<ComplianceLote> ListadoLotesAbiertos(int empresaid, int institucionid)
        {
            var lista = new List<ComplianceLote>();
            foreach (var item in _lotebl.ListarLote(empresaid, institucionid, false))
            {
                var (etapa, limite, porcentaje, vencido) = LoteBL.PlazoActual(item, conTope: false);
                if (limite == null) continue;
                lista.Add(new ComplianceLote
                {
                    LoteId = item.Id,
                    Codigo = item.Codigo,
                    InstitucionId = item.InstitucionId,
                    InstitucionNombre = item.InstitucionNombre,
                    ContenedorNombre = item.ContenedorNombre,
                    EstadoLote = item.EstadoLote,
                    EtapaControlada = etapa,
                    FechaLimite = limite.Value,
                    PorcentajePlazo = porcentaje,
                    EstadoPlazo = Clasificar(porcentaje, vencido)
                });
            }
            return lista.OrderByDescending(c => c.PorcentajePlazo).ToList();
        }

        public List<LoteNoConforme> ListadoNoConforme(int empresaid, List<Lote> cerrado)
        {
            var lista = new List<LoteNoConforme>();
            foreach (var item in cerrado.Where(x => x.Conforme == false))
            {
                var lote = _lotebl.ObtenerLote(item.Id, empresaid);
                var hito = lote.Hitos.FirstOrDefault(h => h.DentroDelPlazo == false);
                if (hito == null) continue;
                DateTime? limite = hito.TipoHito switch
                {
                    TipoEstado.TipoHito.Recoleccion => lote.FechaLimiteRecoleccion,
                    TipoEstado.TipoHito.Tratamiento => lote.FechaLimiteTratamiento,
                    _ => null
                };

                lista.Add(new LoteNoConforme
                {
                    LoteId = lote.Id,
                    Codigo = lote.Codigo,
                    InstitucionNombre = item.InstitucionNombre,
                    HitoVencido = hito.TipoHito == TipoEstado.TipoHito.Recoleccion ? "Recolección" : "Tratamiento",
                    DiasExcedidos = limite == null ? 0 : Math.Max(1, (int)Math.Ceiling((hito.Fecha - limite.Value).TotalDays))
                });
            }
            return lista;

        }

        public List<Lote> CerradosDelPeriodo(int empresaId, int institucionId, int dias)
        {
            var cerrados = _lotebl.ListarLote(empresaId, institucionId, true);
            if (dias <= 0) return cerrados;
            var desde = DateTime.Now.AddDays(-dias);
            return cerrados.Where(l => l.FechaCierre >= desde).ToList();
        }

        public List<string> InstitucionesEnRiesgo(List<Lote> cerrados)
        {
            return cerrados
                .GroupBy(l => l.InstitucionNombre)
                .Where(g => g.Count(l => l.Conforme == false) * 100 > g.Count() * 20)
                .Select(g => g.Key)
                .ToList();
        }

    }
}
