using FluentMigrator;
namespace Migrations
{
    [Migration(1)]
    public class AddUsers : Migration
    {
        public override void Down()
        {
            Delete.Table("users");
        }

        public override void Up()
        {
            Create.Table("users")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(128)
                .WithColumn("Username").AsString(128)
                .WithColumn("Email").AsString(128).Nullable()
                .WithColumn("Url").AsString(256).Nullable()
                .WithColumn("HashedPassword").AsString(128).NotNullable();
        }
    }
}