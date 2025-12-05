using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Velum.Base.Migrations
{
    /// <inheritdoc />
    public partial class AddInterpretationGuide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Questionnaires",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "InterpretationGuide",
                table: "Questionnaires",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Questionnaires",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Questionnaires");

            migrationBuilder.DropColumn(
                name: "InterpretationGuide",
                table: "Questionnaires");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Questionnaires");
        }
    }
}
