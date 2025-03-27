using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project1.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonToVacationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "VacationRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "VacationRequests");
        }
    }
}
