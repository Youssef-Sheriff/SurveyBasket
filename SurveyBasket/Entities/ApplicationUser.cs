using Microsoft.AspNetCore.Identity;

namespace SurveyBasket.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public List<RefreshToken> refreshTokens { get; set; } = [];
}
