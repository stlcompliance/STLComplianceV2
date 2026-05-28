using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class TrainingAcknowledgementPublicationService(StaffArrTrainingAcknowledgementClient staffArrClient)
{
    public const string StatusPending = "pending";
    public const string StatusAcknowledged = "acknowledged";
    public const string StatusSuperseded = "superseded";

    public async Task PublishForAssignmentAsync(
        TrainingAssignment assignment,
        string trainingTitle,
        CancellationToken cancellationToken = default)
    {
        var requestId = assignment.Id;
        var summary =
            $"Acknowledge receipt of the training assignment \"{trainingTitle}\" before uploading evidence or continuing.";
        await staffArrClient.IngestAsync(
            new StaffArrIngestTrainingAcknowledgementPayload(
                assignment.TenantId,
                assignment.StaffarrPersonId,
                requestId,
                assignment.Id,
                trainingTitle,
                assignment.AssignmentReason,
                summary,
                assignment.DueAt),
            cancellationToken);

        assignment.StaffarrAcknowledgementRequestId = requestId;
        assignment.StaffarrAcknowledgementStatus = StatusPending;
        assignment.StaffarrAcknowledgementAt = null;
    }

    public async Task SyncMirrorFromStaffArrAsync(
        TrainingAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        if (assignment.StaffarrAcknowledgementRequestId is not Guid requestId)
        {
            return;
        }

        var status = await staffArrClient.GetStatusAsync(assignment.TenantId, requestId, cancellationToken);
        if (status is null)
        {
            return;
        }

        assignment.StaffarrAcknowledgementStatus = status.Status;
        assignment.StaffarrAcknowledgementAt = status.AcknowledgedAt;
    }

    public async Task SupersedeIfOpenAsync(
        TrainingAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        if (assignment.StaffarrAcknowledgementRequestId is not Guid requestId)
        {
            return;
        }

        if (string.Equals(assignment.StaffarrAcknowledgementStatus, StatusAcknowledged, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await staffArrClient.SupersedeAsync(
            new StaffArrSupersedeTrainingAcknowledgementPayload(
                assignment.TenantId,
                assignment.StaffarrPersonId,
                requestId),
            cancellationToken);

        assignment.StaffarrAcknowledgementStatus = StatusSuperseded;
    }

    public static bool RequiresAcknowledgement(TrainingAssignment assignment) =>
        assignment.StaffarrAcknowledgementRequestId is not null
        && !string.Equals(assignment.StaffarrAcknowledgementStatus, StatusAcknowledged, StringComparison.OrdinalIgnoreCase);
}
