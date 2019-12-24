using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCloudStore.Service.DataLayer.Migrations
{
    public partial class AddMaxKBsField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxKBs",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxKBs",
                table: "AspNetUsers");
        }
    }
}
