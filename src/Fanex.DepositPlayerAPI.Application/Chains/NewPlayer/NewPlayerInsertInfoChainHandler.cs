namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using System.Text;
using Common.Interfaces;
using Domain.Entities;
using Domain.Errors;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;

public class NewPlayerInsertInfoChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    private const int RetryLimit = 3;

    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInsertInfoChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        ServiceResult<InsertPlayerInfoOutputParam> result = null;
        var extraInfo = new StringBuilder();
        var canRetry = true;

        var param = new InsertPlayerInfoParam
        {
            Username2 = request.Username2,
            Recommend = request.Recommend,
            SiteDbGroupId = request.SiteDbGroupId,
            Site = request.Site,
            CurrencyName = request.CurrencyName,
            RetryNumber = 0
        };

        while (canRetry)
        {
            (canRetry, result) = await InsertNewPlayerInfo(param, extraInfo);
        }

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            _ => ServiceResult<Player>.Success(null, extraInfo.ToString()),
            (error, _) => ServiceResult<Player>.Failure(error, extraInfo.ToString()));
    }

    public override void HandleError(NewPlayerChainParam request)
    {
        LogAggregate.AppendLog("[NewPlayer] - Run Error Handler Task: RollbackInfoNewPlayer");

        backgroundTask.RollbackInfoNewPlayer(ChainName, request.CustId);
    }

    private static void UpdateRequest(InsertPlayerInfoOutputParam output, NewPlayerChainParam request)
    {
        request.Username = output.Username;
        request.CustId = output.CustId;
    }

    private async Task<(bool canRetry, ServiceResult<InsertPlayerInfoOutputParam> result)> InsertNewPlayerInfo(
        InsertPlayerInfoParam param,
        StringBuilder extraInfo)
    {
        var result = await playerRepository.InsertPlayerInfoAsync(param);
        // Add extra info
        extraInfo.AppendLine($"\n--- Retry {param.RetryNumber} ---\n");
        extraInfo.AppendLine($"1. Generate:\n{result.ExtraInfo}");

        if (result.IsFailure)
        {
            return (false, ServiceResult<InsertPlayerInfoOutputParam>.FailureWithData(result.Error, result.Result, extraInfo.ToString()));
        }

        var verifyResult = await playerRepository.VerifyUsernameExistence(result.Result.Username);

        // Add extra info
        extraInfo.AppendLine($"2. Verify:\n{verifyResult.ExtraInfo}");

        if (verifyResult.IsSuccess)
        {
            return (false, ServiceResult<InsertPlayerInfoOutputParam>.Success(result.Result, extraInfo.ToString()));
        }

        // Log error
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(
            $"ERROR: Username \"{result.Result.Username}\" already exists at attempt {param.RetryNumber} in the {ChainName} process.");
        messageBuilder.AppendLine($"\nEXTRA INFO:\n--- Retry {param.RetryNumber} ---\n");
        messageBuilder.AppendLine($"1. Generate:\n{result.ExtraInfo}");
        messageBuilder.AppendLine($"2. Verify:\n{verifyResult.ExtraInfo}");

        await Logger.ErrorAsync(messageBuilder.ToString());

        // Update param
        param.CustId = result.Result.CustId;
        param.RetryNumber++;

        if (param.RetryNumber > RetryLimit)
        {
            return (false, ServiceResult<InsertPlayerInfoOutputParam>.FailureWithData(
                PlayerError.CreateCustomerFailed,
                result.Result,
                extraInfo.ToString()));
        }

        return (true, ServiceResult<InsertPlayerInfoOutputParam>.FailureWithData(
            PlayerError.CreateCustomerFailed,
            result.Result,
            extraInfo.ToString()));
    }
}