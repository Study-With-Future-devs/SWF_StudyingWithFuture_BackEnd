using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Studying_With_Future.Migrations
{
    /// <inheritdoc />
    public partial class SyncAlunoTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlunoTurma_Turmas_TurmaId",
                table: "AlunoTurma");

            migrationBuilder.DropForeignKey(
                name: "FK_AlunoTurma_Usuarios_AlunoId",
                table: "AlunoTurma");

            migrationBuilder.CreateTable(
                name: "AlunoTurmas",
                columns: table => new
                {
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    TurmaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunoTurmas", x => new { x.AlunoId, x.TurmaId });
                    table.ForeignKey(
                        name: "FK_AlunoTurmas_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlunoTurmas_Usuarios_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoTurmas_TurmaId",
                table: "AlunoTurmas",
                column: "TurmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoTurma_Turmas_TurmaId",
                table: "AlunoTurma",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoTurma_Usuarios_AlunoId",
                table: "AlunoTurma",
                column: "AlunoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlunoTurma_Turmas_TurmaId",
                table: "AlunoTurma");

            migrationBuilder.DropForeignKey(
                name: "FK_AlunoTurma_Usuarios_AlunoId",
                table: "AlunoTurma");

            migrationBuilder.DropTable(
                name: "AlunoTurmas");

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoTurma_Turmas_TurmaId",
                table: "AlunoTurma",
                column: "TurmaId",
                principalTable: "Turmas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlunoTurma_Usuarios_AlunoId",
                table: "AlunoTurma",
                column: "AlunoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
