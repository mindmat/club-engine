using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubEngine.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class PaymentFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFeeOverride",
                table: "Memberships",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalMails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalMailConfigurationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderMail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentPlainText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Imported = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MessageIdentifier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SendGridMessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalMails", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ExternalMails_Clubs_PartitionId",
                        column: x => x.PartitionId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountIban = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BookingsFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingsTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsFiles", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentsFiles_Clubs_PartitionId",
                        column: x => x.PartitionId,
                        principalTable: "Clubs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IbanProposed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClubId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    From = table.Column<DateOnly>(type: "date", nullable: false),
                    Until = table.Column<DateOnly>(type: "date", nullable: false),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periods", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Periods_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentsFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InstructionIdentification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecognizedEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Repaid_ReadModel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Settled_ReadModel = table.Column<bool>(type: "bit", nullable: false),
                    Ignore = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Bookings_Clubs_PartitionId",
                        column: x => x.PartitionId,
                        principalTable: "Clubs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bookings_PaymentsFiles_PaymentsFileId",
                        column: x => x.PaymentsFileId,
                        principalTable: "PaymentsFiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MembershipFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MembershipTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipFees", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MembershipFees_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipFees_MembershipTypes_MembershipTypeId",
                        column: x => x.MembershipTypeId,
                        principalTable: "MembershipTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipFees_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncomingPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebitorIban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DebitorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Charges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingPayments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_IncomingPayments_Bookings_Id",
                        column: x => x.Id,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutgoingPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreditorIban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Charges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingPayments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_OutgoingPayments_Bookings_Id",
                        column: x => x.Id,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncomingPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutgoingPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentAssignmentId_Counter = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayoutRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IncrementalKey = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAssignments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_IncomingPayments_IncomingPaymentId",
                        column: x => x.IncomingPaymentId,
                        principalTable: "IncomingPayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_OutgoingPayments_OutgoingPaymentId",
                        column: x => x.OutgoingPaymentId,
                        principalTable: "OutgoingPayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_PaymentAssignments_PaymentAssignmentId_Counter",
                        column: x => x.PaymentAssignmentId_Counter,
                        principalTable: "PaymentAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_PayoutRequests_PayoutRequestId",
                        column: x => x.PayoutRequestId,
                        principalTable: "PayoutRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_IncrementalKey",
                table: "Bookings",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PartitionId",
                table: "Bookings",
                column: "PartitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PaymentsFileId",
                table: "Bookings",
                column: "PaymentsFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalMails_IncrementalKey",
                table: "ExternalMails",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalMails_PartitionId",
                table: "ExternalMails",
                column: "PartitionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPayments_IncrementalKey",
                table: "IncomingPayments",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipFees_IncrementalKey",
                table: "MembershipFees",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipFees_MemberId",
                table: "MembershipFees",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipFees_MembershipTypeId",
                table: "MembershipFees",
                column: "MembershipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipFees_PeriodId",
                table: "MembershipFees",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPayments_IncrementalKey",
                table: "OutgoingPayments",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_IncomingPaymentId",
                table: "PaymentAssignments",
                column: "IncomingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_IncrementalKey",
                table: "PaymentAssignments",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_OutgoingPaymentId",
                table: "PaymentAssignments",
                column: "OutgoingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_PaymentAssignmentId_Counter",
                table: "PaymentAssignments",
                column: "PaymentAssignmentId_Counter");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_PayoutRequestId",
                table: "PaymentAssignments",
                column: "PayoutRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsFiles_IncrementalKey",
                table: "PaymentsFiles",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsFiles_PartitionId",
                table: "PaymentsFiles",
                column: "PartitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_IncrementalKey",
                table: "PayoutRequests",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Periods_ClubId",
                table: "Periods",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Periods_IncrementalKey",
                table: "Periods",
                column: "IncrementalKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalMails");

            migrationBuilder.DropTable(
                name: "MembershipFees");

            migrationBuilder.DropTable(
                name: "PaymentAssignments");

            migrationBuilder.DropTable(
                name: "Periods");

            migrationBuilder.DropTable(
                name: "IncomingPayments");

            migrationBuilder.DropTable(
                name: "OutgoingPayments");

            migrationBuilder.DropTable(
                name: "PayoutRequests");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "PaymentsFiles");

            migrationBuilder.DropColumn(
                name: "AnnualFeeOverride",
                table: "Memberships");
        }
    }
}
