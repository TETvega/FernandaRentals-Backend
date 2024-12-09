using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FernandaRentals.Migrations
{
    /// <inheritdoc />
    public partial class AddedPaypalCaptureIdFieldAtEventsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "paypal_capture_id",
                schema: "dbo",
                table: "events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paypal_capture_id",
                schema: "dbo",
                table: "events");
        }
    }
}
