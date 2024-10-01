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
    IPlayerRepository playerRepository)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    private const int RetryLimit = 3;

    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInsertInfoChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        ServiceResult<InsertPlayerInfoOutputParam> result = null;
        List<string> triedUsernames = [];
        var extraInfo = new StringBuilder();
        var retryCount = 0;

        var param = new InsertPlayerInfoParam
        {
            Username2 = request.Username2,
            Recommend = request.Recommend,
            SiteDbGroupId = request.SiteDbGroupId,
            Site = request.Site,
            CurrencyName = request.CurrencyName,
            RetryNumber = retryCount
        };

        while (retryCount++ < RetryLimit)
        {
            result = await playerRepository.InsertPlayerInfoAsync(param);

            // Add extra info
            extraInfo.AppendLine($"\n- RetryNumber {param.RetryNumber}:\n");
            extraInfo.AppendLine($"Generate:\n{result.ExtraInfo}");

            if (result.IsFailure)
            {
                break;
            }

            var verifyResult = await playerRepository.VerifyUsernameExistence(result.Result.Username);

            // Add extra info
            extraInfo.AppendLine($"Verify:\n{verifyResult.ExtraInfo}");

            if (verifyResult.IsSuccess)
            {
                break;
            }

            // Log error
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(
                $"ERROR: Username \"{result.Result.Username}\" already exists at attempt {param.RetryNumber} in the {ChainName} process.");
            messageBuilder.AppendLine($"\nEXTRA INFO: {verifyResult.ExtraInfo}");
            await Logger.ErrorAsync(messageBuilder.ToString());

            // Assign result to create player failed when Username already exists
            triedUsernames.Add(result.Result.Username);
            result = ServiceResult<InsertPlayerInfoOutputParam>.Failure(
                PlayerError.CreateCustomerFailed,
                $"Username already exists after all attempts.\nList all tried usernames: [{string.Join(",", triedUsernames)}]");

            // Update RetryNumber
            param.RetryNumber = retryCount;
        }

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            _ => ServiceResult<Player>.Success(null, extraInfo.ToString()),
            (error, _) => ServiceResult<Player>.Failure(error, extraInfo.ToString()));
    }

    private static void UpdateRequest(InsertPlayerInfoOutputParam output, NewPlayerChainParam request)
    {
        request.Username = output.Username;
        request.CustId = output.CustId;
    }
}