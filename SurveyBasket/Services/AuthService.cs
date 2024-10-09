using Microsoft.AspNetCore.Identity;
using SurveyBasket.Authentication;
using System.Security.Cryptography;

namespace SurveyBasket.Services;

public class AuthService(UserManager<ApplicationUser> userManager, IJwtProvider jwtProvider) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtProvider _jwtProvider = jwtProvider;

    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {

        // check user exists?
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return null;

        // check correct password
        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!isValidPassword)
            return null;

        // generate Jwt token
        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        //generate refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        // save to database
        user.refreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExiration
        });
        await _userManager.UpdateAsync(user);

        return new AuthResponse(Guid.NewGuid().ToString(), user.Email,user.FirstName, user.LastName, token, expiresIn, refreshToken, refreshTokenExiration);
    }  

    public async Task<AuthResponse?> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) 
            return null;

        var userRefreshToken = user.refreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null) 
            return null;

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        // generate new Jwt token
        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);

        //generate new refresh token
        var newRefreshToken = GenerateRefreshToken();
        var refreshTokenExiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        // save to database
        user.refreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExiration
        });
        await _userManager.UpdateAsync(user);


        return new AuthResponse(Guid.NewGuid().ToString(), user.Email, user.FirstName, user.LastName, newToken, expiresIn, newRefreshToken, refreshTokenExiration);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return false;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return false;

        var userRefreshToken = user.refreshTokens.SingleOrDefault(x => x.Token == refreshToken && x.IsActive);

        if (userRefreshToken is null)
            return false;

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        
        return true;
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

   
}