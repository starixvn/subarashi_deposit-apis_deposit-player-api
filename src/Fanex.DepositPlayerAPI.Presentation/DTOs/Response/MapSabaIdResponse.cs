namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Response;

using Interfaces;

public class MapSabaIdResponse : IResultResponse
{
    public int ErrorCode { get; set; }

    public string ErrorMessage { get; set; }
}