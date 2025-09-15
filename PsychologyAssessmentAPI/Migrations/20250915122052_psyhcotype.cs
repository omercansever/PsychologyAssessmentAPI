using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsychologyAssessmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class psyhcotype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfessionalType",
                table: "Psychologists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Psychologists",
                keyColumn: "Id",
                keyValue: 1,
                column: "ProfessionalType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Psychologists",
                keyColumn: "Id",
                keyValue: 2,
                column: "ProfessionalType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Psychologists",
                keyColumn: "Id",
                keyValue: 3,
                column: "ProfessionalType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionalType",
                table: "Psychologists");
        }
    }
}
