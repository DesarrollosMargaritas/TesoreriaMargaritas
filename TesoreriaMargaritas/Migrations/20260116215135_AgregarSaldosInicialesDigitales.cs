using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSaldosInicialesDigitales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Arqueos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoInicialDaviplata",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoInicialNequi",
                table: "Arqueos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoInicialDaviplata",
                table: "Arqueos");

            migrationBuilder.DropColumn(
                name: "SaldoInicialNequi",
                table: "Arqueos");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Arqueos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
