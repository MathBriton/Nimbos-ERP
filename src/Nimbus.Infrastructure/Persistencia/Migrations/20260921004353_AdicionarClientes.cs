using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nimbus.Infrastructure.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Documento = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    TipoDePessoa = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    EnderecoCep = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    EnderecoLogradouro = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnderecoNumero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EnderecoComplemento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnderecoBairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnderecoCidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnderecoUf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CriadoPor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AtualizadoPor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Excluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Documento",
                table: "Clientes",
                column: "Documento",
                unique: true,
                filter: "[Excluido] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nome",
                table: "Clientes",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
