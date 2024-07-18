namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Response;

using Domain.Entities;
using Interfaces;

public class CreateShadowAccountResponse : IResultResponse<ShadowAccount>
{
    public int ErrorCode { get; set; }

    public string ErrorMessage { get; set; }

    public ShadowAccount Result { get; set; }
}