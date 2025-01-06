namespace Fanex.DepositPlayerAPI.Application.Commands;

using Domain.Entities;
using Domain.Results;
using MediatR;

public class NewShadowAccountCommand : IRequest<ServiceResult<ShadowAccount>>
{
    public int AgentId { get; set; }

    public int SiteDbGroupId { get; set; }

    public bool IsTest { get; set; }
}