using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BSDCPolls.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "application_users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    SupabaseUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_application_users_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_application_users_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    EntityUid = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PerformedById = table.Column<int>(type: "integer", nullable: false),
                    PerformedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_application_users_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invite_allowlist_entries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    AllowedUserId = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invite_allowlist_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invite_allowlist_entries_application_users_AllowedUserId",
                        column: x => x.AllowedUserId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_allowlist_entries_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invite_allowlist_entries_application_users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invite_allowlist_entries_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "polls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_polls_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_polls_application_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_polls_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "surveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    QuestionTree = table.Column<string>(type: "jsonb", nullable: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_surveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_surveys_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_surveys_application_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_surveys_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_privacy_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ShowPublicContent = table.Column<bool>(type: "boolean", nullable: false),
                    InvitePermission = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_privacy_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_privacy_settings_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_privacy_settings_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_privacy_settings_application_users_UserId",
                        column: x => x.UserId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "username_history",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_username_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_username_history_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_username_history_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_username_history_application_users_UserId",
                        column: x => x.UserId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_questions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    PushedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_questions_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_questions_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_questions_polls_PollId",
                        column: x => x.PollId,
                        principalTable: "polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InviterId = table.Column<int>(type: "integer", nullable: false),
                    InviteeId = table.Column<int>(type: "integer", nullable: false),
                    PollId = table.Column<int>(type: "integer", nullable: true),
                    SurveyId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invitations_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_application_users_InviteeId",
                        column: x => x.InviteeId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_application_users_InviterId",
                        column: x => x.InviterId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_polls_PollId",
                        column: x => x.PollId,
                        principalTable: "polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_responses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    RespondentId = table.Column<int>(type: "integer", nullable: false),
                    AnswersJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_survey_responses_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_survey_responses_application_users_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_survey_responses_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_survey_responses_surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_answer_options",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollQuestionId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_answer_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_answer_options_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_answer_options_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_answer_options_poll_questions_PollQuestionId",
                        column: x => x.PollQuestionId,
                        principalTable: "poll_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    InvitationId = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notifications_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_application_users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "survey_documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SurveyResponseId = table.Column<int>(type: "integer", nullable: false),
                    QuestionUid = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_survey_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_survey_documents_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_survey_documents_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_survey_documents_survey_responses_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "survey_responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollQuestionId = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "integer", nullable: false),
                    RespondentId = table.Column<int>(type: "integer", nullable: false),
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<int>(type: "integer", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poll_submissions_application_users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_submissions_application_users_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poll_submissions_application_users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "application_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_submissions_poll_answer_options_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "poll_answer_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_poll_submissions_poll_questions_PollQuestionId",
                        column: x => x.PollQuestionId,
                        principalTable: "poll_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "application_users",
                columns: new[] { "Id", "CreatedById", "CreatedOn", "IsActive", "SupabaseUserId", "Uid", "UpdatedById", "UpdatedOn", "Username" },
                values: new object[] { 1, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "SYSTEM", new Guid("00000000-0000-0000-0000-000000000001"), 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system" });

            migrationBuilder.CreateIndex(
                name: "IX_application_users_CreatedById",
                table: "application_users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_application_users_SupabaseUserId",
                table: "application_users",
                column: "SupabaseUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_users_Uid",
                table: "application_users",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_application_users_UpdatedById",
                table: "application_users",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_application_users_Username",
                table: "application_users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_PerformedById",
                table: "audit_logs",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_CreatedById",
                table: "invitations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_InviteeId_Status",
                table: "invitations",
                columns: new[] { "InviteeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_InviterId",
                table: "invitations",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_PollId",
                table: "invitations",
                column: "PollId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_SurveyId",
                table: "invitations",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_Uid",
                table: "invitations",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invitations_UpdatedById",
                table: "invitations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_invite_allowlist_entries_AllowedUserId",
                table: "invite_allowlist_entries",
                column: "AllowedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_invite_allowlist_entries_CreatedById",
                table: "invite_allowlist_entries",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_invite_allowlist_entries_OwnerId_AllowedUserId",
                table: "invite_allowlist_entries",
                columns: new[] { "OwnerId", "AllowedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_allowlist_entries_Uid",
                table: "invite_allowlist_entries",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invite_allowlist_entries_UpdatedById",
                table: "invite_allowlist_entries",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_CreatedById",
                table: "notifications",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_InvitationId",
                table: "notifications",
                column: "InvitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientId_IsRead",
                table: "notifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_Uid",
                table: "notifications",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_UpdatedById",
                table: "notifications",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_answer_options_CreatedById",
                table: "poll_answer_options",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_answer_options_PollQuestionId",
                table: "poll_answer_options",
                column: "PollQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_answer_options_Uid",
                table: "poll_answer_options",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_answer_options_UpdatedById",
                table: "poll_answer_options",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_questions_CreatedById",
                table: "poll_questions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_questions_PollId_OrderIndex",
                table: "poll_questions",
                columns: new[] { "PollId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_poll_questions_Uid",
                table: "poll_questions",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_questions_UpdatedById",
                table: "poll_questions",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_CreatedById",
                table: "poll_submissions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_PollQuestionId_RespondentId",
                table: "poll_submissions",
                columns: new[] { "PollQuestionId", "RespondentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_RespondentId",
                table: "poll_submissions",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_SelectedOptionId",
                table: "poll_submissions",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_Uid",
                table: "poll_submissions",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_submissions_UpdatedById",
                table: "poll_submissions",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_polls_CreatedById",
                table: "polls",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_polls_CreatorId_Status_IsPublic",
                table: "polls",
                columns: new[] { "CreatorId", "Status", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_polls_Uid",
                table: "polls",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_polls_UpdatedById",
                table: "polls",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_survey_documents_CreatedById",
                table: "survey_documents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_survey_documents_SurveyResponseId",
                table: "survey_documents",
                column: "SurveyResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_survey_documents_Uid",
                table: "survey_documents",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_survey_documents_UpdatedById",
                table: "survey_documents",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_CreatedById",
                table: "survey_responses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_RespondentId",
                table: "survey_responses",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_SurveyId_RespondentId",
                table: "survey_responses",
                columns: new[] { "SurveyId", "RespondentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_Uid",
                table: "survey_responses",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_survey_responses_UpdatedById",
                table: "survey_responses",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_surveys_CreatedById",
                table: "surveys",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_surveys_CreatorId_Status_IsPublic",
                table: "surveys",
                columns: new[] { "CreatorId", "Status", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_surveys_Uid",
                table: "surveys",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_surveys_UpdatedById",
                table: "surveys",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_user_privacy_settings_CreatedById",
                table: "user_privacy_settings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_user_privacy_settings_Uid",
                table: "user_privacy_settings",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_privacy_settings_UpdatedById",
                table: "user_privacy_settings",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_user_privacy_settings_UserId",
                table: "user_privacy_settings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_username_history_CreatedById",
                table: "username_history",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_username_history_Uid",
                table: "username_history",
                column: "Uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_username_history_UpdatedById",
                table: "username_history",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_username_history_UserId",
                table: "username_history",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "invite_allowlist_entries");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "poll_submissions");

            migrationBuilder.DropTable(
                name: "survey_documents");

            migrationBuilder.DropTable(
                name: "user_privacy_settings");

            migrationBuilder.DropTable(
                name: "username_history");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "poll_answer_options");

            migrationBuilder.DropTable(
                name: "survey_responses");

            migrationBuilder.DropTable(
                name: "poll_questions");

            migrationBuilder.DropTable(
                name: "surveys");

            migrationBuilder.DropTable(
                name: "polls");

            migrationBuilder.DropTable(
                name: "application_users");
        }
    }
}
