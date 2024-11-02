namespace SurveyBasket.Errors;

public static class PollErrors
{
    public static readonly Error PollNotFound =
        new ("Poll.NotFound", "No poll was found with given ID", StatusCodes.Status404NotFound);

    public static readonly Error PollTitleDuplicated =
        new("Poll.TitleDuplicated", "A poll with this title already exists", StatusCodes.Status409Conflict);
}
