using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TesoreriaMargaritas.Migrations
{
    public partial class CierreCaja : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear Tabla Cajas (Si no existe)
            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                });

            // Insertar Caja por defecto
            migrationBuilder.InsertData(
                table: "Cajas",
                columns: new[] { "Id", "Estado", "Nombre", "SaldoActual" },
                values: new object[] { 1, "Abierta", "Caja Principal", 0m });

            // 2. Crear Tabla Arqueos (Esta es la que falta y causa el error en la app)
            migrationBuilder.CreateTable(
                name: "Arqueos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaArqueo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotEntradas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotEntradasAnu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotSalidas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotSalidasAnu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConteoDinero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalConteoDinero = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descuadre = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoFinalDia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CajaId = table.Column<int>(type: "int", nullable: false)
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

            // 3. CONECTAR Relaciones (Indices y FK)
            // NOTA: Eliminamos AddColumn porque la base de datos YA TIENE las columnas ArqueoId.
            // Solo aseguramos que tengan los índices y las llaves foráneas.

            // Indices
            migrationBuilder.CreateIndex(
                name: "IX_Arqueos_CajaId",
                table: "Arqueos",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Arqueos_UsuarioId",
                table: "Arqueos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Entradas_ArqueoId",
                table: "Entradas",
                column: "ArqueoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_ArqueoId",
                table: "Gastos",
                column: "ArqueoId");

            // Foreign Keys (Conectamos la columna existente con la nueva tabla Arqueos)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Entradas_Arqueos_ArqueoId", table: "Entradas");
            migrationBuilder.DropForeignKey(name: "FK_Gastos_Arqueos_ArqueoId", table: "Gastos");
            migrationBuilder.DropTable(name: "Arqueos");
            migrationBuilder.DropTable(name: "Cajas");
            migrationBuilder.DropIndex(name: "IX_Entradas_ArqueoId", table: "Entradas");
            migrationBuilder.DropIndex(name: "IX_Gastos_ArqueoId", table: "Gastos");
            // No borramos las columnas ArqueoId en Down para evitar pérdida de datos accidental
            // si se revierte esta migración específica, ya que fueron creadas por otra migración.
        }
    }
}