using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThetisData.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "THETIS_ATIVOS",
                columns: table => new
                {
                    ID_ATIVO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODIGO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    NOME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    TIPO_ATIVO = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=RendaFixa, 2=RendaVariavel, 3=Fundos, 4=CriptoMoedas, 5=Commodities"),
                    RENTABILIDADE_ESPERADA = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    NIVEL_RISCO = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=Conservador, 2=Moderado, 3=Agressivo"),
                    LIQUIDEZ_DIAS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VALOR_MINIMO = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    TAXA_ADMINISTRACAO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    ATIVO_SISTEMA = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DATA_CRIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_ATIVOS", x => x.ID_ATIVO);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_CLIENTES",
                columns: table => new
                {
                    ID_CLIENTE = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    CPF = table.Column<string>(type: "NCHAR(11)", fixedLength: true, maxLength: 11, nullable: false),
                    DATA_NASCIMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RENDA_MENSAL = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    VALOR_DISPONIVEL = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    PERFIL_RISCO = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=Conservador, 2=Moderado, 3=Agressivo"),
                    OBJETIVO_PRINCIPAL = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=CurtoPrazo, 2=MedioPrazo, 3=LongoPrazo, 4=Aposentadoria, 5=ReservaEmergencia"),
                    PRAZO_INVESTIMENTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DATA_CADASTRO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATIVO = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_CLIENTES", x => x.ID_CLIENTE);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_VARIAVEIS_MACROECONOMICAS",
                columns: table => new
                {
                    ID_VARIAVEL = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CODIGO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    VALOR_ATUAL = table.Column<decimal>(type: "NUMBER(10,4)", nullable: false),
                    VALOR_ANTERIOR = table.Column<decimal>(type: "NUMBER(10,4)", nullable: true),
                    DATA_ATUALIZACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_REFERENCIA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UNIDADE_MEDIDA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    FONTE_DADOS = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    TENDENCIA = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    IMPACTO_INVESTIMENTOS = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ATIVA = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_VARIAVEIS_MACROECONOMICAS", x => x.ID_VARIAVEL);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_CARTEIRAS_RECOMENDADAS",
                columns: table => new
                {
                    ID_CARTEIRA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_CLIENTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOME_CARTEIRA = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    VALOR_TOTAL = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    RENTABILIDADE_ESPERADA = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    NIVEL_RISCO = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=Conservador, 2=Moderado, 3=Agressivo"),
                    OBJETIVO = table.Column<int>(type: "NUMBER(10)", nullable: false, comment: "1=CurtoPrazo, 2=MedioPrazo, 3=LongoPrazo, 4=Aposentadoria, 5=ReservaEmergencia"),
                    PRAZO_MESES = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EXPLICACAO = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                    SCORE_ADEQUACAO = table.Column<decimal>(type: "NUMBER(3,2)", nullable: false),
                    DATA_GERACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ATIVA = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    APROVADA_CLIENTE = table.Column<bool>(type: "NUMBER(1)", nullable: true),
                    DATA_APROVACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_CARTEIRAS_RECOMENDADAS", x => x.ID_CARTEIRA);
                    table.ForeignKey(
                        name: "FK_CARTEIRA_CLIENTE",
                        column: x => x.ID_CLIENTE,
                        principalTable: "THETIS_CLIENTES",
                        principalColumn: "ID_CLIENTE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TB_ATIVO_VARIAVEL_MACRO",
                columns: table => new
                {
                    ID_ATIVO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_VARIAVEL = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_ATIVO_VARIAVEL_MACRO", x => new { x.ID_ATIVO, x.ID_VARIAVEL });
                    table.ForeignKey(
                        name: "FK_ATIVO_VAR_ATIVO",
                        column: x => x.ID_ATIVO,
                        principalTable: "THETIS_ATIVOS",
                        principalColumn: "ID_ATIVO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ATIVO_VAR_VARIAVEL",
                        column: x => x.ID_VARIAVEL,
                        principalTable: "THETIS_VARIAVEIS_MACROECONOMICAS",
                        principalColumn: "ID_VARIAVEL",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_AVALIACOES_CARTEIRA",
                columns: table => new
                {
                    ID_AVALIACAO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_CARTEIRA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_CLIENTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOTA_GERAL = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOTA_ADEQUACAO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NOTA_EXPLICACAO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    COMENTARIOS = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    IMPLEMENTOU_RECOMENDACAO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PERCENTUAL_IMPLEMENTADO = table.Column<decimal>(type: "NUMBER(5,2)", nullable: true),
                    MOTIVO_NAO_IMPLEMENTACAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    DATA_AVALIACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RECOMENDARIA = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_AVALIACOES_CARTEIRA", x => x.ID_AVALIACAO);
                    table.ForeignKey(
                        name: "FK_AVALIACAO_CARTEIRA",
                        column: x => x.ID_CARTEIRA,
                        principalTable: "THETIS_CARTEIRAS_RECOMENDADAS",
                        principalColumn: "ID_CARTEIRA",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AVALIACAO_CLIENTE",
                        column: x => x.ID_CLIENTE,
                        principalTable: "THETIS_CLIENTES",
                        principalColumn: "ID_CLIENTE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_HISTORICO_INVESTIMENTOS",
                columns: table => new
                {
                    ID_HISTORICO = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_CLIENTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_ATIVO = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TIPO_OPERACAO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    VALOR_OPERACAO = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    QUANTIDADE = table.Column<decimal>(type: "NUMBER(15,6)", nullable: true),
                    PRECO_UNITARIO = table.Column<decimal>(type: "NUMBER(15,6)", nullable: true),
                    DATA_OPERACAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    RENTABILIDADE_OBTIDA = table.Column<decimal>(type: "NUMBER(5,2)", nullable: true),
                    OBSERVACOES = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ORIGEM_RECOMENDACAO = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TAXA_CORRETAGEM = table.Column<decimal>(type: "NUMBER(15,2)", nullable: true),
                    IMPOSTO_RENDA = table.Column<decimal>(type: "NUMBER(15,2)", nullable: true),
                    DATA_VENCIMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_HISTORICO_INVESTIMENTOS", x => x.ID_HISTORICO);
                    table.ForeignKey(
                        name: "FK_HISTORICO_ATIVO",
                        column: x => x.ID_ATIVO,
                        principalTable: "THETIS_ATIVOS",
                        principalColumn: "ID_ATIVO",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HISTORICO_CARTEIRA",
                        column: x => x.ORIGEM_RECOMENDACAO,
                        principalTable: "THETIS_CARTEIRAS_RECOMENDADAS",
                        principalColumn: "ID_CARTEIRA",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HISTORICO_CLIENTE",
                        column: x => x.ID_CLIENTE,
                        principalTable: "THETIS_CLIENTES",
                        principalColumn: "ID_CLIENTE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_ITENS_CARTEIRA",
                columns: table => new
                {
                    ID_ITEM = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_CARTEIRA = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_ATIVO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PERCENTUAL_CARTEIRA = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    VALOR_INVESTIMENTO = table.Column<decimal>(type: "NUMBER(15,2)", nullable: false),
                    QUANTIDADE = table.Column<decimal>(type: "NUMBER(15,6)", nullable: true),
                    RENTABILIDADE_ESPERADA = table.Column<decimal>(type: "NUMBER(5,2)", nullable: false),
                    JUSTIFICATIVA = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    PRIORIDADE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DATA_INCLUIDO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_ITENS_CARTEIRA", x => x.ID_ITEM);
                    table.ForeignKey(
                        name: "FK_ITEM_ATIVO",
                        column: x => x.ID_ATIVO,
                        principalTable: "THETIS_ATIVOS",
                        principalColumn: "ID_ATIVO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ITEM_CARTEIRA",
                        column: x => x.ID_CARTEIRA,
                        principalTable: "THETIS_CARTEIRAS_RECOMENDADAS",
                        principalColumn: "ID_CARTEIRA",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "THETIS_LOGS_RECOMENDACAO",
                columns: table => new
                {
                    ID_LOG = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ID_CLIENTE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    ID_CARTEIRA = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    PARAMETROS_ENTRADA = table.Column<string>(type: "CLOB", nullable: false),
                    RESULTADO_ALGORITMO = table.Column<string>(type: "CLOB", nullable: false),
                    TEMPO_PROCESSAMENTO = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    VERSAO_ALGORITMO = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DATA_PROCESSAMENTO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    IP_ORIGEM = table.Column<string>(type: "NVARCHAR2(45)", maxLength: 45, nullable: false),
                    USER_AGENT = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    SUCESSO = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    ERRO_MENSAGEM = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THETIS_LOGS_RECOMENDACAO", x => x.ID_LOG);
                    table.ForeignKey(
                        name: "FK_LOG_CARTEIRA",
                        column: x => x.ID_CARTEIRA,
                        principalTable: "THETIS_CARTEIRAS_RECOMENDADAS",
                        principalColumn: "ID_CARTEIRA",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LOG_CLIENTE",
                        column: x => x.ID_CLIENTE,
                        principalTable: "THETIS_CLIENTES",
                        principalColumn: "ID_CLIENTE",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_ATIVO_VARIAVEL_MACRO_ID_VARIAVEL",
                table: "TB_ATIVO_VARIAVEL_MACRO",
                column: "ID_VARIAVEL");

            migrationBuilder.CreateIndex(
                name: "IX_ATIVO_CODIGO",
                table: "THETIS_ATIVOS",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_AVALIACOES_CARTEIRA_ID_CARTEIRA",
                table: "THETIS_AVALIACOES_CARTEIRA",
                column: "ID_CARTEIRA");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_AVALIACOES_CARTEIRA_ID_CLIENTE",
                table: "THETIS_AVALIACOES_CARTEIRA",
                column: "ID_CLIENTE");

            migrationBuilder.CreateIndex(
                name: "IX_CARTEIRA_ATIVA",
                table: "THETIS_CARTEIRAS_RECOMENDADAS",
                column: "ATIVA");

            migrationBuilder.CreateIndex(
                name: "IX_CARTEIRA_CLIENTE_DATA",
                table: "THETIS_CARTEIRAS_RECOMENDADAS",
                columns: new[] { "ID_CLIENTE", "DATA_GERACAO" });

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_CPF",
                table: "THETIS_CLIENTES",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLIENTE_EMAIL",
                table: "THETIS_CLIENTES",
                column: "EMAIL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_CLIENTE_DATA",
                table: "THETIS_HISTORICO_INVESTIMENTOS",
                columns: new[] { "ID_CLIENTE", "DATA_OPERACAO" });

            migrationBuilder.CreateIndex(
                name: "IX_HISTORICO_TIPO",
                table: "THETIS_HISTORICO_INVESTIMENTOS",
                column: "TIPO_OPERACAO");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_HISTORICO_INVESTIMENTOS_ID_ATIVO",
                table: "THETIS_HISTORICO_INVESTIMENTOS",
                column: "ID_ATIVO");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_HISTORICO_INVESTIMENTOS_ORIGEM_RECOMENDACAO",
                table: "THETIS_HISTORICO_INVESTIMENTOS",
                column: "ORIGEM_RECOMENDACAO");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_ITENS_CARTEIRA_ID_ATIVO",
                table: "THETIS_ITENS_CARTEIRA",
                column: "ID_ATIVO");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_ITENS_CARTEIRA_ID_CARTEIRA",
                table: "THETIS_ITENS_CARTEIRA",
                column: "ID_CARTEIRA");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_DATA",
                table: "THETIS_LOGS_RECOMENDACAO",
                column: "DATA_PROCESSAMENTO");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_LOGS_RECOMENDACAO_ID_CARTEIRA",
                table: "THETIS_LOGS_RECOMENDACAO",
                column: "ID_CARTEIRA");

            migrationBuilder.CreateIndex(
                name: "IX_THETIS_LOGS_RECOMENDACAO_ID_CLIENTE",
                table: "THETIS_LOGS_RECOMENDACAO",
                column: "ID_CLIENTE");

            migrationBuilder.CreateIndex(
                name: "IX_VARIAVEL_CODIGO",
                table: "THETIS_VARIAVEIS_MACROECONOMICAS",
                column: "CODIGO",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VARIAVEL_DATA",
                table: "THETIS_VARIAVEIS_MACROECONOMICAS",
                column: "DATA_ATUALIZACAO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_ATIVO_VARIAVEL_MACRO");

            migrationBuilder.DropTable(
                name: "THETIS_AVALIACOES_CARTEIRA");

            migrationBuilder.DropTable(
                name: "THETIS_HISTORICO_INVESTIMENTOS");

            migrationBuilder.DropTable(
                name: "THETIS_ITENS_CARTEIRA");

            migrationBuilder.DropTable(
                name: "THETIS_LOGS_RECOMENDACAO");

            migrationBuilder.DropTable(
                name: "THETIS_VARIAVEIS_MACROECONOMICAS");

            migrationBuilder.DropTable(
                name: "THETIS_ATIVOS");

            migrationBuilder.DropTable(
                name: "THETIS_CARTEIRAS_RECOMENDADAS");

            migrationBuilder.DropTable(
                name: "THETIS_CLIENTES");
        }
    }
}
