using GestaoLoja.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250212000000_AddCategoriaParentId")]
    public partial class AddCategoriaParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Categorias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_ParentId",
                table: "Categorias",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias",
                column: "ParentId",
                principalTable: "Categorias",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categorias_Categorias_ParentId",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_ParentId",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Categorias");
        }
    }
}
