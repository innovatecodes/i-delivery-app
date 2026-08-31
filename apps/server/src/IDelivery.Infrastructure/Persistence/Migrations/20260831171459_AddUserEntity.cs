using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDelivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contact_info_exists",
                schema: "public",
                table: "tenants");

            migrationBuilder.RenameColumn(
                name: "contact_whatsapp",
                schema: "public",
                table: "tenants",
                newName: "whatsapp");

            migrationBuilder.RenameColumn(
                name: "contact_phone",
                schema: "public",
                table: "tenants",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                schema: "public",
                table: "tenants",
                newName: "email");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    role = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    activation_token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activation_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reset_password_token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reset_password_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.RenameColumn(
                name: "whatsapp",
                schema: "public",
                table: "tenants",
                newName: "contact_whatsapp");

            migrationBuilder.RenameColumn(
                name: "phone",
                schema: "public",
                table: "tenants",
                newName: "contact_phone");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "public",
                table: "tenants",
                newName: "contact_email");

            migrationBuilder.AddColumn<bool>(
                name: "contact_info_exists",
                schema: "public",
                table: "tenants",
                type: "boolean",
                nullable: true);
        }
    }
}
