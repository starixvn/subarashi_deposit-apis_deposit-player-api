namespace Fanex.DepositPlayerAPI.Application.Commands;

using Domain.Results;
using MediatR;

public class ReactivatePlayerCommand : IRequest<ServiceResult>
{
    public int CustId { get; set; }

    public int? SubsidiaryId { get; set; }

    public string Site { get; set; }
}