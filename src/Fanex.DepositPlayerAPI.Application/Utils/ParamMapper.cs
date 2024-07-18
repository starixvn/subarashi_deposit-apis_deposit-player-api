namespace Fanex.DepositPlayerAPI.Application.Utils;

using Commands;
using Domain.Params.Chains;

public static class ParamMapper
{
    public static NewPlayerChainParam MapNewPlayerChainParam(NewPlayerCommand command)
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

    public static NewShadowAccountChainParam MapNewShadowAccountChainParam(NewShadowAccountCommand command)
    {
        return new NewShadowAccountChainParam
        {
            AgentId = command.AgentId,
            RealCustId = command.RealCustId,
            SiteDbGroupId = command.SiteDbGroupId
        };
    }

    public static ReactivatePlayerChainParam MapReactivatePlayerChainParam(ReactivatePlayerCommand command)
    {
        return new ReactivatePlayerChainParam
        {
            CustId = command.CustId,
            SubsidiaryId = command.SubsidiaryId,
            Site = command.Site
        };
    }
}