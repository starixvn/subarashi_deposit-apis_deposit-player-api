namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using Interfaces;

public class ReactivatePlayerRequest : IRequest
{
    [Range(0, int.MaxValue)]
    public int? SubsidiaryId { get; set; }

    public bool IsDebug { get; set; }
}