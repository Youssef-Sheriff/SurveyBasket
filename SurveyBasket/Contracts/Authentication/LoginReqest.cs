namespace SurveyBasket.Contracts.Authentication;

public record LoginReqest(
    string Email,
    string Password
);