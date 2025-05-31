using CsvHelper.Configuration.Attributes;
using Program;

public class RegistroFinanceiro
{
    


    [Name("Empresa")]
    public int CodEmpresa { get; set; }

    [Name("DR")]
    public string TipoConta { get; set; }

    [Name("CPF_CNPJ")]
    public string CNPJCPF { get; set; }

    [Name("Fonecedor")] // importante manter o espaço!
    public string Fornecedor { get; set; }

    [Name("CCusto")]
    public int CentroDeCusto { get; set; }

    [Name("Evento")]
    public int Evento { get; set; }

    [Name("Setor")]
    public string Setor { get; set; }

    [Name("DataCadastro")]
    public DateTime? DataCadastro { get; set; }

    [Name("DataVencimento")]
    public DateTime? DataVencimento { get; set; }

    [Name("DataPagamento")]
    public DateTime? DataPagamento { get; set; }

    [Name("ValorTitulo")]
    [TypeConverter(typeof(ConverterValor))]
    public decimal ValorTitulo { get; set; }

    [Name("ValorPago")]
    [TypeConverter(typeof(ConverterValor))]
    public decimal ValorPago { get; set; }

    [Name("NroNota")]
    public string NumeroNota { get; set; }

    [Name("NroDoc")]
    public string NumeroDocumento { get; set; }

    [Name("PC")]
    public string Parcela { get; set; }

    [Name("QtdePc")]
    public string QtdeParcela { get; set; }

    [Name("TipoPagamento")]
    public string TipoPagamento { get; set; }

}
