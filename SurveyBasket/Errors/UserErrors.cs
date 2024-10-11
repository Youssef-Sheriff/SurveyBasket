namespace SurveyBasket.Errors;

public class UserErrors
{
    public static readonly Error InvalidCredentials =
        new Error("User.InvalidCredentials", "Invalid email/password");
}
