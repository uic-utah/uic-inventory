﻿// <auto-generated />
using System;
using System.Collections.Generic;
using api.Features;
using api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace api.Migrations {
  [DbContext(typeof(AppDbContext))]
  [Migration("20210624043645_SiteLocation")]
  partial class SiteLocation {
    protected override void BuildTargetModel(ModelBuilder modelBuilder) {
#pragma warning disable 612, 618
      modelBuilder
          .HasDefaultSchema("public")
          .HasPostgresEnum(null, "access_level", new[] { "elevated", "standard" })
          .HasPostgresEnum(null, "contact_types", new[] { "owner_operator", "facility_owner", "facility_operator", "facility_manager", "legal_rep", "official_rep", "contractor", "health_dept", "permit_writer", "developer", "other", "project_manager" })
          .HasPostgresEnum(null, "notification_types", new[] { "new_user_account_registration" })
          .HasPostgresEnum("access_level", new[] { "standard", "elevated" })
          .HasPostgresEnum("contact_types", new[] { "owner_operator", "facility_owner", "facility_operator", "facility_manager", "legal_rep", "official_rep", "contractor", "health_dept", "permit_writer", "developer", "other", "project_manager" })
          .HasPostgresEnum("notification_types", new[] { "new_user_account_registration", "facility_contact_modified" })
          .HasAnnotation("Relational:Collation", "en_US.utf8")
          .HasAnnotation("Relational:MaxIdentifierLength", 63)
          .HasAnnotation("ProductVersion", "5.0.7")
          .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

      modelBuilder.Entity("api.GraphQL.Account", b => {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("id")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        b.Property<AccessLevels>("Access")
            .ValueGeneratedOnAdd()
            .HasColumnType("access_level")
            .HasDefaultValue(AccessLevels.standard)
            .HasColumnName("account_access");

        b.Property<string>("City")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("city");

        b.Property<string>("Email")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("email");

        b.Property<string>("FirstName")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("first_name");

        b.Property<string>("LastName")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("last_name");

        b.Property<string>("MailingAddress")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("mailing_address");

        b.Property<string>("Organization")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("organization");

        b.Property<string>("PhoneNumber")
            .HasMaxLength(64)
            .HasColumnType("character varying(64)")
            .HasColumnName("phone");

        b.Property<bool>("ProfileComplete")
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnType("boolean")
            .HasColumnName("complete_profile")
            .HasComputedColumnSql("\nCASE\n    WHEN ((length((organization)::text) > 0) AND (length((email)::text) > 0) AND (length((phone)::text) > 0) AND (length((mailing_address)::text) > 0) AND (length((city)::text) > 0) AND (length((state)::text) > 0) AND (length((zip_code)::text) > 0)) THEN true\n    ELSE false\nEND", true);

        b.Property<bool?>("ReceiveNotifications")
            .ValueGeneratedOnAdd()
            .HasColumnType("boolean")
            .HasColumnName("receive_notifications")
            .HasDefaultValueSql("false");

        b.Property<string>("State")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("state");

        b.Property<string>("UtahId")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("utah_id");

        b.Property<string>("ZipCode")
            .HasMaxLength(64)
            .HasColumnType("character varying(64)")
            .HasColumnName("zip_code");

        b.HasKey("Id")
            .HasName("pk_accounts");

        b.HasIndex(new[] { "UtahId" }, "account_utah_id_key")
            .IsUnique()
            .HasDatabaseName("ix_accounts_utah_id");

        b.ToTable("accounts");
      });

      modelBuilder.Entity("api.GraphQL.Contact", b => {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("id")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        b.Property<string>("City")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("city");

        b.Property<ContactTypes>("ContactType")
            .HasColumnType("contact_types")
            .HasColumnName("contact_type");

        b.Property<string>("Email")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("email");

        b.Property<string>("FirstName")
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("first_name");

        b.Property<string>("LastName")
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("last_name");

        b.Property<string>("MailingAddress")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("mailing_address");

        b.Property<string>("Organization")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("organization");

        b.Property<string>("PhoneNumber")
            .HasMaxLength(64)
            .HasColumnType("character varying(64)")
            .HasColumnName("phone");

        b.Property<int>("SiteFk")
            .HasColumnType("integer")
            .HasColumnName("site_fk");

        b.Property<string>("State")
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("state");

        b.Property<string>("ZipCode")
            .HasMaxLength(64)
            .HasColumnType("character varying(64)")
            .HasColumnName("zip_code");

        b.HasKey("Id")
            .HasName("pk_contacts");

        b.HasIndex("SiteFk")
            .HasDatabaseName("ix_contacts_site_fk");

        b.ToTable("contacts");
      });

      modelBuilder.Entity("api.GraphQL.Notification", b => {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("id")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        b.Property<Dictionary<string, object>>("AdditionalData")
            .HasColumnType("jsonb")
            .HasColumnName("additional_data");

        b.Property<DateTime>("CreatedAt")
            .ValueGeneratedOnAdd()
            .HasColumnType("timestamp without time zone")
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()");

        b.Property<NotificationTypes?>("NotificationType")
            .HasColumnType("notification_types")
            .HasColumnName("notification_type");

        b.Property<string>("Url")
            .HasMaxLength(512)
            .HasColumnType("character varying(512)")
            .HasColumnName("url");

        b.HasKey("Id")
            .HasName("pk_notifications");

        b.ToTable("notifications");
      });

      modelBuilder.Entity("api.GraphQL.NotificationReceipt", b => {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("id")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        b.Property<DateTime?>("DeletedAt")
            .HasColumnType("timestamp without time zone")
            .HasColumnName("deleted_at");

        b.Property<int>("NotificationFk")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("notification_fk");

        b.Property<DateTime?>("ReadAt")
            .HasColumnType("timestamp without time zone")
            .HasColumnName("read_at");

        b.Property<int>("RecipientId")
            .HasColumnType("integer")
            .HasColumnName("recipient_id");

        b.HasKey("Id")
            .HasName("pk_notification_receipts");

        b.HasIndex("NotificationFk")
            .HasDatabaseName("ix_notification_receipts_notification_fk");

        b.HasIndex("RecipientId")
            .HasDatabaseName("ix_notification_receipts_recipient_id");

        b.ToTable("notification_receipts");
      });

      modelBuilder.Entity("api.GraphQL.Site", b => {
        b.Property<int>("Id")
            .ValueGeneratedOnAdd()
            .HasColumnType("integer")
            .HasColumnName("id")
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        b.Property<int>("AccountFk")
            .HasColumnType("integer")
            .HasColumnName("account_fk");

        b.Property<string>("Address")
            .HasColumnType("text")
            .HasColumnName("address");

        b.Property<string>("Geometry")
            .HasColumnType("jsonb")
            .HasColumnName("geometry");

        b.Property<int?>("NaicsPrimary")
            .HasColumnType("integer")
            .HasColumnName("naics_primary");

        b.Property<string>("NaicsTitle")
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("naics_title");

        b.Property<string>("Name")
            .IsRequired()
            .HasMaxLength(128)
            .HasColumnType("character varying(128)")
            .HasColumnName("name");

        b.Property<string>("Ownership")
            .IsRequired()
            .HasMaxLength(2)
            .HasColumnType("character varying(2)")
            .HasColumnName("ownership");

        b.HasKey("Id")
            .HasName("pk_sites");

        b.HasIndex("AccountFk")
            .HasDatabaseName("ix_sites_account_fk");

        b.ToTable("sites");
      });

      modelBuilder.Entity("api.GraphQL.Contact", b => {
        b.HasOne("api.GraphQL.Site", "Site")
            .WithMany("Contacts")
            .HasForeignKey("SiteFk")
            .HasConstraintName("contact_to_site_fk")
            .IsRequired();

        b.Navigation("Site");
      });

      modelBuilder.Entity("api.GraphQL.NotificationReceipt", b => {
        b.HasOne("api.GraphQL.Notification", "Notification")
            .WithMany("NotificationReceipts")
            .HasForeignKey("NotificationFk")
            .HasConstraintName("notification_receipt_fk")
            .IsRequired();

        b.HasOne("api.GraphQL.Account", "Recipient")
            .WithMany("NotificationReceipts")
            .HasForeignKey("RecipientId")
            .HasConstraintName("accounts_fk")
            .IsRequired();

        b.Navigation("Notification");

        b.Navigation("Recipient");
      });

      modelBuilder.Entity("api.GraphQL.Site", b => {
        b.HasOne("api.GraphQL.Account", "Account")
            .WithMany("Sites")
            .HasForeignKey("AccountFk")
            .HasConstraintName("site_to_account_fk")
            .IsRequired();

        b.Navigation("Account");
      });

      modelBuilder.Entity("api.GraphQL.Account", b => {
        b.Navigation("NotificationReceipts");

        b.Navigation("Sites");
      });

      modelBuilder.Entity("api.GraphQL.Notification", b => {
        b.Navigation("NotificationReceipts");
      });

      modelBuilder.Entity("api.GraphQL.Site", b => {
        b.Navigation("Contacts");
      });
#pragma warning restore 612, 618
    }
  }
}