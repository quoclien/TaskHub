using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorEntitiesAndAddAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardMembers_Boards_BoardId",
                table: "BoardMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_BoardMembers_Users_UserId",
                table: "BoardMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Boards_Users_OwnerId",
                table: "Boards");

            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Lists_ListId",
                table: "Cards");

            migrationBuilder.DropForeignKey(
                name: "FK_Lists_Boards_BoardId",
                table: "Lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lists",
                table: "Lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cards",
                table: "Cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Boards",
                table: "Boards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BoardMembers",
                table: "BoardMembers");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Lists",
                newName: "lists");

            migrationBuilder.RenameTable(
                name: "Cards",
                newName: "cards");

            migrationBuilder.RenameTable(
                name: "Boards",
                newName: "boards");

            migrationBuilder.RenameTable(
                name: "BoardMembers",
                newName: "board_members");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "LastLoginAt",
                table: "users",
                newName: "last_login_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "users",
                newName: "avatar_url");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "lists",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "lists",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "lists",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "lists",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "lists",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "lists",
                newName: "board_id");

            migrationBuilder.RenameIndex(
                name: "IX_Lists_BoardId",
                table: "lists",
                newName: "ix_lists_board_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "cards",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "cards",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "cards",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "cards",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "cards",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "ListId",
                table: "cards",
                newName: "list_id");

            migrationBuilder.RenameColumn(
                name: "IsArchived",
                table: "cards",
                newName: "is_archived");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "cards",
                newName: "due_date");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "cards",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AssignedTo",
                table: "cards",
                newName: "assigned_to");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_ListId",
                table: "cards",
                newName: "ix_cards_list_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "boards",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "boards",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "boards",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "boards",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "boards",
                newName: "is_public");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "boards",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Boards_OwnerId",
                table: "boards",
                newName: "ix_boards_owner_id");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "board_members",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "board_members",
                newName: "joined_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "board_members",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "board_members",
                newName: "board_id");

            migrationBuilder.RenameIndex(
                name: "IX_BoardMembers_UserId",
                table: "board_members",
                newName: "ix_board_members_user_id");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "last_name",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "first_name",
                table: "users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "avatar_url",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "lists",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "cards",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "cards",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "is_archived",
                table: "cards",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "boards",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "is_public",
                table: "boards",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "boards",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "boards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_lists",
                table: "lists",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cards",
                table: "cards",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_boards",
                table: "boards",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_board_members",
                table: "board_members",
                columns: new[] { "board_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lists_board_id_position",
                table: "lists",
                columns: new[] { "board_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_cards_assigned_to",
                table: "cards",
                column: "assigned_to");

            migrationBuilder.CreateIndex(
                name: "ix_cards_due_date",
                table: "cards",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "ix_cards_list_id_position",
                table: "cards",
                columns: new[] { "list_id", "position" });

            migrationBuilder.CreateIndex(
                name: "ix_boards_is_public",
                table: "boards",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "ix_board_members_board_id",
                table: "board_members",
                column: "board_id");

            migrationBuilder.AddForeignKey(
                name: "FK_board_members_boards_board_id",
                table: "board_members",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_board_members_users_user_id",
                table: "board_members",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boards_users_owner_id",
                table: "boards",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cards_lists_list_id",
                table: "cards",
                column: "list_id",
                principalTable: "lists",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lists_boards_board_id",
                table: "lists",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_board_members_boards_board_id",
                table: "board_members");

            migrationBuilder.DropForeignKey(
                name: "FK_board_members_users_user_id",
                table: "board_members");

            migrationBuilder.DropForeignKey(
                name: "FK_boards_users_owner_id",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "FK_cards_lists_list_id",
                table: "cards");

            migrationBuilder.DropForeignKey(
                name: "FK_lists_boards_board_id",
                table: "lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_lists",
                table: "lists");

            migrationBuilder.DropIndex(
                name: "ix_lists_board_id_position",
                table: "lists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cards",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_assigned_to",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_due_date",
                table: "cards");

            migrationBuilder.DropIndex(
                name: "ix_cards_list_id_position",
                table: "cards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_boards",
                table: "boards");

            migrationBuilder.DropIndex(
                name: "ix_boards_is_public",
                table: "boards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_board_members",
                table: "board_members");

            migrationBuilder.DropIndex(
                name: "ix_board_members_board_id",
                table: "board_members");

            migrationBuilder.DropColumn(
                name: "description",
                table: "boards");

            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "boards");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "lists",
                newName: "Lists");

            migrationBuilder.RenameTable(
                name: "cards",
                newName: "Cards");

            migrationBuilder.RenameTable(
                name: "boards",
                newName: "Boards");

            migrationBuilder.RenameTable(
                name: "board_members",
                newName: "BoardMembers");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "last_login_at",
                table: "Users",
                newName: "LastLoginAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "Users",
                newName: "AvatarUrl");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Lists",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "Lists",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Lists",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Lists",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Lists",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "board_id",
                table: "Lists",
                newName: "BoardId");

            migrationBuilder.RenameIndex(
                name: "ix_lists_board_id",
                table: "Lists",
                newName: "IX_Lists_BoardId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Cards",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "Cards",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Cards",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Cards",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Cards",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "list_id",
                table: "Cards",
                newName: "ListId");

            migrationBuilder.RenameColumn(
                name: "is_archived",
                table: "Cards",
                newName: "IsArchived");

            migrationBuilder.RenameColumn(
                name: "due_date",
                table: "Cards",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Cards",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "assigned_to",
                table: "Cards",
                newName: "AssignedTo");

            migrationBuilder.RenameIndex(
                name: "ix_cards_list_id",
                table: "Cards",
                newName: "IX_Cards_ListId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Boards",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Boards",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Boards",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Boards",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "is_public",
                table: "Boards",
                newName: "IsPublic");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Boards",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_boards_owner_id",
                table: "Boards",
                newName: "IX_Boards_OwnerId");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "BoardMembers",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "joined_at",
                table: "BoardMembers",
                newName: "JoinedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "BoardMembers",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "board_id",
                table: "BoardMembers",
                newName: "BoardId");

            migrationBuilder.RenameIndex(
                name: "ix_board_members_user_id",
                table: "BoardMembers",
                newName: "IX_BoardMembers_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Lists",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Cards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Cards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Cards",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Boards",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublic",
                table: "Boards",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lists",
                table: "Lists",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cards",
                table: "Cards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Boards",
                table: "Boards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BoardMembers",
                table: "BoardMembers",
                columns: new[] { "BoardId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMembers_Boards_BoardId",
                table: "BoardMembers",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardMembers_Users_UserId",
                table: "BoardMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Boards_Users_OwnerId",
                table: "Boards",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Lists_ListId",
                table: "Cards",
                column: "ListId",
                principalTable: "Lists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lists_Boards_BoardId",
                table: "Lists",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
