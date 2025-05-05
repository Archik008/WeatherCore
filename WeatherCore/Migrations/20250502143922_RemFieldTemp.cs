using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherCore.Migrations
{
    /// <inheritdoc />
    public partial class RemFieldTemp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayWeather_Cities_CityId",
                table: "DayWeather");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DayWeather",
                table: "DayWeather");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "DayWeather");

            migrationBuilder.RenameTable(
                name: "DayWeather",
                newName: "Days");

            migrationBuilder.RenameIndex(
                name: "IX_DayWeather_CityId",
                table: "Days",
                newName: "IX_Days_CityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Days",
                table: "Days",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Cities_CityId",
                table: "Days",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Cities_CityId",
                table: "Days");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Days",
                table: "Days");

            migrationBuilder.RenameTable(
                name: "Days",
                newName: "DayWeather");

            migrationBuilder.RenameIndex(
                name: "IX_Days_CityId",
                table: "DayWeather",
                newName: "IX_DayWeather_CityId");

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "DayWeather",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DayWeather",
                table: "DayWeather",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayWeather_Cities_CityId",
                table: "DayWeather",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
