﻿using System;
using FluentMigrator;
namespace SV.Maat.Micropub
{

    [Migration(202004261035)]
    public class AddEventsTable : Migration
    {
        public override void Down()
        {
            Delete.Table("events");
        }

        public override void Up()
        {
            Create.Table("events")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("aggregate_id").AsGuid()
                .WithColumn("type").AsString(128)
                .WithColumn("Body").AsString()
                .WithColumn("event_type").AsString(128)
                .WithColumn("version").AsInt32().NotNullable();
        }
    }
}