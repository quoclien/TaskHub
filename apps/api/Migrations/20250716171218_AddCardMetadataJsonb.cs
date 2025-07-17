using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCardMetadataJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "cards",
                newName: "metadata");

            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "metadata",
                table: "cards",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "cards",
                newName: "Metadata");

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "cards",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
