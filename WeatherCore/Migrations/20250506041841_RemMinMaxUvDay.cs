using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherCore.Migrations
{
    /// <inheritdoc />
    public partial class RemMinMaxUvDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Uv_max",
                table: "Days");

            migrationBuilder.RenameColumn(
                name: "Uv_min",
                table: "Days",
                newName: "Uv");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Uv",
                table: "Days",
                newName: "Uv_min");

            migrationBuilder.AddColumn<double>(
                name: "Uv_max",
                table: "Days",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
