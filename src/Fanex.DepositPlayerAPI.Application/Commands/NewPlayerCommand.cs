namespace Fanex.DepositPlayerAPI.Application.Commands;

using Domain.Entities;
using Domain.Results;
using MediatR;

public class NewPlayerCommand : IRequest<ServiceResult<Player>>
{
    public string DepositUserName { get; set; }

    public int DepositUserId { get; set; }

    public string Password { get; set; }

    public string OddsType { get; set; }

    public int AgentId { get; set; }

    public int? AffiliateId { get; set; }

    public string Country { get; set; }

    public int? SubsidiaryId { get; set; }

    public int SiteDbGroupId { get; set; }
}