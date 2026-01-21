using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFormasPagoMovimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arqueos_Cajas_CajaId",
                table: "Arqueos");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropIndex(
                name: "IX_Arqueos_CajaId",
                table: "Arqueos");

            migrationBuilder.RenameColumn(
                name: "TotalConteoDinero",
                table: "Arqueos",
                newName: "SistTotalNequi");

            migrationBuilder.RenameColumn(
                name: "TotSalidasAnu",
                table: "Arqueos",
                newName: "SistTotalEfectivo");

            migrationBuilder.RenameColumn(
                name: "TotSalidas",
                table: "Arqueos",
                newName: "SistTotalDaviplata");

            migrationBuilder.RenameColumn(
                name: "TotEntradasAnu",
                table: "Arqueos",
                newName: "SistSalidasNequi");

            migrationBuilder.RenameColumn(
                name: "TotEntradas",
                table: "Arqueos",
                newName: "SistSalidasEfectivo");

            migrationBuilder.RenameColumn(
                name: "SaldoInicial",
                table: "Arqueos",
                newName: "SistSalidasDaviplata");

            migrationBuilder.RenameColumn(
                name: "SaldoFinalDia",
                table: "Arqueos",
                newName: "SistEntradasNequi");

            migrationBuilder.RenameColumn(
                name: "Descuadre",
                table: "Arqueos",
                newName: "SistEntradasEfectivo");

            migrationBuilder.RenameColumn(
                name: "ConteoDinero",
                table: "Arqueos",
                newName: "Observaciones");

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                table: "Gastos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormaPago",
                table: "Entradas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCaja",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConteoBilletesJson",
                table: "Arqueos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DescuadreDaviplata",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuadreEfectivo",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuadreNequi",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Arqueos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "FisicoEfectivo",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportadoDaviplata",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportadoNequi",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoInicialEfectivo",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SistEntradasDaviplata",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPago",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "FormaPago",
                table: "Entradas");

            migrationBuilder.DropColumn(
                name: "BaseCaja",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "ConteoBilletesJson",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "DescuadreDaviplata",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "DescuadreEfectivo",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "DescuadreNequi",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "FisicoEfectivo",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "ReportadoDaviplata",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "ReportadoNequi",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "SaldoInicialEfectivo",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "SistEntradasDaviplata",
                table: "Arqueos");

            migrationBuilder.RenameColumn(
                name: "SistTotalNequi",
                table: "Arqueos",
                newName: "TotalConteoDinero");

            migrationBuilder.RenameColumn(
                name: "SistTotalEfectivo",
                table: "Arqueos",
                newName: "TotSalidasAnu");

            migrationBuilder.RenameColumn(
                name: "SistTotalDaviplata",
                table: "Arqueos",
                newName: "TotSalidas");

            migrationBuilder.RenameColumn(
                name: "SistSalidasNequi",
                table: "Arqueos",
                newName: "TotEntradasAnu");

            migrationBuilder.RenameColumn(
                name: "SistSalidasEfectivo",
                table: "Arqueos",
                newName: "TotEntradas");

            migrationBuilder.RenameColumn(
                name: "SistSalidasDaviplata",
                table: "Arqueos",
                newName: "SaldoInicial");

            migrationBuilder.RenameColumn(
                name: "SistEntradasNequi",
                table: "Arqueos",
                newName: "SaldoFinalDia");

            migrationBuilder.RenameColumn(
                name: "SistEntradasEfectivo",
                table: "Arqueos",
                newName: "Descuadre");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "Arqueos",
                newName: "ConteoDinero");

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

            migrationBuilder.CreateIndex(
                name: "IX_Arqueos_CajaId",
                table: "Arqueos",
                column: "CajaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arqueos_Cajas_CajaId",
                table: "Arqueos",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
