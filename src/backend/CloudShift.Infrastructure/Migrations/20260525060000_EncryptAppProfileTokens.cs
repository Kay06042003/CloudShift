using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudShift.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260525060000_EncryptAppProfileTokens")]
    public partial class EncryptAppProfileTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccessToken",
                table: "AppProfiles",
                newName: "EncryptedAccessToken");

            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "AppProfiles",
                newName: "EncryptedRefreshToken");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EncryptedAccessToken",
                table: "AppProfiles",
                newName: "AccessToken");

            migrationBuilder.RenameColumn(
                name: "EncryptedRefreshToken",
                table: "AppProfiles",
                newName: "RefreshToken");
        }
    }
}
