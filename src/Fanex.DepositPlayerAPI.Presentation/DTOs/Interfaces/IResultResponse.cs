namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Interfaces;

public interface IResultResponse<T> where T : class
{
    public int ErrorCode { get; set; }

    public string ErrorMessage { get; set; }

    public T Result { get; set; }
}

public interface IResultResponse
{
    public int ErrorCode { get; set; }

    public string ErrorMessage { get; set; }
}