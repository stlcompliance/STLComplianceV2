using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public static class TrainingCompletionRequirements
{
    public static readonly HashSet<string> AllowedEvaluationResults = new(StringComparer.OrdinalIgnoreCase)
    {
        "pass",
        "fail",
        "incomplete"
    };

    public static readonly HashSet<string> AllowedSignoffRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "trainee",
        "trainer"
    };

    public static bool AreMet(TrainingAssignment assignment) =>
        HasPassingEvaluation(assignment)
        && HasSignoffRole(assignment, "trainee")
        && HasSignoffRole(assignment, "trainer");

    public static bool HasPassingEvaluation(TrainingAssignment assignment) =>
        assignment.Evaluation is not null
        && string.Equals(assignment.Evaluation.Result, "pass", StringComparison.OrdinalIgnoreCase);

    public static bool HasSignoffRole(TrainingAssignment assignment, string signoffRole) =>
        assignment.Signoffs.Any(x =>
            string.Equals(x.SignoffRole, signoffRole, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> MissingRequirements(TrainingAssignment assignment)
    {
        var missing = new List<string>();
        if (!HasPassingEvaluation(assignment))
        {
            missing.Add("passing_evaluation");
        }

        if (!HasSignoffRole(assignment, "trainee"))
        {
            missing.Add("trainee_signoff");
        }

        if (!HasSignoffRole(assignment, "trainer"))
        {
            missing.Add("trainer_signoff");
        }

        return missing;
    }
}
