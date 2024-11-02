﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using SurveyBasket.Contracts.Questions;
using SurveyBasket.Entities;
using System.Collections.Generic;

namespace SurveyBasket.Services;

public class QuestionService(ApplicationDbContext context, ICacheService cacheService, ILogger<QuestionService> logger) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ILogger<QuestionService> _logger = logger;

    private const string _cachePrefix = "availableQuestions";

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
            .Where(x => x.PollId == pollId)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        //var hasVote = await _context.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);

        //if (hasVote)
        //    return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DupplicatedVote);

        //var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId && x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        //if (!pollIsExists)
        //    return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var cahcheKey = $"{_cachePrefix}-{pollId}";

        var cachedQuestions = await _cacheService.GetAsync<IEnumerable<QuestionResponse>>(cahcheKey, cancellationToken);

        IEnumerable<QuestionResponse> questions = [];

        if (cachedQuestions is null)
        {
            _logger.LogInformation("Select questions from database");

            questions = await _context.Questions
                .Where(x => x.PollId == pollId && x.IsActive)
                .Include(x => x.Answers)
                .Select(q => new QuestionResponse(
                    q.Id,
                    q.Content,
                    q.Answers.Where(a => a.IsActive).Select(a => new Contracts.Answers.AnswerResponse(a.Id, a.Content))
                ))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            await _cacheService.SetAsync(cahcheKey, questions, cancellationToken);
        }
        else
        {
            _logger.LogInformation("get questions from cache");
            questions = cachedQuestions;
        }

        //var questions = await _memoryCache.GetOrCreateAsync(
        //    cahcheKey,
        //    cacheEntry =>
        //    {
        //        cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(5);
        //        return _context.Questions
        //        .Where(x => x.PollId == pollId && x.IsActive)
        //        .Include(x => x.Answers)
        //        .Select(q => new QuestionResponse(
        //            q.Id,
        //            q.Content,
        //            q.Answers.Where(a => a.IsActive).Select(a => new Contracts.Answers.AnswerResponse(a.Id, a.Content))
        //        ))
        //        .AsNoTracking()
        //        .ToListAsync(cancellationToken);
        //    }
        //);

        return Result.Success<IEnumerable<QuestionResponse>>(questions!);
    }

    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {

        var question = await _context.Questions
            .Where(x => x.PollId == pollId && x.Id == id)
            .Include(x => x.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        return Result.Success(question);
    }

    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var questionIsExists = await _context.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken);

        if (questionIsExists)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();

        question.PollId = pollId;

        await _context.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());
    }

    public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var questionIsExists = await _context.Questions
            .AnyAsync(x => x.PollId == pollId
            && x.Id != id
            && x.Content == request.Content,
            cancellationToken
           );

        if (questionIsExists)
            return Result.Failure(QuestionErrors.DuplicatedQuestionContent);

        var question = await _context.Questions
            .Include(x => x.Answers)
            .SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.Content = request.Content;

        var currentAnswers = question.Answers.Select(x => x.Content).ToList();

        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        // add new answers
        newAnswers.ForEach(answer =>
        {
            question.Answers.Add(new Answer { Content = answer });
        });

        // deactivate removed answers
        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);


        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions.SingleOrDefaultAsync(x => x.PollId == pollId && x.Id == id, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"{_cachePrefix}-{pollId}", cancellationToken);

        return Result.Success();
    }
}