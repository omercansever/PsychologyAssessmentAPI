using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PsychologyAssessmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class InıtialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(453), "Kaygı ve anksiyete ile ilgili sorular", "Anksiyete" },
                    { 2, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(1614), "Depresif belirtiler ile ilgili sorular", "Depresyon" },
                    { 3, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(1617), "Stres yönetimi ile ilgili sorular", "Stres" },
                    { 4, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(1618), "Uyku kalitesi ile ilgili sorular", "Uyku" },
                    { 5, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(1619), "Sosyal etkileşimler ile ilgili sorular", "Sosyal İlişkiler" }
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "IsActive", "Text", "Weight" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(7907), true, "Günlük yaşamda endişelendiğim durumlar sıklıkla aklımdan çıkmaz.", 3 },
                    { 2, 1, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8962), true, "Gelecek hakkında aşırı kaygı duyarım.", 2 },
                    { 3, 1, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8965), true, "Önemli olaylar öncesinde fiziksel rahatsızlık (kalp çarpıntısı, terleme) yaşarım.", 3 },
                    { 4, 2, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8966), true, "Çoğu zaman kendimi üzgün ve umutsuz hissederim.", 3 },
                    { 5, 2, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8966), true, "Eskiden hoşlandığım aktivitelere artık ilgi duymuyorum.", 3 },
                    { 6, 2, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8967), true, "Günlük görevleri yapmakta zorlanırım.", 2 },
                    { 7, 3, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8968), true, "İş/okul yükü nedeniyle sürekli baskı altında hissederim.", 2 },
                    { 8, 3, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8969), true, "Stresli durumlarda sakin kalmakta zorlanırım.", 2 },
                    { 9, 4, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8970), true, "Gece rahat uyuyabiliyorum.", 2 },
                    { 10, 4, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8971), true, "Sabah dinlenmiş olarak uyanırım.", 2 },
                    { 11, 5, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8975), true, "İnsanlarla rahatça iletişim kurabiliyorum.", 1 },
                    { 12, 5, new DateTime(2025, 8, 19, 8, 35, 14, 986, DateTimeKind.Utc).AddTicks(8976), true, "Sosyal ortamlarda kendimi rahat hissederim.", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CategoryId",
                table: "Questions",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
