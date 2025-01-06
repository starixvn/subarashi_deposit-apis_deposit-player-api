namespace Fanex.DepositPlayerAPI.Application.Utils;

using Commands;
using Domain.Params.Chains;

public static class ParamMapper
{
    public static NewPlayerChainParam BuildNewPlayerChainParam(NewPlayerCommand command)
    {
        return new NewPlayerChainParam
        {
            DepositUserName = command.DepositUserName,
            DepositUserId = command.DepositUserId,
            Password = command.Password,
            OddsType = command.OddsType,
            AgentId = command.AgentId,
            AffiliateId = command.AffiliateId,
            Country = command.Country,
            SubsidiaryId = command.SubsidiaryId,
            SiteDbGroupId = command.SiteDbGroupId
        };
    }

    public static NewShadowAccountChainParam BuildNewShadowAccountChainParam(NewShadowAccountCommand command)
    {
        return new NewShadowAccountChainParam
        {
            AgentId = command.AgentId,
            SiteDbGroupId = command.SiteDbGroupId,
            IsTest = command.IsTest
        };
    }

    public static ReactivatePlayerChainParam BuildReactivatePlayerChainParam(ReactivatePlayerCommand command)
    {
        return new ReactivatePlayerChainParam
        {
            CustId = command.CustId,
            SubsidiaryId = command.SubsidiaryId,
            Site = command.Site
        };
    }

    public static MapSabaIdChainParam BuildMapSabaIdChainParam(MapSabaIdCommand command)
    {
        return new MapSabaIdChainParam
        {
            CustId = command.CustId,
            SabaId = command.SabaId,
            CustType = command.CustType
        };
    }
}