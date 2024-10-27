namespace SurveyBasket.Errors;

public static class VoteErrors
{
     public static readonly Error DupplicatedVote =
        new("Vote.duplicated", "You have already voted for this poll.", StatusCodes.Status409Conflict);

}