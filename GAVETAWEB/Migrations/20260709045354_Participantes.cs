using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAVETAWEB.Migrations
{
    /// <inheritdoc />
    public partial class Participantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParticipanteGrupo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipanteGrupo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipanteGrupo_Grupo_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParticipanteGrupo_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipanteGrupo_GrupoId",
                table: "ParticipanteGrupo",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipanteGrupo_UsuarioId",
                table: "ParticipanteGrupo",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipanteGrupo");
        }
    }
}
