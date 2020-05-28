using System;
using FluentMigrator;
namespace Migrations
{
    [Migration(202005211938)]
    public class AddAccessTokens : Migration
    {
        public override void Down()
        {
            Delete.Table("accesstokens");
        }

        public override void Up()
        {
            Create.Table("accesstokens")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("AuthenticationRequestId").AsInt64()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("Name").AsString(128).NotNullable()
                .WithColumn("ClientId").AsString(128)
                .WithColumn("Scope").AsString(128).NotNullable();
        }
    }
}