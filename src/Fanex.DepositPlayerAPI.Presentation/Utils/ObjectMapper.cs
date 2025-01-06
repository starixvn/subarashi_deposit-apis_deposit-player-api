namespace Fanex.DepositPlayerAPI.Presentation.Utils;

using Application.Commands;
using DTOs.Request;

public static class ObjectMapper
{
    public static NewPlayerCommand MapNewPlayerCommand(CreatePlayerRequest request)
    {
        return new NewPlayerCommand
        {
            DepositUserName = request.UserName,
            DepositUserId = request.UserId,
            Password = request.Password,
            OddsType = request.OddsType,
            AgentId = request.AgentId,
            AffiliateId = request.AffiliateId,
            Country = request.Country,
            SubsidiaryId = request.SubsidiaryId
        };
    }

    public static NewShadowAccountCommand MapShadowAccountCommand(CreateShadowAccountRequest request)
    {
        return new NewShadowAccountCommand
        {
            AgentId = request.AgentId,
            IsTest = request.IsTest
        };
    }

    public static ReactivatePlayerCommand MapReactivatePlayerCommand(ReactivatePlayerRequest request, int custId)
    {
        return new ReactivatePlayerCommand
        {
            CustId = custId,
            SubsidiaryId = request.SubsidiaryId
        };
    }
}