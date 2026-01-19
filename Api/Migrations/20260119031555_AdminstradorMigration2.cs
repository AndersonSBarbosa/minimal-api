using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minimal_api.Migrations
{
    /// <inheritdoc />
    public partial class AdminstradorMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Administradores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "SenhaFake",
                table: "Administradores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenhaFake",
                table: "Administradores");

            migrationBuilder.InsertData(
                table: "Administradores",
                columns: new[] { "Id", "Email", "Perfil", "Senha" },
                values: new object[] { 1, "adm@teste.com", "Admin", "123456" });
        }
    }
}
