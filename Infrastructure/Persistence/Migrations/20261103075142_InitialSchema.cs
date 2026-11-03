using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Munaqasat.Web.Domain;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Munaqasat.Web.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:request_status", "PENDING,ACCEPTED,REJECTED,WITHDRAWN")
                .Annotation("Npgsql:Enum:text_direction", "ltr,rtl")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "identity_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "identity_users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    native_name = table.Column<string>(type: "text", nullable: false),
                    direction = table.Column<TextDirection>(type: "text_direction", nullable: false, defaultValue: TextDirection.Ltr),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_languages", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "identity_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_role_claims_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_identity_user_claims_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_identity_user_logins_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "identity_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_identity_user_roles_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identity_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_identity_user_tokens_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    company_profile_id = table.Column<string>(type: "text", nullable: true),
                    company_number = table.Column<string>(type: "text", nullable: true),
                    company_website = table.Column<string>(type: "text", nullable: true),
                    company_email = table.Column<string>(type: "text", nullable: false),
                    sectors = table.Column<List<string>>(type: "text[]", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    profile_image = table.Column<string>(type: "text", nullable: true),
                    default_language = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                    avg_communication = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    avg_deadline = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    avg_experience = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    avg_overall_rating = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    avg_quality = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    number_of_ratings = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    verification_status = table.Column<string>(type: "text", nullable: false, defaultValue: "PENDING"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.CheckConstraint("ck_companies_verification_status", "verification_status IN ('PENDING','VERIFIED')");
                    table.ForeignKey(
                        name: "fk_companies_languages_default_language",
                        column: x => x.default_language,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    external_tender_id = table.Column<string>(type: "text", nullable: false),
                    tender_no = table.Column<string>(type: "text", nullable: false),
                    procurement_category_code = table.Column<string>(type: "text", nullable: false),
                    tender_type_code = table.Column<string>(type: "text", nullable: false),
                    evaluation_type_code = table.Column<string>(type: "text", nullable: true),
                    envelope_type_code = table.Column<string>(type: "text", nullable: true),
                    is_alternate_bid_allowed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    vendor_grades = table.Column<List<string>>(type: "text[]", nullable: true),
                    prebid_clarification_mode = table.Column<string>(type: "text", nullable: true),
                    document_fee = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    document_fee_payment_mode = table.Column<string>(type: "text", nullable: true),
                    bond_percentage = table.Column<decimal>(type: "numeric(6,3)", nullable: true),
                    is_e_bond_applicable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    original_language = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                    published_date = table.Column<DateOnly>(type: "date", nullable: false),
                    document_sales_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    document_sales_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prebid_clarification_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    prebid_clarification_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    bid_submission_closing = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    priced_bid_opening = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    budget_min = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    budget_max = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "PUBLISHED"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenders", x => x.id);
                    table.CheckConstraint("ck_tenders_bond_percentage", "bond_percentage IS NULL OR bond_percentage BETWEEN 0 AND 100");
                    table.CheckConstraint("ck_tenders_budget", "budget_min IS NULL OR budget_max IS NULL OR budget_min <= budget_max");
                    table.CheckConstraint("ck_tenders_category", "procurement_category_code IN ('SUPPLY','SERVICES','WORKS')");
                    table.CheckConstraint("ck_tenders_document_fee", "document_fee IS NULL OR document_fee >= 0");
                    table.CheckConstraint("ck_tenders_status", "status IN ('DRAFT','PUBLISHED')");
                    table.ForeignKey(
                        name: "fk_tenders_languages_original_language",
                        column: x => x.original_language,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ui_translations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    @namespace = table.Column<string>(name: "namespace", type: "text", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ui_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_ui_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "company_translations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    company_title = table.Column<string>(type: "text", nullable: false),
                    bio = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_translations_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "custom_sections",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    company_profile_id = table.Column<string>(type: "text", nullable: false),
                    file_url = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_sections_companies_company_profile_id",
                        column: x => x.company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    bio = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    profile_image = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<string>(type: "text", nullable: true),
                    company_that_worked_with = table.Column<string>(type: "text", nullable: true),
                    identity_user_id = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<string>(type: "text", nullable: true),
                    preferred_language = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                    timezone = table.Column<string>(type: "text", nullable: false, defaultValue: "Asia/Muscat"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_profiles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_profiles_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_profiles_languages_preferred_language",
                        column: x => x.preferred_language,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "custom_fields",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tender_id = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_fields", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_fields_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tender_requests",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tender_id = table.Column<string>(type: "text", nullable: false),
                    company_profile_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<RequestStatus>(type: "request_status", nullable: false, defaultValue: RequestStatus.Pending),
                    price = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    days = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tender_requests", x => x.id);
                    table.CheckConstraint("ck_tender_requests_days", "days IS NULL OR days > 0");
                    table.CheckConstraint("ck_tender_requests_price", "price IS NULL OR price >= 0");
                    table.ForeignKey(
                        name: "fk_tender_requests_companies_company_profile_id",
                        column: x => x.company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tender_requests_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_tender_requests_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tender_translations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tender_id = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    entity_name = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    procurement_category = table.Column<string>(type: "text", nullable: true),
                    procurement_sub_category = table.Column<string>(type: "text", nullable: true),
                    governorate = table.Column<string>(type: "text", nullable: true),
                    wilayat = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    is_machine_translated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tender_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_tender_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tender_translations_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_section_translations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    custom_section_id = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    tab_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_section_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_section_translations_custom_sections_custom_section_",
                        column: x => x.custom_section_id,
                        principalTable: "custom_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_section_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    @params = table.Column<JsonDocument>(name: "params", type: "jsonb", nullable: true),
                    tender_id = table.Column<string>(type: "text", nullable: true),
                    actor_id = table.Column<string>(type: "text", nullable: true),
                    read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_user_profiles_actor_id",
                        column: x => x.actor_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_user_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_field_translations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    custom_field_id = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_field_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_field_translations_custom_fields_custom_field_id",
                        column: x => x.custom_field_id,
                        principalTable: "custom_fields",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_field_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chat_rooms",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    tender_id = table.Column<string>(type: "text", nullable: true),
                    tender_request_id = table.Column<string>(type: "text", nullable: true),
                    sender_company_profile_id = table.Column<string>(type: "text", nullable: false),
                    receiver_company_profile_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_rooms_companies_receiver_company_profile_id",
                        column: x => x.receiver_company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chat_rooms_companies_sender_company_profile_id",
                        column: x => x.sender_company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_chat_rooms_tender_requests_tender_request_id",
                        column: x => x.tender_request_id,
                        principalTable: "tender_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_chat_rooms_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "company_ratings",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    rating_company_id = table.Column<string>(type: "text", nullable: false),
                    rated_company_id = table.Column<string>(type: "text", nullable: false),
                    tender_id = table.Column<string>(type: "text", nullable: true),
                    tender_request_id = table.Column<string>(type: "text", nullable: false),
                    quality = table.Column<int>(type: "integer", nullable: false),
                    communication = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<int>(type: "integer", nullable: false),
                    deadline = table.Column<int>(type: "integer", nullable: false),
                    overall_rating = table.Column<double>(type: "double precision", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    language_code = table.Column<string>(type: "text", nullable: true),
                    anonymous = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ratings", x => x.id);
                    table.CheckConstraint("ck_company_ratings_communication", "communication BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_company_ratings_deadline", "deadline BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_company_ratings_experience", "experience BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_company_ratings_not_self", "rating_company_id <> rated_company_id");
                    table.CheckConstraint("ck_company_ratings_overall", "overall_rating BETWEEN 1 AND 5");
                    table.CheckConstraint("ck_company_ratings_quality", "quality BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_company_ratings_companies_rated_company_id",
                        column: x => x.rated_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_ratings_companies_rating_company_id",
                        column: x => x.rating_company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_ratings_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_company_ratings_tender_requests_tender_request_id",
                        column: x => x.tender_request_id,
                        principalTable: "tender_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_company_ratings_tenders_tender_id",
                        column: x => x.tender_id,
                        principalTable: "tenders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    chat_room_id = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    language_code = table.Column<string>(type: "text", nullable: true),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    read_status = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sender_company_profile_id = table.Column<string>(type: "text", nullable: false),
                    receiver_company_profile_id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_messages_chat_rooms_chat_room_id",
                        column: x => x.chat_room_id,
                        principalTable: "chat_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_companies_receiver_company_profile_id",
                        column: x => x.receiver_company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_messages_companies_sender_company_profile_id",
                        column: x => x.sender_company_profile_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_messages_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_chat_rooms_receiver_company_profile_id",
                table: "chat_rooms",
                column: "receiver_company_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_rooms_sender_company_profile_id_receiver_company_profi",
                table: "chat_rooms",
                columns: new[] { "sender_company_profile_id", "receiver_company_profile_id" });

            migrationBuilder.CreateIndex(
                name: "ix_chat_rooms_tender_id",
                table: "chat_rooms",
                column: "tender_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_rooms_tender_request_id",
                table: "chat_rooms",
                column: "tender_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_avg_overall_rating",
                table: "companies",
                column: "avg_overall_rating");

            migrationBuilder.CreateIndex(
                name: "ix_companies_company_email",
                table: "companies",
                column: "company_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companies_default_language",
                table: "companies",
                column: "default_language");

            migrationBuilder.CreateIndex(
                name: "ix_companies_sectors",
                table: "companies",
                column: "sectors")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_company_ratings_language_code",
                table: "company_ratings",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_company_ratings_rated_company_id",
                table: "company_ratings",
                column: "rated_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ratings_rating_company_id_tender_request_id",
                table: "company_ratings",
                columns: new[] { "rating_company_id", "tender_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_ratings_tender_id",
                table: "company_ratings",
                column: "tender_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ratings_tender_request_id",
                table: "company_ratings",
                column: "tender_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_translations_company_id_language_code",
                table: "company_translations",
                columns: new[] { "company_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_translations_language_code",
                table: "company_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_company_translations_title_trgm",
                table: "company_translations",
                column: "company_title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_translations_custom_field_id_language_code",
                table: "custom_field_translations",
                columns: new[] { "custom_field_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_field_translations_language_code",
                table: "custom_field_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_custom_fields_tender_id_sort_order",
                table: "custom_fields",
                columns: new[] { "tender_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_custom_section_translations_custom_section_id_language_code",
                table: "custom_section_translations",
                columns: new[] { "custom_section_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_section_translations_language_code",
                table: "custom_section_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_custom_sections_company_profile_id_sort_order",
                table: "custom_sections",
                columns: new[] { "company_profile_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_identity_role_claims_role_id",
                table: "identity_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "identity_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_claims_user_id",
                table: "identity_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_logins_user_id",
                table: "identity_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_identity_user_roles_role_id",
                table: "identity_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "identity_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "identity_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_languages_single_default",
                table: "languages",
                column: "is_default",
                unique: true,
                filter: "is_default");

            migrationBuilder.CreateIndex(
                name: "ix_messages_chat_room_id_created_at",
                table: "messages",
                columns: new[] { "chat_room_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_messages_language_code",
                table: "messages",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_messages_receiver_company_profile_id",
                table: "messages",
                column: "receiver_company_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_sender_company_profile_id",
                table: "messages",
                column: "sender_company_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_actor_id",
                table: "notifications",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_tender_id",
                table: "notifications",
                column: "tender_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_created_at1",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                filter: "NOT read");

            migrationBuilder.CreateIndex(
                name: "ix_tender_requests_company_profile_id_status",
                table: "tender_requests",
                columns: new[] { "company_profile_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_tender_requests_language_code",
                table: "tender_requests",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_tender_requests_tender_id_company_profile_id",
                table: "tender_requests",
                columns: new[] { "tender_id", "company_profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tender_requests_tender_id_status",
                table: "tender_requests",
                columns: new[] { "tender_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_tender_translations_governorate_wilayat",
                table: "tender_translations",
                columns: new[] { "governorate", "wilayat" });

            migrationBuilder.CreateIndex(
                name: "ix_tender_translations_language_code",
                table: "tender_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_tender_translations_tender_id_language_code",
                table: "tender_translations",
                columns: new[] { "tender_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tender_translations_title_trgm",
                table: "tender_translations",
                column: "title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_tenders_bid_submission_closing",
                table: "tenders",
                column: "bid_submission_closing");

            migrationBuilder.CreateIndex(
                name: "ix_tenders_external_tender_id",
                table: "tenders",
                column: "external_tender_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenders_original_language",
                table: "tenders",
                column: "original_language");

            migrationBuilder.CreateIndex(
                name: "ix_tenders_procurement_category_code",
                table: "tenders",
                column: "procurement_category_code");

            migrationBuilder.CreateIndex(
                name: "ix_tenders_published_date",
                table: "tenders",
                column: "published_date");

            migrationBuilder.CreateIndex(
                name: "ix_tenders_vendor_grades",
                table: "tenders",
                column: "vendor_grades")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_ui_translations_language_code",
                table: "ui_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_ui_translations_namespace_key_language_code",
                table: "ui_translations",
                columns: new[] { "namespace", "key", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_identity_user_id",
                table: "user_profiles",
                column: "identity_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_company_id",
                table: "user_profiles",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_email",
                table: "user_profiles",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_preferred_language",
                table: "user_profiles",
                column: "preferred_language");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_ratings");

            migrationBuilder.DropTable(
                name: "company_translations");

            migrationBuilder.DropTable(
                name: "custom_field_translations");

            migrationBuilder.DropTable(
                name: "custom_section_translations");

            migrationBuilder.DropTable(
                name: "identity_role_claims");

            migrationBuilder.DropTable(
                name: "identity_user_claims");

            migrationBuilder.DropTable(
                name: "identity_user_logins");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_tokens");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "tender_translations");

            migrationBuilder.DropTable(
                name: "ui_translations");

            migrationBuilder.DropTable(
                name: "custom_fields");

            migrationBuilder.DropTable(
                name: "custom_sections");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "identity_users");

            migrationBuilder.DropTable(
                name: "chat_rooms");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "tender_requests");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "tenders");

            migrationBuilder.DropTable(
                name: "languages");

            // Npgsql does not generate these drops, which would make a full rollback non-repeatable.
            migrationBuilder.Sql("DROP TYPE IF EXISTS request_status;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS text_direction;");
            migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
        }
    }
}
