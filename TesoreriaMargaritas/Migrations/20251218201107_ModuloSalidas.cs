using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TesoreriaMargaritas.Migrations
{
    /// <inheritdoc />
    public partial class ModuloSalidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Cajas_CajaId",
                table: "Gastos");

            migrationBuilder.DropTable(
                name: "Arqueos");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_CajaId",
                table: "Gastos");

            migrationBuilder.DeleteData(
                table: "SecuenciasPrefijos",
                keyColumn: "Prefijo",
                keyValue: "GAP");

            migrationBuilder.DeleteData(
                table: "SecuenciasPrefijos",
                keyColumn: "Prefijo",
                keyValue: "NMP");

            migrationBuilder.DeleteData(
                table: "SecuenciasPrefijos",
                keyColumn: "Prefijo",
                keyValue: "SMP");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "SecuenciasPrefijos");

            migrationBuilder.DropColumn(
                name: "CajaId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "IdentificacionBeneficiario",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "UrlSoporte",
                table: "Gastos");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Gastos",
                newName: "Concepto");

            migrationBuilder.RenameColumn(
                name: "FechaHora",
                table: "Gastos",
                newName: "Fecha");

            migrationBuilder.AlterColumn<string>(
                name: "Prefijo",
                table: "Gastos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Gastos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PROVEEDORES",
                columns: table => new
                {
                    CODPROVEEDOR = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NOMPROVEEDOR = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROVEEDORES", x => x.CODPROVEEDOR);
                });

            migrationBuilder.CreateTable(
                name: "VENDEDORES",
                columns: table => new
                {
                    CODVENDEDOR = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NOMVENDEDOR = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VENDEDORES", x => x.CODVENDEDOR);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PROVEEDORES");

            migrationBuilder.DropTable(
                name: "VENDEDORES");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "Gastos",
                newName: "FechaHora");

            migrationBuilder.RenameColumn(
                name: "Concepto",
                table: "Gastos",
                newName: "Tipo");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "SecuenciasPrefijos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Prefijo",
                table: "Gastos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<int>(
                name: "CajaId",
                table: "Gastos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdentificacionBeneficiario",
                table: "Gastos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlSoporte",
                table: "Gastos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Arqueos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CajaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConteoDinero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descuadre = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaArqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaldoFinalDia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotEntradas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotEntradasAnu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotSalidas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotSalidasAnu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalConteoDinero = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arqueos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Arqueos_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Arqueos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "NumeroDocumento",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cajas",
                columns: new[] { "Id", "Estado", "Nombre", "SaldoActual" },
                values: new object[] { 1, "Abierta", "Funza Principal", 0m });

            migrationBuilder.InsertData(
                table: "SecuenciasPrefijos",
                columns: new[] { "Prefijo", "Descripcion", "UltimoConsecutivo" },
                values: new object[,]
                {
                    { "GAP", "Gastos Generales", 0 },
                    { "NMP", "Nóminas", 0 },
                    { "SMP", "Simplificados", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_CajaId",
                table: "Gastos",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Arqueos_CajaId",
                table: "Arqueos",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Arqueos_UsuarioId",
                table: "Arqueos",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Cajas_CajaId",
                table: "Gastos",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
