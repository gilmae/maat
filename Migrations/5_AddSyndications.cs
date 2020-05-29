using FluentMigrator;
namespace Migrations
{
    [Migration(5)]
    public class AddSyndications : Migration
    {
        public override void Down()
        {
            Delete.Table("syndications");
        }

        public override void Up()
        {
            Create.Table("syndications")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("AccountName").AsString(128).NotNullable()
                .WithColumn("Network").AsString(128).NotNullable()
                .WithColumn("UserId").AsInt32().NotNullable();
        }
    }
}