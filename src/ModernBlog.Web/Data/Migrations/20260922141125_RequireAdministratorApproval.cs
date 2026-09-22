using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModernBlog.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequireAdministratorApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Bootstrap only the account that exists when this migration runs.
            // Never grant administrative privileges during public registration.
            migrationBuilder.Sql("""
                INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                VALUES ('modernblog-system-administrator', 'SystemAdministrator', 'SYSTEMADMINISTRATOR', 'modernblog-system-administrator');

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT Id, 'modernblog-system-administrator' FROM AspNetUsers
                WHERE NormalizedEmail = 'JUSTINMARKARK@GMAIL.COM';

                UPDATE AspNetUsers SET ApprovalStatus = 1, SecurityStamp = lower(hex(randomblob(16)))
                WHERE NormalizedEmail = 'JUSTINMARKARK@GMAIL.COM';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM AspNetUserRoles WHERE RoleId = 'modernblog-system-administrator';
                DELETE FROM AspNetRoles WHERE Id = 'modernblog-system-administrator';
                """);
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "AspNetUsers");
        }
    }
}
