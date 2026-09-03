using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IDelivery.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantEmail",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "MinimumFee",
                table: "DeliverySettings",
                newName: "minimum_fee");

            migrationBuilder.RenameColumn(
                name: "MaximumFee",
                table: "DeliverySettings",
                newName: "maximum_fee");

            migrationBuilder.RenameColumn(
                name: "FreeAboveAmount",
                table: "DeliverySettings",
                newName: "free_above_amount");

            migrationBuilder.RenameColumn(
                name: "FixedFee",
                table: "DeliverySettings",
                newName: "fixed_fee");

            migrationBuilder.RenameColumn(
                name: "FeePerKm",
                table: "DeliverySettings",
                newName: "fee_per_km");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Customers",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Customers",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "CustomerAddresses",
                newName: "Address_Street");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "CustomerAddresses",
                newName: "Address_State");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "CustomerAddresses",
                newName: "Address_Reference");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "CustomerAddresses",
                newName: "Address_Number");

            migrationBuilder.RenameColumn(
                name: "Neighborhood",
                table: "CustomerAddresses",
                newName: "Address_Neighborhood");

            migrationBuilder.RenameColumn(
                name: "Complement",
                table: "CustomerAddresses",
                newName: "Address_Complement");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "CustomerAddresses",
                newName: "Address_City");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_City",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Complement",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Neighborhood",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Number",
                table: "Orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Reference",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_State",
                table: "Orders",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_Street",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress_ZipCode_DigitsOnly",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fee_per_km_currency",
                table: "DeliverySettings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fixed_fee_currency",
                table: "DeliverySettings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "free_above_amount_currency",
                table: "DeliverySettings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "maximum_fee_currency",
                table: "DeliverySettings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "minimum_fee_currency",
                table: "DeliverySettings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode_DigitsOnly",
                table: "CustomerAddresses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                table: "Payments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_City",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Complement",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Neighborhood",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Number",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Reference",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_State",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_Street",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress_ZipCode_DigitsOnly",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "fee_per_km_currency",
                table: "DeliverySettings");

            migrationBuilder.DropColumn(
                name: "fixed_fee_currency",
                table: "DeliverySettings");

            migrationBuilder.DropColumn(
                name: "free_above_amount_currency",
                table: "DeliverySettings");

            migrationBuilder.DropColumn(
                name: "maximum_fee_currency",
                table: "DeliverySettings");

            migrationBuilder.DropColumn(
                name: "minimum_fee_currency",
                table: "DeliverySettings");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode_DigitsOnly",
                table: "CustomerAddresses");

            migrationBuilder.RenameColumn(
                name: "minimum_fee",
                table: "DeliverySettings",
                newName: "MinimumFee");

            migrationBuilder.RenameColumn(
                name: "maximum_fee",
                table: "DeliverySettings",
                newName: "MaximumFee");

            migrationBuilder.RenameColumn(
                name: "free_above_amount",
                table: "DeliverySettings",
                newName: "FreeAboveAmount");

            migrationBuilder.RenameColumn(
                name: "fixed_fee",
                table: "DeliverySettings",
                newName: "FixedFee");

            migrationBuilder.RenameColumn(
                name: "fee_per_km",
                table: "DeliverySettings",
                newName: "FeePerKm");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Customers",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                table: "Customers",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "Address_Street",
                table: "CustomerAddresses",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "Address_State",
                table: "CustomerAddresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "Address_Reference",
                table: "CustomerAddresses",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "Address_Number",
                table: "CustomerAddresses",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "Address_Neighborhood",
                table: "CustomerAddresses",
                newName: "Neighborhood");

            migrationBuilder.RenameColumn(
                name: "Address_Complement",
                table: "CustomerAddresses",
                newName: "Complement");

            migrationBuilder.RenameColumn(
                name: "Address_City",
                table: "CustomerAddresses",
                newName: "City");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantEmail",
                table: "Customers",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }
    }
}
