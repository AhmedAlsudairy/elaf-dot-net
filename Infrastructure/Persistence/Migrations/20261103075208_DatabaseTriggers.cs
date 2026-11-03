using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munaqasat.Web.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// What EF cannot model (the ERD's tender_platform_extras.sql): an updated_at trigger on every table
    /// that has the column, and the trigger that keeps the cached rating averages on companies current.
    /// </summary>
    public partial class DatabaseTriggers : Migration
    {
        private static readonly string[] UpdatedAtTables =
        {
            "ui_translations", "tenders", "tender_translations", "custom_fields", "tender_requests",
            "companies", "company_translations", "custom_sections", "user_profiles", "chat_rooms", "messages"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_updated_at() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    NEW.updated_at = now();
                    RETURN NEW;
                END;
                $$;
                """);

            foreach (var table in UpdatedAtTables)
            {
                migrationBuilder.Sql($"""
                    CREATE TRIGGER trg_{table}_updated_at
                    BEFORE UPDATE ON {table}
                    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
                    """);
            }

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION refresh_company_rating(p_company_id text) RETURNS void
                LANGUAGE sql AS $$
                    UPDATE companies c
                    SET avg_quality        = COALESCE(s.quality, 0),
                        avg_communication  = COALESCE(s.communication, 0),
                        avg_experience     = COALESCE(s.experience, 0),
                        avg_deadline       = COALESCE(s.deadline, 0),
                        avg_overall_rating = COALESCE(s.overall, 0),
                        number_of_ratings  = s.n
                    FROM (
                        SELECT AVG(quality)::float8        AS quality,
                               AVG(communication)::float8  AS communication,
                               AVG(experience)::float8     AS experience,
                               AVG(deadline)::float8       AS deadline,
                               AVG(overall_rating)::float8 AS overall,
                               COUNT(*)::int               AS n
                        FROM company_ratings
                        WHERE rated_company_id = p_company_id
                    ) s
                    WHERE c.id = p_company_id;
                $$;

                CREATE OR REPLACE FUNCTION company_ratings_aggregate() RETURNS trigger
                LANGUAGE plpgsql AS $$
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        PERFORM refresh_company_rating(NEW.rated_company_id);
                    ELSIF TG_OP = 'DELETE' THEN
                        PERFORM refresh_company_rating(OLD.rated_company_id);
                    ELSE
                        PERFORM refresh_company_rating(NEW.rated_company_id);
                        IF OLD.rated_company_id IS DISTINCT FROM NEW.rated_company_id THEN
                            PERFORM refresh_company_rating(OLD.rated_company_id);
                        END IF;
                    END IF;
                    RETURN NULL;
                END;
                $$;

                CREATE TRIGGER trg_company_ratings_aggregate
                AFTER INSERT OR UPDATE OR DELETE ON company_ratings
                FOR EACH ROW EXECUTE FUNCTION company_ratings_aggregate();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_company_ratings_aggregate ON company_ratings;
                DROP FUNCTION IF EXISTS company_ratings_aggregate();
                DROP FUNCTION IF EXISTS refresh_company_rating(text);
                """);

            foreach (var table in UpdatedAtTables)
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS trg_{table}_updated_at ON {table};");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_updated_at();");
        }
    }
}
