using FluentMigrator;
namespace Migrations
{
    [Migration(8)]
    public class AddUsersHost : Migration
    {
        public override void Down()
        {
            Delete.Column("Host").FromTable("users");
        }

        public override void Up()
        {
            Alter.Table("users")
                .AddColumn("Host").AsString().Nullable();
        }
        
    }
}