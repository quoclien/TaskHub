using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCardMetadataConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "metadata",
                table: "cards",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, object>>(
                name: "metadata",
                table: "cards",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(Dictionary<string, object>),
                oldType: "jsonb",
                oldNullable: true,
                oldDefaultValueSql: "'{}'::jsonb");
        }
    }
}
