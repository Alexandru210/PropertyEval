using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyEval.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluatorWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Users_UserId",
                table: "Evaluations");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Evaluations",
                newName: "RequestedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Evaluations_UserId",
                table: "Evaluations",
                newName: "IX_Evaluations_RequestedByUserId");

            migrationBuilder.AddColumn<int>(
                name: "EvaluatorUserId",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "Description", "Name" },
                values: new object[] { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Evaluator role for handling property valuation requests.", "Evaluator" });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EvaluatorUserId",
                table: "Evaluations",
                column: "EvaluatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Users_EvaluatorUserId",
                table: "Evaluations",
                column: "EvaluatorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Users_RequestedByUserId",
                table: "Evaluations",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Users_EvaluatorUserId",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Users_RequestedByUserId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EvaluatorUserId",
                table: "Evaluations");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "EvaluatorUserId",
                table: "Evaluations");

            migrationBuilder.RenameColumn(
                name: "RequestedByUserId",
                table: "Evaluations",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Evaluations_RequestedByUserId",
                table: "Evaluations",
                newName: "IX_Evaluations_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Users_UserId",
                table: "Evaluations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
