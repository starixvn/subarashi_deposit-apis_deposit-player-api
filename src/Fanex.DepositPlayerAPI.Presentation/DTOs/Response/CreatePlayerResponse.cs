namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Response;

using Domain.Entities;
using Interfaces;

public class CreatePlayerResponse : IResultResponse<Player>
{
    public int ErrorCode { get; set; }

    public string ErrorMessage { get; set; }

    public Player Result { get; set; }
}