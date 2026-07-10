using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forza.LastMile.TopPerformers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "repartidores_rendimiento",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_repartidor = table.Column<string>(type: "text", nullable: false),
                    tipo_vehiculo = table.Column<string>(type: "text", nullable: false),
                    satisfaccion_score = table.Column<decimal>(type: "numeric", nullable: false),
                    entregas_completadas = table.Column<int>(type: "integer", nullable: false),
                    meta_diaria_original = table.Column<int>(type: "integer", nullable: false),
                    meta_diaria_actual = table.Column<int>(type: "integer", nullable: false),
                    is_meta_ajustada = table.Column<bool>(type: "boolean", nullable: false),
                    incidencia_descripcion = table.Column<string>(type: "text", nullable: true),
                    mes_periodo = table.Column<string>(type: "text", nullable: false),
                    ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_repartidores_rendimiento", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repartidores_rendimiento");
        }
    }
}
