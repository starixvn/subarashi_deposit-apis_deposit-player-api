namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using Interfaces;

public class CreateShadowAccountRequest : IRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int RealCustId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int AgentId { get; set; }

    public bool IsDebug { get; set; }
}