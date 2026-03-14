using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuyAlan.Initializer.Migrations
{
    /// <inheritdoc />
    public partial class Intivitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveSubscriptionId",
                table: "srbd_asp_net_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "srbd_subscription_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("srbd_pk_srbd_subscription_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "srbd_fk_srbd_subscription_invitations_srbd_asp_net_users_invited_by_u~",
                        column: x => x.InvitedByUserId,
                        principalTable: "srbd_asp_net_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "srbd_fk_srbd_subscription_invitations_srbd_subscriptions_subscripti~",
                        column: x => x.SubscriptionId,
                        principalTable: "srbd_subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "srbd_ix_srbd_asp_net_users_active_subscription_id",
                table: "srbd_asp_net_users",
                column: "ActiveSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "srbd_ix_srbd_subscription_invitations_invited_by_user_id",
                table: "srbd_subscription_invitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "srbd_ix_srbd_subscription_invitations_subscription_id_email",
                table: "srbd_subscription_invitations",
                columns: new[] { "SubscriptionId", "Email" });

            migrationBuilder.CreateIndex(
                name: "srbd_ix_srbd_subscription_invitations_token",
                table: "srbd_subscription_invitations",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "srbd_fk_srbd_asp_net_users_subscriptions_active_subscription_id",
                table: "srbd_asp_net_users",
                column: "ActiveSubscriptionId",
                principalTable: "srbd_subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "srbd_fk_srbd_asp_net_users_subscriptions_active_subscription_id",
                table: "srbd_asp_net_users");

            migrationBuilder.DropTable(
                name: "srbd_subscription_invitations");

            migrationBuilder.DropIndex(
                name: "srbd_ix_srbd_asp_net_users_active_subscription_id",
                table: "srbd_asp_net_users");

            migrationBuilder.DropColumn(
                name: "ActiveSubscriptionId",
                table: "srbd_asp_net_users");
        }
    }
}
