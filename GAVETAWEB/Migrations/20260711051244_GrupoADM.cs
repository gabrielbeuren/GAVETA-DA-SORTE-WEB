using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAVETAWEB.Migrations
{
    /// <inheritdoc />
    public partial class GrupoADM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdministradorId",
                table: "Grupo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdministradorId",
                table: "Grupo");
        }
    }
}
