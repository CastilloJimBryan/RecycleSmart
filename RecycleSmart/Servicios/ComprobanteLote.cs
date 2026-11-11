using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RecycleSmartBE;

namespace RecycleSmart.Servicios
{
    public static class ComprobanteLote
    {
        public static byte[] Generar(Lote lote, string empresaNombre, string mensajeIntegridad)
        {
            bool conforme = lote.Conforme == true;
            string color = conforme ? Colors.Green.Darken2 : Colors.Red.Darken2;
            string fondo = conforme ? Colors.Green.Lighten5 : Colors.Red.Lighten5;

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(t => t.FontSize(10));

                    // ===== Encabezado =====
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("RecycleSmart").FontSize(18).Bold().FontColor(Colors.Green.Darken3);
                                c.Item().Text("Comprobante de trazabilidad de residuos patogénicos").FontSize(11);
                                c.Item().Text($"Empresa logística: {empresaNombre}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                            row.AutoItem().Column(c =>
                            {
                                c.Item().AlignRight().Text("Lote").FontSize(9).FontColor(Colors.Grey.Darken1);
                                c.Item().AlignRight().Text(lote.Codigo).FontSize(14).Bold();
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // ===== Contenido =====
                    page.Content().PaddingVertical(12).Column(col =>
                    {
                        col.Spacing(14);

                        // Resultado
                        col.Item().Background(fondo).Border(1).BorderColor(color).Padding(10).Column(c =>
                        {
                            c.Item().Text(conforme ? "LOTE CONFORME" : "LOTE NO CONFORME")
                                    .FontSize(16).Bold().FontColor(color);
                            c.Item().Text(conforme
                                    ? "Todas las etapas se cumplieron dentro de los plazos de la Ley 24.051."
                                    : "Al menos una etapa se cumplió fuera del plazo legal.").FontSize(9);
                        });

                        // Datos del lote
                        col.Item().Text("Datos del lote").FontSize(12).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(95); c.RelativeColumn();
                                c.ConstantColumn(95); c.RelativeColumn();
                            });
                            Dato(t, "Institución", lote.InstitucionNombre);
                            Dato(t, "Contenedor", lote.ContenedorNombre);
                            Dato(t, "Tipo de residuo", lote.TipoResiduo);
                            Dato(t, "Volumen total", $"{lote.VolumenTotal:0.##} L");
                            Dato(t, "Depósitos", lote.CantidadDepositada.ToString());
                            Dato(t, "Generación", lote.FechaGeneracion.ToString("dd/MM/yyyy HH:mm"));
                            Dato(t, "Cierre", lote.FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "—");
                            Dato(t, "Hitos", lote.Hitos.Count.ToString());
                        });

                        // Cadena de custodia
                        col.Item().Text("Cadena de custodia").FontSize(12).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);    // Etapa
                                c.ConstantColumn(100);   // Fecha
                                c.RelativeColumn(2);    // Responsable
                                c.RelativeColumn(2);    // Documento
                                c.ConstantColumn(50);   // En plazo
                            });

                            t.Header(h =>
                            {
                                h.Cell().Element(CeldaTitulo).Text("Etapa").Bold();
                                h.Cell().Element(CeldaTitulo).Text("Fecha").Bold();
                                h.Cell().Element(CeldaTitulo).Text("Responsable").Bold();
                                h.Cell().Element(CeldaTitulo).Text("Documento").Bold();
                                h.Cell().Element(CeldaTitulo).AlignCenter().Text("En plazo").Bold();
                            });

                            foreach (var h in lote.Hitos)
                            {
                                string etapa = NombreEtapa(h.TipoHito)
                                             + (string.IsNullOrEmpty(h.Metodo) ? "" : $" ({NombreMetodo(h.Metodo)})");
                                string plazo = h.DentroDelPlazo switch { true => "Sí", false => "No", _ => "—" };
                                string colorPlazo = h.DentroDelPlazo == false ? Colors.Red.Darken2 : Colors.Black;

                                t.Cell().Element(Celda).Text(etapa);
                                t.Cell().Element(Celda).Text(h.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                t.Cell().Element(Celda).Text(h.Responsable ?? h.UsuarioNombre ?? "—");
                                t.Cell().Element(Celda).Text(h.NumeroDocumento ?? "—");
                                t.Cell().Element(Celda).AlignCenter().Text(plazo).FontColor(colorPlazo);
                            }
                        });

                        // Sello de integridad
                        col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(c =>
                        {
                            c.Spacing(4);
                            c.Item().Text("Sello de integridad").Bold();
                            c.Item().Text(lote.HashFinal ?? "—").FontSize(8);
                            c.Item().Text(mensajeIntegridad).FontSize(9).FontColor(Colors.Green.Darken2);
                            c.Item().Text("Cada etapa está encadenada criptográficamente con la anterior: cualquier modificación posterior de los registros invalida este sello.")
                                    .FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // ===== Pie =====
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Emitido el {DateTime.Now:dd/MM/yyyy HH:mm} · Verificable en RecycleSmart con el código del lote")
                                          .FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.AutoItem().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
                            t.Span("Página ");
                            t.CurrentPageNumber();
                            t.Span(" de ");
                            t.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();
        }

        // ----- Auxiliares -----
        private static void Dato(TableDescriptor t, string etiqueta, string valor)
        {
            t.Cell().PaddingVertical(3).Text(etiqueta).FontColor(Colors.Grey.Darken1);
            t.Cell().PaddingVertical(3).Text(valor).SemiBold();
        }

        private static IContainer CeldaTitulo(IContainer c) =>
            c.Background(Colors.Grey.Lighten3).Padding(4);

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);

        private static string NombreEtapa(TipoEstado.TipoHito t) => t switch
        {
            TipoEstado.TipoHito.Generacion => "Generación",
            TipoEstado.TipoHito.SolicitudRecoleccion => "Solicitud de recolección",
            TipoEstado.TipoHito.Recoleccion => "Recolección",
            TipoEstado.TipoHito.Tratamiento => "Tratamiento",
            TipoEstado.TipoHito.DisposicionFinal => "Disposición final",
            _ => t.ToString()
        };
        private static string NombreMetodo(string metodo) => metodo switch
        {
            "Incineracion" => "Incineración",
            "Autoclave" => "Autoclave",
            "Microondas" => "Microondas",
            _ => metodo
        };
    }
}