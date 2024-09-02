using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalAndGeneralConsultantCRM.Migrations
{
    public partial class kgkng : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AspNetUsers",
                newName: "StateLicensed");

            migrationBuilder.AddColumn<string>(
                name: "Address1",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Licensed",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mywebsite",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address1",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address2",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Licensed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Mywebsite",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "StateLicensed",
                table: "AspNetUsers",
                newName: "Address");
        }
    }
}
