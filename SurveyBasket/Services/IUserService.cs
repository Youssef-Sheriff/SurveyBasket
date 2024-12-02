﻿using SurveyBasket.Contracts.Users;

namespace SurveyBasket.Services;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);

}