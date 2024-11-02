namespace SurveyBasket.Errors;

public static class VoteErrors
{
    public static readonly Error InvalidQuestions =
        new("Vote.InvalidQuestion", "Invalid question id.", StatusCodes.Status400BadRequest);

    public static readonly Error DupplicatedVote =
        new("Vote.Duplicated", "You have already voted for this poll.", StatusCodes.Status409Conflict);
}