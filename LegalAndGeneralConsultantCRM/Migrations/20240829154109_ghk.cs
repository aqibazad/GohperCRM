using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalAndGeneralConsultantCRM.Migrations
{
    public partial class ghk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpReminder",
                table: "FollowUps",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUpReminder",
                table: "FollowUps");
        }
    }
}
