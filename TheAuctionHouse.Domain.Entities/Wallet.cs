using System.ComponentModel.DataAnnotations;

namespace TheAuctionHouse.Domain.Entities;

public class Wallet
{
    [Key]
    public int UserId { get; set; }
    public int Amount { get; set; }
    public int BlockedAmount { get; set; }
}