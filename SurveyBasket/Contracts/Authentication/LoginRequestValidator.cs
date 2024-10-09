namespace SurveyBasket.Contracts.Authentication;

public class LoginRequestValidator : AbstractValidator<LoginReqest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}