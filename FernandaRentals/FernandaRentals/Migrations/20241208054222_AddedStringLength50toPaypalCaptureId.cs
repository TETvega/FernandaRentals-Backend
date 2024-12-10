using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FernandaRentals.Migrations
{
    /// <inheritdoc />
    public partial class AddedStringLength50toPaypalCaptureId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "paypal_capture_id",
                schema: "dbo",
                table: "events",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "paypal_capture_id",
                schema: "dbo",
                table: "events",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
