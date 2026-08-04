using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummerGameServer.Migrations
{
    /// <inheritdoc />
    public partial class HardenGameDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE oldRoom FROM `UserRooms` oldRoom INNER JOIN `UserRooms` newRoom ON oldRoom.`UserId` = newRoom.`UserId` AND oldRoom.`Id` < newRoom.`Id`;");
            migrationBuilder.Sql(
                "DELETE room FROM `UserRooms` room LEFT JOIN `Users` user ON room.`UserId` = user.`Id` WHERE user.`Id` IS NULL;");
            migrationBuilder.Sql(
                "DELETE run FROM `StageRuns` run LEFT JOIN `Users` user ON run.`UserId` = user.`Id` WHERE user.`Id` IS NULL;");
            migrationBuilder.Sql(
                "UPDATE `Currencies` SET `Amount` = 0 WHERE `Amount` < 0;");
            migrationBuilder.Sql(
                "UPDATE `Characters` SET `Level` = GREATEST(`Level`, 1), `Exp` = GREATEST(`Exp`, 0);");

            migrationBuilder.AddColumn<long>(
                name: "ExpGained",
                table: "StageRuns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserRooms_UserId",
                table: "UserRooms",
                column: "UserId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Currencies_Amount",
                table: "Currencies",
                sql: "`Amount` >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Characters_Level_Exp",
                table: "Characters",
                sql: "`Level` >= 1 AND `Exp` >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_StageRuns_Users_UserId",
                table: "StageRuns",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRooms_Users_UserId",
                table: "UserRooms",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StageRuns_Users_UserId",
                table: "StageRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRooms_Users_UserId",
                table: "UserRooms");

            migrationBuilder.DropIndex(
                name: "IX_UserRooms_UserId",
                table: "UserRooms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Currencies_Amount",
                table: "Currencies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Characters_Level_Exp",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "ExpGained",
                table: "StageRuns");
        }
    }
}
