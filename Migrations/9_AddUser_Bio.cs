using FluentMigrator;
namespace Migrations
{
    [Migration(9)]
    public class AddUsersBio : Migration
    {
        public override void Down()
        {
            Delete.Column("Bio").FromTable("users");
        }

        public override void Up()
        {
            Alter.Table("users")
                .AddColumn("Bio").AsString().Nullable();
        }
        
    }
}