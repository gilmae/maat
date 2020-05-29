using System;
using FluentMigrator;
namespace Migrations
{
    [Migration(2)]
    public class AddAuthenticationRequests : Migration
    {
        public override void Down()
        {
            Delete.Table("authenticationrequests");
        }

        public override void Up()
        {
            Create.Table("authenticationrequests")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("UserProfileUrl").AsString(128).Nullable()
                .WithColumn("ClientId").AsString(128)
                .WithColumn("RedirectUri").AsString(256).Nullable()
                .WithColumn("CsrfToken").AsString(256)
                .WithColumn("ResponseType").AsString(128).NotNullable()
                .WithColumn("AuthorisationCode").AsString(128).Nullable()
                .WithColumn("ClientName").AsString(128).Nullable()
                .WithColumn("ClientLogo").AsString(128).Nullable()
                .WithColumn("Scope").AsString(128).Nullable()
                .WithColumn("AuthCodeExpiresAt").AsDateTime2().Nullable()
                .WithColumn("Status").AsString(128).Nullable()
                .WithColumn("AccessToken").AsString(128).Nullable()
                .WithColumn("UserId").AsInt32();
        }
    }
}