using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    /// <inheritdoc />
    public partial class DBTest4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias",
                column: "ParentId",
                principalTable: "Categorias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias",
                column: "ParentId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
