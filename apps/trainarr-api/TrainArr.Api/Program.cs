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
        app.MapTrainArrFieldInboxEndpoints();
        app.MapTrainArrLoadTestJourneySeedEndpoints();
        app.MapTrainArrNotificationSettingsEndpoints();
        app.MapTrainArrRecertificationSettingsEndpoints();
        app.MapTrainArrQualificationRecalculationSettingsEndpoints();
        app.MapTrainArrStaffarrPublicationSettingsEndpoints();
        app.MapTrainArrEventProcessingSettingsEndpoints();
        app.MapTrainArrPersonTrainingHistoryEndpoints();
        app.MapTrainArrInternalTrainingNotificationEndpoints();
        app.MapTrainArrInternalStaffarrPublicationRetryEndpoints();
        app.MapTrainArrInternalTrainingEventProcessingEndpoints();

        await Task.CompletedTask;

    });


