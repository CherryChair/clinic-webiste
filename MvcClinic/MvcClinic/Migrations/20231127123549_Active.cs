using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcClinic.Migrations
{
    /// <inheritdoc />
    public partial class Active : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Patient",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Patient");
        }
    }
}
