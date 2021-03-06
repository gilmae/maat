﻿using System;
using FluentMigrator;

namespace Migrations
{
    [Migration(7)]
    public class Add_Mastodon_AppRegistrations : Migration
    {
        public override void Down()
        {
            Delete.Table("mastodonapps");
        }

        public override void Up()
        {
            Create.Table("mastodonapps")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("instance").AsString(256)
                .WithColumn("authenticationclient").AsString();
        }
    }
}
