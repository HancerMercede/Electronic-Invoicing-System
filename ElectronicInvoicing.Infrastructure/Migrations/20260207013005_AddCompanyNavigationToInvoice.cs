using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElectronicInvoicing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyNavigationToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Invoices",
                newName: "Invoices",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "InvoiceItems",
                newName: "InvoiceItems",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Companies",
                newName: "Companies",
                newSchema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Invoices",
                schema: "public",
                newName: "Invoices");

            migrationBuilder.RenameTable(
                name: "InvoiceItems",
                schema: "public",
                newName: "InvoiceItems");

            migrationBuilder.RenameTable(
                name: "Companies",
                schema: "public",
                newName: "Companies");
        }
    }
}
