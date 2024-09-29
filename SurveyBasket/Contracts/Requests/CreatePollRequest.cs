using System.ComponentModel.DataAnnotations;

namespace SurveyBasket.Contracts.Requests;

public record CreatePollRequest(

    string Title,
    string Description
    );

