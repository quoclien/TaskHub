using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleColumnAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoogleId",
                table: "users",
                newName: "google_id");

            migrationBuilder.AlterColumn<string>(
                name: "google_id",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "role",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "first_name", "google_id", "is_active", "last_login_at", "last_name", "password_hash", "role", "updated_at", "username" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", null, null, true, null, null, "$2a$11$rLmZgDJGNuFu.M7P1zYL1uZ7h4YDZE4XlkV3mL3LzHBE4jQ7jWf1m", 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo@taskhub.com", "Demo", null, true, null, "User", "$2a$11$8K2wJZjAe3qXV3xkR1L9K.6D4E0.aGJo.dI/MgJ8X.5sT9rYzN7mO", 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "demo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440001"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440002"));

            migrationBuilder.DropColumn(
                name: "role",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "google_id",
                table: "users",
                newName: "GoogleId");

            migrationBuilder.AlterColumn<string>(
                name: "GoogleId",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
