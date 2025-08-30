using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocPatientAppMVC.Migrations
{
    /// <inheritdoc />
    public partial class modify_patient_tbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DOB",
                table: "Patients",
                newName: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Patients",
                newName: "DOB");
        }
    }
}
