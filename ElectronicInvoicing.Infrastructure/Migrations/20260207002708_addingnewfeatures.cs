using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicInvoicing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addingnewfeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DgiiRawResponse",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrContent",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedXmlPath",
                table: "Invoices",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "InvoiceItems",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DgiiRawResponse",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "QrContent",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SignedXmlPath",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "Invoices");

            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "InvoiceItems",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
