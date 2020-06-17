using FluentMigrator;
namespace Migrations
{
    [Migration(6)]
    public class AddSyndicationCredentials : Migration
    {
        public override void Down()
        {
            Delete.Column("Credentials").FromTable("syndications");
        }

        public override void Up()
        {
            Alter.Table("syndications")
                .AddColumn("Credentials").AsString().Nullable();
        }
    }
}