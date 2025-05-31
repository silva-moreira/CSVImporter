# Importador de Contas a Pagar e Receber (CPR)

Este projeto em C# tem como objetivo importar dados financeiros de um arquivo CSV e inseri-los automaticamente em um banco de dados SQL Server. Ele trata especificamente do cadastro e lançamento de **Contas a Pagar e Receber**, garantindo que os fornecedores estejam previamente cadastrados.

## Funcionalidades

- Leitura de arquivo CSV com informações financeiras.
- Tratamento e limpeza dos dados para garantir formatação correta.
- Cadastro automático de fornecedores e pessoas na base de dados, caso ainda não existam.
- Inserção dos dados importados na tabela `ContasPagarReceber`.
- Suporte a diferentes tipos de pagamento (`Pix`, `Boleto`, etc.).
- Conversão e padronização de valores monetários.

## Tecnologias Utilizadas

- [.NET 8.0](https://dotnet.microsoft.com/)
- [CsvHelper](https://joshclose.github.io/CsvHelper/) - Leitura e conversão de arquivos CSV.
- SQL Server - Banco de dados para armazenar os registros.

## Como Usar

1. **Clone o repositório**:
   ```bash
   git clone https://github.com/silva-moreira/seu-repositorio.git
   
2. **Configure a string de conexão:**
   No início do arquivo Program.cs, substitua:
   ```bash   
   static string connectionString = @"";

4. **Ajuste o caminho do CSV**:
   Verifique e altere o caminho do arquivo CSV conforme sua máquina:
   ```bash
      var caminhoArquivo = @"C:\joaov\Clientes\CPR.csv";

## Autor 
João Silva – @silva-moreira
