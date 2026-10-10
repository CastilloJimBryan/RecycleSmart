namespace RecycleSmart.EtiquetasColor
{
    public static  class FormatoEtiqueta
    {
        public static string CUIT(string? cuit) => cuit is { Length: 11 } ? $"{cuit[..2]}-{cuit[2..10]}-{cuit[10]}" : cuit ?? "";
    }
}
