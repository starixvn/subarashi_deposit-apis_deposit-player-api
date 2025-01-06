namespace Fanex.DepositPlayerAPI.Application.Commands;

using Domain.Results;
using MediatR;

public class MapSabaIdCommand : IRequest<ServiceResult>
{
    public long SabaId { get; set; }

    public int CustId { get; set; }

    public byte CustType { get; set; }
}