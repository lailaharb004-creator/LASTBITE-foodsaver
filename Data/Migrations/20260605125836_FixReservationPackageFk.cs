using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastBiteNew.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixReservationPackageFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_FoodPackages_FoodPackagePackageId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_FoodPackagePackageId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FoodPackagePackageId",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_FoodPackages_PackageId",
                table: "Reservations",
                column: "PackageId",
                principalTable: "FoodPackages",
                principalColumn: "PackageId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_FoodPackages_PackageId",
                table: "Reservations");

            migrationBuilder.AddColumn<int>(
                name: "FoodPackagePackageId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_FoodPackagePackageId",
                table: "Reservations",
                column: "FoodPackagePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_FoodPackages_FoodPackagePackageId",
                table: "Reservations",
                column: "FoodPackagePackageId",
                principalTable: "FoodPackages",
                principalColumn: "PackageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
