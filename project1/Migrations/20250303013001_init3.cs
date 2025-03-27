using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project1.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "VacationRequests",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "IsAnnualLeave",
                table: "VacationRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "SickLeaveRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnnualLeaveRemainingAfterDec31",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnnualLeaveUsedByDec31",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAnnualLeaveUsedDate",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SickLeaveDays",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnnualLeave",
                table: "VacationRequests");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "SickLeaveRequests");

            migrationBuilder.DropColumn(
                name: "AnnualLeaveRemainingAfterDec31",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AnnualLeaveUsedByDec31",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastAnnualLeaveUsedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SickLeaveDays",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "VacationRequests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
