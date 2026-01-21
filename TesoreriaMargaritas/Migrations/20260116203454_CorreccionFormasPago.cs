using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionFormasPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Gastos_ArqueoId",
                table: "Gastos",
                column: "ArqueoId");

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_ArqueoId",
                table: "Entradas",
                column: "ArqueoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Arqueos_ArqueoId",
                table: "Entradas",
                column: "ArqueoId",
                principalTable: "Arqueos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gastos_Arqueos_ArqueoId",
                table: "Gastos",
                column: "ArqueoId",
                principalTable: "Arqueos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Arqueos_ArqueoId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Gastos_Arqueos_ArqueoId",
                table: "Gastos");

            migrationBuilder.DropIndex(
                name: "IX_Gastos_ArqueoId",
                table: "Gastos");

            migrationBuilder.DropIndex(
                name: "IX_Entradas_ArqueoId",
                table: "Entradas");
        }
    }
}
