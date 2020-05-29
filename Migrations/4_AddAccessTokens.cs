using System;
using FluentMigrator;
namespace Migrations
{
    [Migration(4)]
    public class AddAccessTokens : Migration
    {
        public override void Down()
        {
            Delete.Table("accesstokens");
        }

        public override void Up()
        {
            Create.Table("accesstokens")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AuthenticationRequestId").AsInt32().NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(128).NotNullable()
                .WithColumn("ClientId").AsString(128)
                .WithColumn("Scope").AsString(128).NotNullable();
        }
    }
}