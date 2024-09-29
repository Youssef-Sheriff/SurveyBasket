using SurveyBasket.Contracts.Responses;

namespace SurveyBasket.Models;

public class Poll
{
    public int Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;

}

