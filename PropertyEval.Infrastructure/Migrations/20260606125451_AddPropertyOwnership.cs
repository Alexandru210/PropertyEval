using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyEval.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE p
                SET OwnerUserId = ownership.UserId
                FROM Properties AS p
                CROSS APPLY (
                    SELECT TOP (1) candidate.UserId
                    FROM (
                        SELECT l.UserId, l.CreatedAt, 0 AS SourcePriority, l.Id
                        FROM Listings AS l
                        WHERE l.PropertyId = p.Id

                        UNION ALL

                        SELECT e.RequestedByUserId AS UserId, e.CreatedAt, 1 AS SourcePriority, e.Id
                        FROM Evaluations AS e
                        WHERE e.PropertyId = p.Id
                    ) AS candidate
                    ORDER BY candidate.SourcePriority, candidate.CreatedAt, candidate.Id
                ) AS ownership
                WHERE p.OwnerUserId IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_OwnerUserId",
                table: "Properties",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Users_OwnerUserId",
                table: "Properties",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Users_OwnerUserId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_OwnerUserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Properties");
        }
    }
}
