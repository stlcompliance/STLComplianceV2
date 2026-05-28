using TrainArr.Api;

using TrainArr.Api.Data;

using TrainArr.Api.Endpoints;

using STLCompliance.Shared.Endpoints;
using STLCompliance.Shared.Hosting;



await StlApiHost.RunAsync<TrainArrDbContext>(

    new ProductDescriptor("trainarr", "TrainArr", 5103),

    args,

    TrainArrServiceRegistration.ConfigureServices,

    TrainArrServiceRegistration.ConfigurePipeline,

    async app =>

    {

        app.MapTrainArrAuthEndpoints();

        app.MapStlProductLaunchEndpoints();

        app.MapTrainArrTrainingDefinitionEndpoints();

        app.MapTrainArrTrainingProgramEndpoints();

        app.MapTrainArrTrainingAssignmentEndpoints();

        app.MapTrainArrTrainingAssignmentMaterialDemandEndpoints();

        app.MapTrainArrTrainingEvidenceEndpoints();

        app.MapTrainArrTrainingEvaluationEndpoints();

        app.MapTrainArrTrainingSignoffEndpoints();

        app.MapTrainArrIncidentRemediationEndpoints();

        app.MapTrainArrCertificationPublicationEndpoints();

        app.MapTrainArrQualificationIssueEndpoints();

        app.MapTrainArrQualificationCheckEndpoints();

        app.MapTrainArrTrainingCitationEndpoints();

        app.MapTrainArrTrainingRulePackRequirementEndpoints();

        app.MapTrainArrRulePackImpactEndpoints();

        app.MapTrainArrIntegrationEndpoints();

        app.MapTrainArrInternalQualificationExpirationEndpoints();
        app.MapTrainArrInternalRecertificationAssignmentEndpoints();
        app.MapTrainArrInternalQualificationRecalculationEndpoints();
        app.MapTrainArrInternalRulePackImpactEndpoints();
        app.MapTrainArrFieldInboxEndpoints();
        app.MapTrainArrLoadTestJourneySeedEndpoints();
        app.MapTrainArrNotificationSettingsEndpoints();
        app.MapTrainArrAssignmentDueReminderSettingsEndpoints();
        app.MapTrainArrAssignmentEscalationSettingsEndpoints();
        app.MapTrainArrRecertificationSettingsEndpoints();
        app.MapTrainArrQualificationRecalculationSettingsEndpoints();
        app.MapTrainArrRulePackImpactSettingsEndpoints();
        app.MapTrainArrEvidenceRetentionSettingsEndpoints();
        app.MapTrainArrOrphanReferenceSettingsEndpoints();
        app.MapTrainArrStaffarrPublicationSettingsEndpoints();
        app.MapTrainArrInternalEvidenceRetentionEndpoints();
        app.MapTrainArrInternalOrphanReferenceEndpoints();
        app.MapTrainArrEventProcessingSettingsEndpoints();
        app.MapTrainArrPersonTrainingHistoryEndpoints();
        app.MapTrainArrInternalTrainingNotificationEndpoints();
        app.MapTrainArrInternalAssignmentDueReminderEndpoints();
        app.MapTrainArrInternalAssignmentEscalationEndpoints();
        app.MapTrainArrInternalStaffarrPublicationRetryEndpoints();
        app.MapTrainArrInternalTrainingEventProcessingEndpoints();
        app.MapTrainArrAuditPackageEndpoints();
        app.MapTrainArrInternalAuditPackageGenerationEndpoints();
        app.MapTrainArrIntegrationSettingsEndpoints();

        await Task.CompletedTask;

    });


