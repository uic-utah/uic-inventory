﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using api.Features;
using api.Infrastructure;

#nullable disable

namespace api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .UseCollation("en_US.utf8")
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "access_level", new[] { "elevated", "standard" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "contact_types", new[] { "owner_operator", "facility_owner", "facility_operator", "facility_manager", "legal_rep", "official_rep", "contractor", "health_dept", "permit_writer", "developer", "other", "project_manager" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "notification_types", new[] { "new_user_account_registration" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "public", "access_level", new[] { "standard", "elevated" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "public", "contact_types", new[] { "owner_operator", "facility_owner", "facility_operator", "facility_manager", "legal_rep", "official_rep", "contractor", "project_manager", "health_dept", "permit_writer", "developer", "other" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "public", "notification_types", new[] { "new_user_account_registration", "inventory_submission", "facility_contact_modified" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("api.Features.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<AccessLevels>("Access")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("access_levels")
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

                    b.ToTable("accounts", "public");
                });

            modelBuilder.Entity("api.Features.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

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

                    b.Property<bool>("SerContact")
                        .HasColumnType("boolean")
                        .HasColumnName("ser_contact");

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

                    b.ToTable("contacts", "public");
                });

            modelBuilder.Entity("api.Features.Inventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountFk")
                        .HasColumnType("integer")
                        .HasColumnName("account_fk");

                    b.Property<bool>("ContactStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("contact_status");

                    b.Property<DateTime?>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP()");

                    b.Property<bool>("DetailStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("detail_status");

                    b.Property<string>("Edocs")
                        .HasColumnType("text")
                        .HasColumnName("edocs");

                    b.Property<string>("Flagged")
                        .HasColumnType("text")
                        .HasColumnName("flagged");

                    b.Property<bool>("LocationStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("location_status");

                    b.Property<int>("OrderNumber")
                        .HasMaxLength(128)
                        .HasColumnType("integer")
                        .HasColumnName("order_number");

                    b.Property<bool>("PaymentStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("payment_status");

                    b.Property<string>("Signature")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("signature");

                    b.Property<bool>("SignatureStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("signature_status");

                    b.Property<int>("SiteFk")
                        .HasColumnType("integer")
                        .HasColumnName("site_fk");

                    b.Property<InventoryStatus>("Status")
                        .HasColumnType("inventory_status")
                        .HasColumnName("status");

                    b.Property<int>("SubClass")
                        .HasColumnType("integer")
                        .HasColumnName("sub_class");

                    b.Property<DateTime?>("SubmittedOn")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("submitted_on");

                    b.HasKey("Id")
                        .HasName("inventory_primary_key");

                    b.HasIndex("AccountFk")
                        .HasDatabaseName("ix_inventory_account_fk");

                    b.HasIndex("SiteFk")
                        .HasDatabaseName("ix_inventory_site_fk");

                    b.ToTable("inventories", "public");
                });

            modelBuilder.Entity("api.Features.Notification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Dictionary<string, object>>("AdditionalData")
                        .HasColumnType("jsonb")
                        .HasColumnName("additional_data");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
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

                    b.ToTable("notifications", "public");
                });

            modelBuilder.Entity("api.Features.NotificationReceipt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_at");

                    b.Property<int>("NotificationFk")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("notification_fk");

                    b.Property<DateTime?>("ReadAt")
                        .HasColumnType("timestamp with time zone")
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

                    b.ToTable("notification_receipts", "public");
                });

            modelBuilder.Entity("api.Features.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountFk")
                        .HasColumnType("integer")
                        .HasColumnName("account_fk");

                    b.Property<string>("Address")
                        .HasColumnType("text")
                        .HasColumnName("address");

                    b.Property<bool>("ContactStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("contact_status");

                    b.Property<bool>("DetailStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("detail_status");

                    b.Property<string>("Geometry")
                        .HasColumnType("jsonb")
                        .HasColumnName("geometry");

                    b.Property<bool>("LocationStatus")
                        .HasColumnType("boolean")
                        .HasColumnName("location_status");

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

                    b.Property<SiteStatus>("Status")
                        .HasColumnType("site_status")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_sites");

                    b.HasIndex("AccountFk")
                        .HasDatabaseName("ix_sites_account_fk");

                    b.ToTable("sites", "public");
                });

            modelBuilder.Entity("api.Features.WaterSystemContacts", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountFk")
                        .HasColumnType("integer")
                        .HasColumnName("account_fk");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("email");

                    b.Property<int>("InventoryFk")
                        .HasColumnType("integer")
                        .HasColumnName("inventory_fk");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("name");

                    b.Property<int>("SiteFk")
                        .HasColumnType("integer")
                        .HasColumnName("site_fk");

                    b.Property<string>("System")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("system");

                    b.Property<int>("WellFk")
                        .HasColumnType("integer")
                        .HasColumnName("well_fk");

                    b.HasKey("Id")
                        .HasName("pk_water_system_contacts");

                    b.HasIndex("InventoryFk")
                        .HasDatabaseName("ix_water_system_contacts_inventory_fk");

                    b.HasIndex("WellFk")
                        .HasDatabaseName("ix_water_system_contacts_well_fk");

                    b.ToTable("water_system_contacts", "public");
                });

            modelBuilder.Entity("api.Features.Well", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountFk")
                        .HasColumnType("integer")
                        .HasColumnName("account_fk");

                    b.Property<string>("ConstructionDetails")
                        .HasMaxLength(2500)
                        .HasColumnType("character varying(2500)")
                        .HasColumnName("construction_details");

                    b.Property<DateTime?>("CreatedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_on")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Geometry")
                        .HasColumnType("jsonb")
                        .HasColumnName("geometry");

                    b.Property<string>("HydrogeologicCharacterization")
                        .HasMaxLength(2500)
                        .HasColumnType("character varying(2500)")
                        .HasColumnName("hydrogeologic_characterization");

                    b.Property<string>("InjectateCharacterization")
                        .HasMaxLength(2500)
                        .HasColumnType("character varying(2500)")
                        .HasColumnName("injectate_characterization");

                    b.Property<int>("InventoryFk")
                        .HasColumnType("integer")
                        .HasColumnName("inventory_fk");

                    b.Property<int?>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<string>("RemediationDescription")
                        .HasColumnType("text")
                        .HasColumnName("remediation_description");

                    b.Property<string>("RemediationProjectId")
                        .HasColumnType("text")
                        .HasColumnName("remediation_project_id");

                    b.Property<int?>("RemediationType")
                        .HasColumnType("integer")
                        .HasColumnName("remediation_type");

                    b.Property<int>("SiteFk")
                        .HasColumnType("integer")
                        .HasColumnName("site_fk");

                    b.Property<string>("Status")
                        .HasColumnType("text")
                        .HasColumnName("status");

                    b.Property<int>("SubClass")
                        .HasColumnType("integer")
                        .HasColumnName("sub_class");

                    b.Property<string>("SurfaceWaterProtection")
                        .HasColumnType("text")
                        .HasColumnName("surface_water_protection");

                    b.Property<string>("WellName")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasColumnName("well_name");

                    b.HasKey("Id")
                        .HasName("pk_wells");

                    b.HasIndex("AccountFk")
                        .HasDatabaseName("ix_wells_account_fk");

                    b.HasIndex("InventoryFk")
                        .HasDatabaseName("ix_wells_inventory_fk");

                    b.HasIndex("SiteFk")
                        .HasDatabaseName("ix_wells_site_fk");

                    b.ToTable("wells", "public");
                });

            modelBuilder.Entity("api.Features.Contact", b =>
                {
                    b.HasOne("api.Features.Site", "Site")
                        .WithMany("Contacts")
                        .HasForeignKey("SiteFk")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired()
                        .HasConstraintName("contact_to_site_fk");

                    b.Navigation("Site");
                });

            modelBuilder.Entity("api.Features.Inventory", b =>
                {
                    b.HasOne("api.Features.Account", "Account")
                        .WithMany("Inventories")
                        .HasForeignKey("AccountFk")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("inventory_to_account_fk");

                    b.HasOne("api.Features.Site", "Site")
                        .WithMany("Inventories")
                        .HasForeignKey("SiteFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("inventory_to_site_fk");

                    b.Navigation("Account");

                    b.Navigation("Site");
                });

            modelBuilder.Entity("api.Features.NotificationReceipt", b =>
                {
                    b.HasOne("api.Features.Notification", "Notification")
                        .WithMany("NotificationReceipts")
                        .HasForeignKey("NotificationFk")
                        .IsRequired()
                        .HasConstraintName("notification_receipt_fk");

                    b.HasOne("api.Features.Account", "Recipient")
                        .WithMany("NotificationReceipts")
                        .HasForeignKey("RecipientId")
                        .IsRequired()
                        .HasConstraintName("accounts_fk");

                    b.Navigation("Notification");

                    b.Navigation("Recipient");
                });

            modelBuilder.Entity("api.Features.Site", b =>
                {
                    b.HasOne("api.Features.Account", "Account")
                        .WithMany("Sites")
                        .HasForeignKey("AccountFk")
                        .IsRequired()
                        .HasConstraintName("site_to_account_fk");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("api.Features.WaterSystemContacts", b =>
                {
                    b.HasOne("api.Features.Inventory", "Inventory")
                        .WithMany()
                        .HasForeignKey("InventoryFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_water_system_contacts_inventories_inventory_fk");

                    b.HasOne("api.Features.Well", "Well")
                        .WithMany("WaterSystemContacts")
                        .HasForeignKey("WellFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_water_system_contacts_wells_well_fk");

                    b.Navigation("Inventory");

                    b.Navigation("Well");
                });

            modelBuilder.Entity("api.Features.Well", b =>
                {
                    b.HasOne("api.Features.Account", "Account")
                        .WithMany("Wells")
                        .HasForeignKey("AccountFk")
                        .IsRequired()
                        .HasConstraintName("well_to_account_fk");

                    b.HasOne("api.Features.Inventory", "Inventory")
                        .WithMany("Wells")
                        .HasForeignKey("InventoryFk")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("well_to_inventory_fk");

                    b.HasOne("api.Features.Site", "Site")
                        .WithMany("Wells")
                        .HasForeignKey("SiteFk")
                        .IsRequired()
                        .HasConstraintName("well_to_site_fk");

                    b.Navigation("Account");

                    b.Navigation("Inventory");

                    b.Navigation("Site");
                });

            modelBuilder.Entity("api.Features.Account", b =>
                {
                    b.Navigation("Inventories");

                    b.Navigation("NotificationReceipts");

                    b.Navigation("Sites");

                    b.Navigation("Wells");
                });

            modelBuilder.Entity("api.Features.Inventory", b =>
                {
                    b.Navigation("Wells");
                });

            modelBuilder.Entity("api.Features.Notification", b =>
                {
                    b.Navigation("NotificationReceipts");
                });

            modelBuilder.Entity("api.Features.Site", b =>
                {
                    b.Navigation("Contacts");

                    b.Navigation("Inventories");

                    b.Navigation("Wells");
                });

            modelBuilder.Entity("api.Features.Well", b =>
                {
                    b.Navigation("WaterSystemContacts");
                });
#pragma warning restore 612, 618
        }
    }
}
