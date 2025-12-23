using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    /// <inheritdoc />
    public partial class CierreCajaLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArqueoId",
                table: "Gastos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArqueoId",
                table: "Entradas",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArqueoId",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "ArqueoId",
                table: "Entradas");
        }
    }
}
