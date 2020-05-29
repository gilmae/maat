using System;
using FluentMigrator;
namespace SV.Maat.Micropub
{

    [Migration(4)]
    public class AddEventsTable : Migration
    {
        public override void Down()
        {
            Delete.Table("events");
        }

        public override void Up()
        {
            Create.Table("events")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("aggregate_id").AsGuid()
                .WithColumn("type").AsString(128)
                .WithColumn("body").AsString()
                .WithColumn("event_type").AsString(128)
                .WithColumn("version").AsInt32().NotNullable();
        }
    }
}