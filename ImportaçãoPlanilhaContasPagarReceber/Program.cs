using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using CsvHelper.TypeConversion;
using Microsoft.Win32;


namespace Program
{
    class Program
    {
        static string connectionString = @"";

        static void Main()
        {
            var caminhoArquivo = @"C:\joaov\Clientes\Santin\CPR.csv";

            var linhasBrutas = File.ReadAllLines(caminhoArquivo);

            var linhasCorrigidas = linhasBrutas.Select(l =>
            {
                var lTrim = l.Trim();

                // Remove aspas externas que abraçam toda a linha, se existirem
                if (lTrim.StartsWith("\"") && lTrim.EndsWith("\""))
                {
                    lTrim = lTrim.Substring(1, lTrim.Length - 2);
                }

                // Substitui aspas duplas internas por aspas simples
                lTrim = lTrim.Replace("\"\"", "\"");

                return lTrim;
            }).ToList();

            var textoCorrigido = string.Join(Environment.NewLine, linhasCorrigidas);

            using var reader = new StreamReader(caminhoArquivo);
            using var csv = new CsvReader(reader, new CsvConfiguration(new CultureInfo("pt-BR"))
            {
                Delimiter = ";",
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null
            });

            var registros = csv.GetRecords<RegistroFinanceiro>().ToList();

            Console.WriteLine($"Total registros lidos: {registros.Count}");

            using var conn = new SqlConnection(connectionString);
            conn.Open();

            if ((registros?.Count ?? 0) == 0)
                Console.WriteLine("Não encontramos nada no arquivo");

            foreach (var r in registros.OrderBy(x=> x.DataVencimento).ToList())
            {
                int codPessoa = ObterOuInserirPessoaEFornecedor(conn, r.CNPJCPF, r.Fornecedor, Convert.ToInt32(r.CodEmpresa), Convert.ToInt32(r.Evento));
                if (codPessoa == 0)
                {
                    codPessoa = 1679; // Código padrão para "sem fornecedor"
                }

                #region Script 
                string insertSql = string.Empty;
                insertSql += " INSERT INTO ContasPagarReceber ( ";
                insertSql += "             CodEmpresa, ";
                insertSql += "             TipoConta,           ";
                insertSql += "             CodPessoa,           ";
                insertSql += "             CodCentroCusto,      ";
                insertSql += "             CodEvento,           ";
                insertSql += "             CodSetor,            ";
                insertSql += "             DtaCadastro,         ";
                insertSql += "             DtaVencimento,       ";
                insertSql += "             DtaPagamento,        ";
                insertSql += "             ValorTitulo,         ";
                insertSql += "             ValorPago,           ";
                insertSql += "             NumeroNotaFiscal,    ";
                insertSql += "             Documento,           ";
                insertSql += "             Parcela,             ";
                insertSql += "             QtdeParcela,         ";
                insertSql += "             TipoPgto,            ";
                insertSql += "             SequenciaBaixa,      ";
                insertSql += "             TipoLanc,            ";
                insertSql += "             ValorDesconto,       ";
                insertSql += "             ValorJuros,          ";
                insertSql += "             ValorTituloOriginal, ";
                insertSql += "             CodFuncionario,      ";
                insertSql += "             Status               ";
                insertSql += "    ) VALUES ( ";
                insertSql += "             @CodEmpresa, ";
                insertSql += "             @TipoConta, ";
                insertSql += "             @CodPessoa, ";
                insertSql += "             @CodCentroCusto, ";
                insertSql += "             @CodEvento, ";
                insertSql += "             @CodSetor, ";
                insertSql += "             @DtaCadastro, ";
                insertSql += "             @DtaVencimento, ";
                insertSql += "             @DtaPagamento, ";
                insertSql += "             @ValorTitulo, ";
                insertSql += "             @ValorPago, ";
                insertSql += "             @NumeroNotaFiscal, ";
                insertSql += "             @Documento, ";
                insertSql += "             @Parcela, ";
                insertSql += "             @QtdeParcela, ";
                insertSql += "             @TipoPgto, ";
                insertSql += "             @SequenciaBaixa, ";
                insertSql += "             @TipoLanc, ";
                insertSql += "             @ValorDesconto, ";
                insertSql += "             @ValorJuros, ";
                insertSql += "             @ValorTituloOriginal, ";
                insertSql += "             @CodFuncionario,";
                insertSql += "             @Status )";
                #endregion

                using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@CodEmpresa", Convert.ToInt32(r.CodEmpresa));
                cmd.Parameters.AddWithValue("@TipoConta", Convert.ToInt32(r.TipoConta));
                cmd.Parameters.AddWithValue("@CodPessoa", codPessoa);
                cmd.Parameters.AddWithValue("@CodCentroCusto", Convert.ToInt32(r.CentroDeCusto));
                cmd.Parameters.AddWithValue("@CodEvento", Convert.ToInt32(r.Evento));
                cmd.Parameters.AddWithValue("@CodSetor", Convert.ToInt32(r.Setor));
                cmd.Parameters.AddWithValue("@DtaCadastro", r.DataCadastro);
                cmd.Parameters.AddWithValue("@DtaVencimento", r.DataVencimento);
                cmd.Parameters.AddWithValue("@ValorTitulo", r.ValorTitulo);
                cmd.Parameters.AddWithValue("@ValorTituloOriginal", r.ValorTitulo);

                if (!string.IsNullOrEmpty(r?.DataPagamento?.ToString() ?? ""))
                {
                    cmd.Parameters.AddWithValue("@DtaPagamento", (object?)r.DataPagamento ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ValorPago", r.ValorPago);
                    cmd.Parameters.AddWithValue("@SequenciaBaixa", 0);
                    cmd.Parameters.AddWithValue("@Status", 3);
                } else
                {
                    cmd.Parameters.AddWithValue("@DtaPagamento", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ValorPago", 0);
                    cmd.Parameters.AddWithValue("@SequenciaBaixa", 0);
                    cmd.Parameters.AddWithValue("@Status", 2);
                }

                cmd.Parameters.AddWithValue("@NumeroNotaFiscal", r.NumeroNota);
                cmd.Parameters.AddWithValue("@Documento", "MIGRACAO"); //r.NumeroDocumento);
                cmd.Parameters.AddWithValue("@Parcela", !string.IsNullOrEmpty(r.Parcela) ? Convert.ToInt32(r.Parcela) : 1);
                cmd.Parameters.AddWithValue("@QtdeParcela", !string.IsNullOrEmpty(r.QtdeParcela) ? Convert.ToInt32(r.QtdeParcela) : 1);
                cmd.Parameters.AddWithValue("@TipoPgto", ConverterTipoPagamento(r.TipoPagamento));

                cmd.Parameters.AddWithValue("@TipoLanc", 1); //1 - Pagar 2 - Receber
                cmd.Parameters.AddWithValue("@ValorDesconto", 0m);
                cmd.Parameters.AddWithValue("@ValorJuros", 0m);

                cmd.Parameters.AddWithValue("@CodFuncionario", 1); // CodFuncionario, se necessário, pode ser adicionado aqui

                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Importação concluída com sucesso!");
            Console.ReadKey();
        }


        static int ObterOuInserirPessoaEFornecedor(SqlConnection conn, string cpfCnpj, string nome, int codEmpresa, int codEvento)
        {
            var cpfCnpjLimpo = cpfCnpj?.Replace("–", "")
                                      .Replace("-", "")
                                      .Replace(".", "")
                                      .Replace("/", "")
                                      .Trim();

            // Aqui já formatamos o CPF/CNPJ para o padrão correto
            var cpfCnpjFormatado = FormatarCpfCnpj(cpfCnpjLimpo);

            var nomeTrimado = nome?.Trim();

            // Se não tem nome OU CPF/CNPJ válido, retorna 0 (indica "sem fornecedor")
            if (string.IsNullOrWhiteSpace(nomeTrimado) || string.IsNullOrWhiteSpace(cpfCnpjFormatado))
            {
                return 0;
            }

            // Usa o cpfCnpjFormatado na consulta e na inserção
            using var checkCmd = new SqlCommand("SELECT CodPessoa FROM Pessoas WHERE CNPJCPF = @cpfCnpj", conn);
            checkCmd.Parameters.AddWithValue("@cpfCnpj", cpfCnpjFormatado);

            var result = checkCmd.ExecuteScalar();
            int codPessoa;

            if (result != null)
            {
                codPessoa = Convert.ToInt32(result);
            }
            else
            {
                using var insertCmd = new SqlCommand(@"
            INSERT INTO Pessoas 
            (Nome, CNPJCPF, InscEstRG, Telefone, Celular, Email, DtFunNasc, Contato, 
             Observacoes, Fantasia, InscMun, TipoPessoa)
            OUTPUT INSERTED.CodPessoa
            VALUES 
            (@nome, @cpfCnpj, '', '', '', '', NULL, '', '', '', '', 2)", conn);

                insertCmd.Parameters.AddWithValue("@nome", nomeTrimado);
                insertCmd.Parameters.AddWithValue("@cpfCnpj", cpfCnpjFormatado);

                codPessoa = (int)insertCmd.ExecuteScalar();

                // Insere na tabela de fornecedores, usando também cpfCnpjFormatado
                using var insertFornecedorCmd = new SqlCommand(@"
            INSERT INTO PFT_FORNECEDOR
            (COD_FORNECEDOR, DESC_FORNECEDOR, CGC, DHMULATU, USUARIO, STATUS, FLAG_CUSTOMEDIO, Email, AtualizaEstoque, StatusDesativado, DtaCadastro, CodPessoa, CodGrupoFornecedor, CodEmpresa, CodEvento)
            VALUES
            (@codFornecedor, @descFornecedor, @cgc, @dhmulatu, @usuario, @status, @flagCustomedio, @Email, @atualizaEstoque, @statusDesativado, @dtaCadastro, @codPessoa, @codGrupoFornecedor, @codEmpresa, @codEvento)", conn);

                insertFornecedorCmd.Parameters.AddWithValue("@codFornecedor", codPessoa.ToString());
                insertFornecedorCmd.Parameters.AddWithValue("@descFornecedor", nomeTrimado);
                insertFornecedorCmd.Parameters.AddWithValue("@cgc", cpfCnpjFormatado);
                insertFornecedorCmd.Parameters.AddWithValue("@dhmulatu", DateTime.Now);
                insertFornecedorCmd.Parameters.AddWithValue("@usuario", Environment.UserName);
                insertFornecedorCmd.Parameters.AddWithValue("@status", 'A');
                insertFornecedorCmd.Parameters.AddWithValue("@flagCustomedio", 'N');
                insertFornecedorCmd.Parameters.AddWithValue("@Email", DBNull.Value);
                insertFornecedorCmd.Parameters.AddWithValue("@atualizaEstoque", false);
                insertFornecedorCmd.Parameters.AddWithValue("@statusDesativado", false);
                insertFornecedorCmd.Parameters.AddWithValue("@dtaCadastro", DateTime.Now);
                insertFornecedorCmd.Parameters.AddWithValue("@codPessoa", codPessoa);
                insertFornecedorCmd.Parameters.AddWithValue("@codGrupoFornecedor", DBNull.Value); // ou um valor válido
                insertFornecedorCmd.Parameters.AddWithValue("@codEmpresa", codEmpresa);
                insertFornecedorCmd.Parameters.AddWithValue("@codEvento", codEvento);

                insertFornecedorCmd.ExecuteNonQuery();
            }

            return codPessoa;
        }




        static int ConverterTipoPagamento(string tipo)
        {
            if (int.TryParse(tipo, out int result))
                return result;

            return (tipo?.Trim() ?? "").ToLower() switch
            {
                "pix" => 3,
                "boleto" => 4,
                _ => 3 // qualquer valor inválido vira PIX
            };
        }

        public static string FormatarCpfCnpj(string cpfCnpj)
        {
            if (string.IsNullOrWhiteSpace(cpfCnpj))
                return string.Empty;

            var numeros = new string(cpfCnpj.Where(char.IsDigit).ToArray());

            if (numeros.Length == 11) // CPF
                return Convert.ToUInt64(numeros).ToString(@"000\.000\.000\-00");

            if (numeros.Length == 14) // CNPJ
                return Convert.ToUInt64(numeros).ToString(@"00\.000\.000\/0000\-00");

            return numeros;
        }

    }


    public class ConverterValor : DecimalConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0m;

            // Remove "R$", espaços e transforma ponto em separador decimal, se necessário
            var valorLimpo = text
                .Replace("R$", "")
                .Replace(".", "")
                .Replace(",", ".")
                .Replace("-", "") // <- REMOVE O SINAL NEGATIVO
                .Trim();

            if (decimal.TryParse(valorLimpo, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0m;
        }
    }


}

