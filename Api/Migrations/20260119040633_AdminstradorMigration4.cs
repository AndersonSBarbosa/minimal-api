using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minimal_api.Migrations
{
    /// <inheritdoc />
    public partial class AdminstradorMigration4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Caminho",
                table: "Administradores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Caminho",
                table: "Administradores");
        }
    }
}
