using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalAndGeneralConsultantCRM.Migrations
{
    public partial class gophercram : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Apt",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street1",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street2",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phoneNumber2",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apt",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Street1",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "Street2",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "phoneNumber2",
                table: "Leads");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
