using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThAmCo.Events.Migrations
{
    /// <inheritdoc />
    public partial class staff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StaffAssignments",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "INTEGER", nullable: false),
                    StaffId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAssignments", x => new { x.EventId, x.StaffId });
                    table.ForeignKey(
                        name: "FK_StaffAssignments_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffAssignments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StaffAssignments_StaffId",
                table: "StaffAssignments",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffAssignments");
        }
    }
}
