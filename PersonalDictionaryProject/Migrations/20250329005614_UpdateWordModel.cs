using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalDictionaryProject.Migrations
{
    public partial class UpdateWordModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedYet",
                table: "Words",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApprovedYet",
                table: "Words");
        }
    }
}
