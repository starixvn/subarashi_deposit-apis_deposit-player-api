namespace Fanex.DepositPlayerAPI.Presentation.Utils;

using Application.Commands;
using DTOs.Request;

public static class ObjectMapper
{
    public static NewPlayerCommand BuildNewPlayerCommand(CreatePlayerRequest request)
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

    public static NewShadowAccountCommand BuildShadowAccountCommand(CreateShadowAccountRequest request)
    {
        return new NewShadowAccountCommand
        {
            AgentId = request.AgentId,
            IsTest = request.IsTest
        };
    }

    public static ReactivatePlayerCommand BuildReactivatePlayerCommand(ReactivatePlayerRequest request, int custId)
    {
        return new ReactivatePlayerCommand
        {
            CustId = custId,
            SubsidiaryId = request.SubsidiaryId
        };
    }

    public static MapSabaIdCommand BuildMapSabaIdCommand(MapSabaIdRequest request, long sabaId)
    {
        return new MapSabaIdCommand
        {
            CustId = request.CustId,
            CustType = request.CustType,
            SabaId = sabaId
        };
    }
}