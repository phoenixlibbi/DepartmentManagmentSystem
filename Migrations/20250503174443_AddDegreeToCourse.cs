using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MS.Migrations
{
    /// <inheritdoc />
    public partial class AddDegreeToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId1",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId1",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Students_StudentId",
                table: "ExamSeatings");

            migrationBuilder.DropTable(
                name: "Degrees");

            migrationBuilder.DropIndex(
                name: "IX_ExamSeatings_CourseId1",
                table: "ExamSeatings");

            migrationBuilder.DropIndex(
                name: "IX_ExamSeatings_RoomId1",
                table: "ExamSeatings");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "ExamSeatings");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "ExamSeatings");

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "RollNumber",
                keyValue: null,
                column: "RollNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RollNumber",
                table: "Students",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Courses",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId",
                table: "ExamSeatings",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId",
                table: "ExamSeatings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Students_StudentId",
                table: "ExamSeatings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId",
                table: "ExamSeatings");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSeatings_Students_StudentId",
                table: "ExamSeatings");

            migrationBuilder.AlterColumn<string>(
                name: "RollNumber",
                table: "Students",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "ExamSeatings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomId1",
                table: "ExamSeatings",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Courses",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Degrees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Degrees", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeatings_CourseId1",
                table: "ExamSeatings",
                column: "CourseId1");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSeatings_RoomId1",
                table: "ExamSeatings",
                column: "RoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId",
                table: "ExamSeatings",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Courses_CourseId1",
                table: "ExamSeatings",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId",
                table: "ExamSeatings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Rooms_RoomId1",
                table: "ExamSeatings",
                column: "RoomId1",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSeatings_Students_StudentId",
                table: "ExamSeatings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
