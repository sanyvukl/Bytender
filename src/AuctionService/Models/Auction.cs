﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AuctionService.Models
{
    [Table("Auctions")]
    public class Auction
    {
        public Guid Id { get; set; }
        public int ReservePrice { get; set; } = 0;
        public string Seller { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public int? SoldAmount { get; set; }
        public int? CurrentHightBid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime AuctionEnd { get; set; }
        //TODO: Change to string, or add converter
        public Status Status { get; set; }
        public Item Item { get; set; }
    }
}
