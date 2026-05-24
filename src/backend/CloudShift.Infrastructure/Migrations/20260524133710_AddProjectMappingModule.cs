using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudShift.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMappingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileTransferLogs_MigrationJobId",
                table: "FileTransferLogs");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMappings_UserId",
                table: "ProjectMappings",
                newName: "IX_ProjectMapping_UserId");

            migrationBuilder.AddColumn<string>(
                name: "ConflictResolutionRule",
                table: "ProjectMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilterConfigJson",
                table: "ProjectMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProjectMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationJob_Status",
                table: "MigrationJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FileTransferLog_JobId_Status",
                table: "FileTransferLogs",
                columns: new[] { "MigrationJobId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MigrationJob_Status",
                table: "MigrationJobs");

            migrationBuilder.DropIndex(
                name: "IX_FileTransferLog_JobId_Status",
                table: "FileTransferLogs");

            migrationBuilder.DropColumn(
                name: "ConflictResolutionRule",
                table: "ProjectMappings");

            migrationBuilder.DropColumn(
                name: "FilterConfigJson",
                table: "ProjectMappings");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProjectMappings");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMapping_UserId",
                table: "ProjectMappings",
                newName: "IX_ProjectMappings_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTransferLogs_MigrationJobId",
                table: "FileTransferLogs",
                column: "MigrationJobId");
        }
    }
}
