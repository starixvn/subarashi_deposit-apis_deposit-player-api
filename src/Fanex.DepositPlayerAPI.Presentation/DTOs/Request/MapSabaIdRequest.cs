namespace Fanex.DepositPlayerAPI.Presentation.DTOs.Request;

using System.ComponentModel.DataAnnotations;
using Interfaces;

public class MapSabaIdRequest : IRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CustId { get; set; }

    [Required]
    public byte CustType { get; set; }

    public bool IsDebug { get; set; }
}