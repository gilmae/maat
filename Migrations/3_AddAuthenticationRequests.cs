using System;
using FluentMigrator;
namespace Migrations
{
    [Migration(3)]
    public class AddAuthenticationRequests : Migration
    {
        public override void Down()
        {
            Delete.Table("authenticationrequests");
        }

        public override void Up()
        {
            Create.Table("authenticationrequests")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("UserProfileUrl").AsString(128)
                .WithColumn("ClientId").AsString(128)
                .WithColumn("RedirectUrl").AsString(256)
                .WithColumn("CsrfToken").AsString(256)
                .WithColumn("ResponseType").AsString(128).NotNullable()
                .WithColumn("AuthorisationCode").AsString(128)
                .WithColumn("ClientName").AsString(128)
                .WithColumn("ClientLogo").AsString(128)
                .WithColumn("Scope").AsString(128)
                .WithColumn("AuthCodeExpiresAt").AsDateTime2()
                .WithColumn("State").AsString(128)
                .WithColumn("AccessToken").AsString(128)
                .WithColumn("UserId").AsInt64();
        }
    }
}