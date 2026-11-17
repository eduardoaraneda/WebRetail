namespace RetailPOS.Application;

public sealed class DocumentoCuadratura
{
    public int Tipo { get; set; }
    public string Nombre { get; set; }="";
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}
public sealed class IngresoCuadratura
{
    public string Medio { get; set; }="";
    public int Operaciones { get; set; }
    public decimal Ventas { get; set; }
    public decimal CobrosDebito { get; set; }
    public decimal Reembolsos { get; set; }
    public decimal Neto => Ventas+CobrosDebito-Reembolsos;
}
public sealed class ResumenCuadratura
{
    public DateOnly Fecha { get; set; }
    public string Alcance { get; set; }="";
    public List<DocumentoCuadratura> Documentos { get; set; }=[];
    public List<IngresoCuadratura> Ingresos { get; set; }=[];
    public int VentasCantidad { get; set; }
    public decimal VentasBrutas { get; set; }
    public int NotasCreditoCantidad { get; set; }
    public decimal NotasCredito { get; set; }
    public decimal CobrosDebito { get; set; }
    public decimal SaldoCambios { get; set; }
    public decimal TotalVentasNeto => VentasBrutas-NotasCredito;
    public decimal TotalSistema => TotalVentasNeto+CobrosDebito;
    public decimal TotalPagos => Ingresos.Sum(x=>x.Neto);
    public decimal DiferenciaRegistro => TotalPagos-TotalSistema;
    public decimal Fondo { get; set; }
    public decimal IngresosAdicionales { get; set; }
    public decimal Retiros { get; set; }
    public decimal? DeclaradoBruto { get; set; }
    public decimal? DeclaradoNeto => DeclaradoBruto-Fondo-IngresosAdicionales+Retiros;
    public decimal? Diferencia => DeclaradoNeto-TotalSistema;
    public int CajasAbiertas { get; set; }
    public bool DiferenciasPorMedio { get; set; }
    public string Estado => DeclaradoBruto is null ? "Pendiente de declaración" : DiferenciaRegistro!=0 ? "Revisar documentos y pagos" : Diferencia<0 ? "Faltante" : Diferencia>0 ? "Sobrante" : DiferenciasPorMedio ? "Total coincide; revisar medios" : "Cuadrada";
}
