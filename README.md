# Thetis – Assessor Virtual de Investimentos
API **ASP.NET Core** para recomendação de investimentos com **IA Generativa (Google Gemini)**, **Entity Framework Core** + **Oracle 19c**, **AutoMapper**, **Swagger**, **manipulação de arquivos (JSON/TXT)** e **APIs externas** (Banco Central do Brasil).

> **Stack:** .NET 8, EF Core 9, Oracle.EntityFrameworkCore, AutoMapper, Swashbuckle (Swagger), Google Gemini AI, APIs REST externas.

---

## 📋 Índice

- [Integrantes](#integrantes)
- [Novidades da Sprint 4](#novidades-da-sprint-4)
- [Funcionalidades](#funcionalidades)
- [Arquitetura](#arquitetura)
- [Tecnologias e Integrações](#tecnologias-e-integrações)
- [Configuração](#configuração)
- [Banco & Migrations](#banco--migrations)
- [Executando](#executando)
- [Endpoints Principais](#endpoints-principais)
- [Exemplos de Uso](#exemplos-de-uso)
- [Documentação da IA](#documentação-da-ia)

---

## Integrantes

| Nome | RM |
|------|-----|
| Júlia Marques Mendes das Neves | RM98680 |
| Kaiky Alvaro Miranda | RM98118 |
| Lucas Rodrigues da Silva | RM98344 |
| Juan Pinheiro de França | RM552202 |
| Matheus Gusmão Aragão | RM550826 |

---

## Novidades da Sprint 4

### 1. **Integração com IA Generativa (Google Gemini)**
- ✅ Análise inteligente de carteiras de investimento
- ✅ Explicações personalizadas de conceitos financeiros
- ✅ Recomendações adaptativas baseadas em perfil
- ✅ Comparação entre perfil do investidor e carteira sugerida
- ✅ Sugestões de melhorias para portfolios existentes

### 2. **APIs Externas - Dados em Tempo Real**
- ✅ Integração com API do Banco Central do Brasil
- ✅ Consulta automática de indicadores macroeconômicos:
  - Taxa SELIC
  - IPCA (Inflação)
  - CDI
  - Cotação do Dólar (PTAX)
- ✅ Atualização automática de variáveis econômicas
- ✅ Sistema de fallback para garantir disponibilidade

### 3. **Melhorias no Algoritmo de Recomendação**
- ✅ Consideração de variáveis macroeconômicas na alocação
- ✅ Ajuste dinâmico de percentuais por perfil de risco
- ✅ Diversificação inteligente por tipo de ativo

### 4. **Novas Funcionalidades**
- ✅ Análise detalhada de diversificação de carteiras
- ✅ Relatórios macroeconômicos consolidados
- ✅ Endpoints de teste de conectividade com APIs externas

---

## Funcionalidades

* **CRUD completo**

  * Clientes
  * Ativos
  * Carteiras recomendadas (geração e análise)
  * Variáveis macroeconômicas (integradas com APIs)
* **Recomendações**

  * Geração de carteira por perfil de risco/objetivo/prazo
  * Aprovação de carteira
  * Análise de diversificação (RF/RV/Fundos)
  * Simulação de rendimento
  * Score de adequação ao perfil
* **IA Generativa (Google Gemini)**
  
  * Análise detalhada de carteiras
  * Identificação de pontos fortes e riscos
  * Recomendações personalizadas
  * Explicação de conceitos financeiros (3 níveis: básico/intermediário/avançado)
  * Comparação perfil x carteira
* **APIs Externas**
  
  * Consulta de dados macroeconômicos em tempo real
  * Atualização automática de indicadores
  * Relatórios econômicos consolidados
* **Arquivos**

  * Exportação **JSON** (snapshot dos dados)
  * Exportação **TXT** (relatório resumido)
  * Importação **JSON** (upsert de Ativos, Clientes e Variáveis)
* **Documentação**

  * Swagger UI na raiz `/`
  * Código limpo e camadas separadas(API, Service, Data, Model)

---

## Arquitetura

```
Thetis/
├── ThetisApi/           # API Web ASP.NET Core (Controllers, Swagger, DI/Startup)
│   ├── Controllers/
│   │   ├── ClientesController.cs
│   │   ├── AtivosController.cs
│   │   ├── RecomendacoesController.cs
│   │   ├── VariaveisController.cs
│   │   ├── IAController.cs        # (NOVO) Endpoints de IA
│   │   └── BackupController.cs
│   └── App_Data/        # Arquivos exportados (JSON/TXT)
│
├── ThetisService/       # Regras de negócio (Services, AutoMapper)
│   ├── Implementations/
│   │   ├── ClienteService.cs
│   │   ├── AtivoService.cs
│   │   ├── RecomendacaoService.cs
│   │   ├── VariavelMacroeconomicaService.cs
│   │   ├── GeminiService.cs              # (NOVO) Serviço de IA
│   │   └── BancoCentralService.cs        # (NOVO) API Externa
│   └── Interfaces/
│
├── ThetisData/          #  Acesso a dados (EF Core, Repository, Migrations)
│   ├── Context/
│   ├── Repositories/
│   └── Migrations/
│
├── ThetisModel/         # Contratos: Entidades, DTOs, ViewModels, Enums
│   ├── Entities/
│   ├── DTOs/
│   │   ├── ClienteDto.cs
│   │   ├── GeminiDto.cs              # (NOVO) Contratos IA
│   │   └── BancoCentralDto.cs        # (NOVO) Contratos API Externa
│   ├── ViewModels/
│   └── Enums/
│
└── Thetis.sln
```

### Camadas & responsabilidades

* **ThetisApi**: expõe endpoints REST, valida entrada e retorna respostas padronizadas. Somente orquestra — não contém regra de negócio.
* **ThetisService**: implementa **use cases** (ex.: gerar recomendação, simular rendimento, CRUD lógico). Usa AutoMapper para mapear DTO/VM ⇄ Entity.
* **ThetisData**: **EF Core** + **Oracle 19c**. `AppDbContext`, Repository genérico, Migrations, conversores (bool ⇄ CHAR(1) Y/N).
* **ThetisModel**: tipos compartilhados (Entities/DTOs/ViewModels/Enums). Sem dependência de infraestrutura.

### Diagrama de Componentes
![Diagrama de componentes](./images/diagrama_componentes(1).png)

### Diagrama de domínio
![Diagrama de domínio](./images/Diagrama.png)

---

## Tecnologias e Integrações

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - Camada de apresentação
- **Entity Framework Core 9** - ORM
- **Oracle 19c** - Banco de dados
- **AutoMapper** - Mapeamento objeto-objeto

### IA e APIs Externas
- **Google Gemini 2.5 Flash** - IA Generativa
  - Modelo: `gemini-2.5-flash`
  - Temperatura: 0.2 (respostas mais determinísticas)
  - Max tokens: 5000
- **Banco Central do Brasil API** - Dados macroeconômicos
  - SELIC (série 432)
  - IPCA (série 433)
  - CDI (série 4392)
  - Dólar PTAX (série 1)

### Documentação e Testes
- **Swagger/OpenAPI** - Documentação interativa
- **Swashbuckle** - Geração automática de docs

---

## Configuração

### 1) Connection String & API Keys

`appsettings.json` (ou Secrets):

```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=xxxxx;Password=xxxxx;Data Source=host:1521/ORCL;"
  },
 "GeminiApiKey": "SUA_CHAVE_AQUI"
}
```

⚠️ **Importante:** 
- Obtenha sua API Key do Google Gemini em: https://makersuite.google.com/app/apikey
- O plano gratuito oferece 60 requisições/minuto

### 2) Dependências

* .NET 8 SDK
* Oracle Client
* Pacotes NuGet principais:

  * `Oracle.EntityFrameworkCore`
  * `Microsoft.EntityFrameworkCore.Design`
  * `AutoMapper.Extensions.Microsoft.DependencyInjection`
  * `Swashbuckle.AspNetCore`

---

## Banco & Migrations
> Oracle 19c não possui `BOOLEAN`. O projeto aplica conversores globais para **CHAR(1)** com valores **'Y'/'N'**.

Comandos comuns:

```bash
# gerar migration
dotnet ef migrations add Initial

# aplicar migration no banco
dotnet ef database update
```

### Estrutura do Banco

**Tabelas principais:**
- `THETIS_CLIENTES` - Dados dos investidores
- `THETIS_ATIVOS` - Produtos de investimento
- `THETIS_CARTEIRAS_RECOMENDADAS` - Portfolios sugeridos
- `THETIS_ITENS_CARTEIRA` - Composição das carteiras
- `THETIS_VARIAVEIS_MACROECONOMICAS` - Indicadores econômicos
- `THETIS_LOGS_RECOMENDACAO` - Auditoria de recomendações
- `THETIS_HISTORICO_INVESTIMENTOS` - Transações realizadas
- `THETIS_AVALIACOES_CARTEIRA` - Feedback dos clientes

---

## Executando

```bash
dotnet restore
dotnet run
```

* Swagger UI disponível em: **[https://localhost:7294/](https://localhost:7294/)**
* JSON da especificação: **/swagger/v1/swagger.json**

---

## Endpoints Principais

### Clientes (`/Clientes`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/Clientes/GetAll` | Lista todos os clientes ativos |
| GET | `/Clientes/GetById/{id}` | Busca cliente por ID |
| GET | `/Clientes/GetPerfil/{id}` | Retorna perfil investidor |
| POST | `/Clientes/Create` | Cria novo cliente |
| PUT | `/Clientes/Update/{id}` | Atualiza cliente |
| DELETE | `/Clientes/Delete/{id}` | Soft delete de cliente |

### Ativos (`/Ativos`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/Ativos/GetAll` | Lista todos os ativos |
| GET | `/Ativos/GetById/{id}` | Busca ativo por ID |
| GET | `/Ativos/GetByCodigo/{codigo}` | Busca por código (ex: VALE3) |
| GET | `/Ativos/GetByTipo?tipo=1` | Filtra por tipo (1=RF, 2=RV, 3=Fundos) |
| GET | `/Ativos/GetByPerfilRisco?perfil=2` | Filtra por perfil de risco |
| POST | `/Ativos/Create` | Cadastra novo ativo |
| PUT | `/Ativos/Update/{id}` | Atualiza ativo |
| DELETE | `/Ativos/Delete/{id}` | Remove ativo |

### Recomendações (`/Recomendacoes`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/Recomendacoes/GerarRecomendacao` | Gera carteira personalizada |
| GET | `/Recomendacoes/GetByCliente/{clienteId}` | Lista carteiras do cliente |
| GET | `/Recomendacoes/GetById/{id}` | Detalhes da carteira |
| PATCH | `/Recomendacoes/AprovarCarteira/{id}?aprovada=true` | Aprova/rejeita carteira |
| GET | `/Recomendacoes/AnalisarDiversificacao/{id}` | Análise de diversificação |
| GET | `/Recomendacoes/SimularRendimento/{id}?meses=12` | Simulação de rendimento |

### Variáveis macro (`/Variaveis`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/Variaveis/GetAll` | Lista todas as variáveis |
| GET | `/Variaveis/GetByCodigo/{codigo}` | Busca por código (ex: SELIC) |
| PUT | `/Variaveis/Update/{id}` | Atualiza variável manualmente |
| POST | `/Variaveis/AtualizarAutomaticamente` | Atualiza via API BCB |
| GET | `/Variaveis/GetDadosTempoReal` | Dados consolidados em tempo real |
| GET | `/Variaveis/GetSelicTempoReal` | Consulta SELIC atual |
| GET | `/Variaveis/GetIpcaTempoReal` | Consulta IPCA atual |
| GET | `/Variaveis/GetCdiTempoReal` | Consulta CDI atual |
| GET | `/Variaveis/GetDolarTempoReal` | Consulta Dólar atual |
| GET | `/Variaveis/Relatorio` | Relatório macroeconômico completo |
| GET | `/Variaveis/TestarConexaoApiBCB` | Teste de conectividade |

### IA - Gemini (`/IA`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/IA/Perguntar` | Faz pergunta livre à IA |
| GET | `/IA/AnalisarCarteira/{carteiraId}` | Análise detalhada com IA |
| GET | `/IA/ExplicarConceito?conceito=X&nivel=basico` | Explica conceitos financeiros |
| GET | `/IA/CompararPerfilCarteira/{carteiraId}` | Compara perfil x carteira |
| GET | `/IA/SugerirMelhorias/{carteiraId}` | Sugere 5 melhorias |
| GET | `/IA/ListarConceitos` | Lista conceitos disponíveis |
| GET | `/IA/TestarConexao` | Testa conexão com Gemini |

### Backup / Arquivos (`/Backup`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/Backup/ExportJson` | Exporta todos os dados em JSON |
| GET | `/Backup/RelatorioTxt` | Gera relatório resumido TXT |
| POST | `/Backup/import-json` | Importa dados do JSON |

---

## Exemplos de uso

### Criar Cliente

```http
POST /Clientes/Create
Content-Type: application/json

{
  "nome": "Maria Silva",
  "email": "maria.silva@example.com",
  "cpf": "12345678901",
  "dataNascimento": "1995-03-12",
  "rendaMensal": 6500.00,
  "valorDisponivel": 15000.00,
  "perfilRisco": 2,
  "objetivoPrincipal": 3,
  "prazoInvestimentoMeses": 36
}
```

### Gerar Recomendação

```http
POST /Recomendacoes/GerarRecomendacao
Content-Type: application/json

{
  "clienteId": 1,
  "ativosDisponiveis": [ /* opcional */ ],
  "valorInvestimento": 10000,
  "objetivo": 3,
  "prazoMeses": 24,
  "considerarVariaveisMacroeconomicas": true
}
```

### Importar Backup (JSON no body)

```http
POST /Backup/import-json
Content-Type: application/json

{
  "Data": "2025-09-21T12:00:00Z",
  "Ativos": [
    {
      "Codigo": "CDBBANCOX",
      "Nome": "CDB Banco X",
      "Descricao": "CDB liquidez diária",
      "TipoAtivo": 1,
      "RentabilidadeEsperada": 12.5,
      "NivelRisco": 1,
      "LiquidezDias": 1,
      "ValorMinimo": 1000.00,
      "TaxaAdministracao": 0.00
    }
  ],
  "Clientes": [
    {
      "Nome": "Maria Silva",
      "Email": "maria.silva@example.com",
      "Cpf": "123.456.789-01",
      "DataNascimento": "1995-03-12",
      "RendaMensal": 6500.00,
      "ValorDisponivel": 15000.00,
      "PerfilRisco": 2,
      "ObjetivoPrincipal": 3,
      "PrazoInvestimentoMeses": 36,
      "Ativo": true
    }
  ],
  "Variaveis": [
    {
      "Nome": "Taxa Selic",
      "Codigo": "SELIC",
      "Descricao": "Taxa básica de juros",
      "ValorAtual": 10.50,
      "ValorAnterior": 10.25,
      "DataReferencia": "2025-09-01",
      "UnidadeMedida": "%",
      "FonteDados": "BACEN",
      "Tendencia": "ESTAVEL",
      "ImpactoInvestimentos": "Aumenta atratividade de renda fixa",
      "Ativa": true
    }
  ]
}
```

---

## Documentação da IA

### Como Funciona a Integração com Gemini

1. **Configuração**
   - Adicione sua API Key no `appsettings.json`
   - O serviço usa o modelo `gemini-2.5-flash`
   - Temperatura: 0.2 (respostas mais consistentes)

2. **Análise de Carteiras**
   - Envia contexto completo da carteira
   - Gemini analisa diversificação, riscos e oportunidades
   - Retorna análise estruturada em JSON

3. **Explicações Personalizadas**
   - 3 níveis de conhecimento: básico, intermediário, avançado
   - Conceitos financeiros explicados de forma didática
   - Exemplos práticos incluídos

4. **Limites e Boas Práticas**
   - Plano gratuito: 60 req/min
   - Timeout: 30 segundos por requisição
   - Fallback em caso de erro

### Conceitos Disponíveis para Explicação

**Básicos:**
- Renda Fixa, Renda Variável
- Diversificação, Liquidez
- Rentabilidade, Taxa Selic
- IPCA, CDI

**Intermediários:**
- Tesouro Direto, CDB, LCI/LCA
- Ações, Fundos de Investimento
- ETF, Perfil de Investidor

**Avançados:**
- Derivativos, Hedge
- Alocação de Ativos
- Rebalanceamento
- Análise Fundamentalista/Técnica

---

## Convenções & Notas

* **Booleans Oracle 19c:** mapeados para `CHAR(1)` com conversores globais `'Y'/'N'`.
* **Soft delete:** alguns registros usam flags (`Ativo`, `AtivoSistema`) em vez de DELETE.
* **Únicos:** índices em CPF, Email, Código de Ativo, Código de Variável.
* **Case-insensitive:** buscas de código normalizam para maiúsculas e email para minúsculas.
* **Swagger**: sempre ativo.
