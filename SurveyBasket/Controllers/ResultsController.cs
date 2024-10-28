﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SurveyBasket.Controllers;
[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class ResultsController(IResultService resultService) : ControllerBase
{
    private readonly IResultService _resultService = resultService;

    [HttpGet("row-data")]
    public async Task<IActionResult> PollVotes([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var result = await _resultService.GetPollVotesAsync(pollId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}