namespace Fanex.DepositPlayerAPI.Application.Commands;

using Domain.Entities;
using Domain.Results;
using MediatR;

public class NewShadowAccountCommand : IRequest<ServiceResult<ShadowAccount>>
{
    public int RealCustId { get; set; }

    public int AgentId { get; set; }

    public int SiteDbGroupId { get; set; }
}