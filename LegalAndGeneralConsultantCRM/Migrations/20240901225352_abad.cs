using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalAndGeneralConsultantCRM.Migrations
{
    public partial class abad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update for the Leads table (no changes here)
            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // Drop the existing ProfileImage column
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "AspNetUsers");

            // Add the ProfileImage column with the new type
            migrationBuilder.AddColumn<byte[]>(
                name: "ProfileImage",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert changes for the Leads table (no changes here)
            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Leads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            // Drop the new ProfileImage column with type byte[]
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "AspNetUsers");

            // Add the ProfileImage column back with the original string type
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
