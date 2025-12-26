using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IpWatcher.Infrastructure.Storage.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IpHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IpText = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedUtc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IpHistory_ChangedUtc",
                table: "IpHistory",
                column: "ChangedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IpHistory");
        }
    }
}
