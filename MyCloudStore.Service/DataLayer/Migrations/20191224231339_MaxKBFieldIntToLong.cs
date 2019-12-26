using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCloudStore.Service.DataLayer.Migrations
{
    public partial class MaxKBFieldIntToLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "MaxKBs",
                table: "AspNetUsers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MaxKBs",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
