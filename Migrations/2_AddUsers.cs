using FluentMigrator;
namespace Migrations
{
    [Migration(2)]
    public class AddUsers : Migration
    {
        public override void Down()
        {
            Delete.Table("Users");
        }

        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Name").AsString(128)
                .WithColumn("Email").AsString(128)
                .WithColumn("Url").AsString(256)
                .WithColumn("HashedPassword").AsString(128).NotNullable();
        }
    }
}