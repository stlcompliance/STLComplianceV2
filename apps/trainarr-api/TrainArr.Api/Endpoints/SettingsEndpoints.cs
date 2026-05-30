using TrainArr.Api.Contracts;
using TrainArr.Api.Services;

namespace TrainArr.Api.Endpoints;

public static class SettingsEndpoints
{
    public static void MapTrainArrSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/settings"),
            app.MapGroup("/api/v1/settings"),
            app.MapGroup("/api/config"),
            app.MapGroup("/api/v1/config"),
        };

        foreach (var group in groups)
        {
            group.WithTags("Settings").RequireAuthorization();

            group.MapGet("/", (
                TrainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireNotificationSettingsManage(context.User);

                var response = new TrainArrSettingsManifestResponse(
                [
                    new("notification_settings", "/api/v1/notification-settings", "Training notification dispatch configuration."),
                    new("assignment_due_reminder_settings", "/api/v1/assignment-due-reminder-settings", "Assignment due reminder worker settings."),
                    new("assignment_escalation_settings", "/api/v1/assignment-escalation-settings", "Assignment escalation worker settings."),
                    new("recertification_settings", "/api/v1/recertification-settings", "Recertification assignment generation settings."),
                    new("qualification_recalculation_settings", "/api/v1/qualification-recalculation-settings", "Qualification recalculation worker controls."),
                    new("rule_pack_impact_settings", "/api/v1/rule-pack-impact-settings", "Rule-pack impact worker settings."),
                    new("evidence_retention_settings", "/api/v1/evidence-retention-settings", "Evidence retention worker settings."),
                    new("orphan_reference_settings", "/api/v1/orphan-reference-settings", "Orphan reference scan and cleanup settings."),
                    new("staffarr_publication_settings", "/api/v1/staffarr-publication-settings", "StaffArr publication retry worker settings."),
                    new("event_processing_settings", "/api/v1/event-processing-settings", "Training event processing worker settings."),
                    new("integration_settings", "/api/v1/integration-settings", "TrainArr integration service endpoints and token settings."),
                ]);

                return Results.Ok(response);
            });
        }
    }
}
