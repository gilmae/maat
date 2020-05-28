using FluentMigrator;
namespace Migrations
{
    [Migration(202005091815)]
    public class AddSyndications : Migration
    {
        public override void Down()
        {
            Delete.Table("Syndications");
        }

        public override void Up()
        {
            Create.Table("Syndications")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("AccountName").AsString(128).NotNullable()
                .WithColumn("Network").AsString(128).NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable();
        }
    }
}