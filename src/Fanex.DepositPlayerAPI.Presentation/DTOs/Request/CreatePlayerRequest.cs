namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using Interfaces;

public class CreatePlayerRequest : IRequest
{
    [Required]
    [RegularExpression(@"^\S*$", ErrorMessage = "No white space allowed")]
    public string UserName { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    public string Password { get; set; }

    public string OddsType { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int AgentId { get; set; }

    [Range(0, int.MaxValue)]
    public int? AffiliateId { get; set; }

    public string Country { get; set; }

    [Range(0, int.MaxValue)]
    public int? SubsidiaryId { get; set; }

    public bool IsDebug { get; set; }
}