using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    public partial class AddAnuladoEntradas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "Entradas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "Entradas");
        }
    }
}