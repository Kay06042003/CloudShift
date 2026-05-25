using System;
using CloudShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudShift.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260525070000_AddOAuthProviderApps")]
    public partial class AddOAuthProviderApps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalAccountId",
                table: "AppProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GrantedScopes",
                table: "AppProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderAppId",
                table: "AppProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OAuthProviderApps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthProviderApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthProviderApps_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProfile_ProviderAppId",
                table: "AppProfiles",
                column: "ProviderAppId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthProviderApp_UserId_Provider",
                table: "OAuthProviderApps",
                columns: new[] { "UserId", "Provider" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppProfiles_OAuthProviderApps_ProviderAppId",
                table: "AppProfiles",
                column: "ProviderAppId",
                principalTable: "OAuthProviderApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppProfiles_OAuthProviderApps_ProviderAppId",
                table: "AppProfiles");

            migrationBuilder.DropTable(
                name: "OAuthProviderApps");

            migrationBuilder.DropIndex(
                name: "IX_AppProfile_ProviderAppId",
                table: "AppProfiles");

            migrationBuilder.DropColumn(
                name: "ExternalAccountId",
                table: "AppProfiles");

            migrationBuilder.DropColumn(
                name: "GrantedScopes",
                table: "AppProfiles");

            migrationBuilder.DropColumn(
                name: "ProviderAppId",
                table: "AppProfiles");
        }
    }
}
