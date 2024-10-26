namespace SurveyBasket.Contracts.Questions;

public class QuestionValidator : AbstractValidator<QuestionRequest>
{
    public QuestionValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(3, 1000);

        RuleFor(x => x.Answers)
            .NotNull();

        RuleFor(x => x.Answers)
            .Must(x => x.Count > 1)
            .WithMessage("Question should has at least 2 answers.")
            .When(x => x.Answers != null);


        RuleFor(x => x.Answers)
             .Must(x => x.Distinct().Count() == x.Count)
             .WithMessage("Can't add duplicated answer for the same question")
                         .When(x => x.Answers != null);

    }
}
