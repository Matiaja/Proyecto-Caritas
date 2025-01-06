﻿namespace ProyectoCaritas.Models.DTOs
{
    public class GetStockDTO
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public int? ProductId { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public required string Status { get; set; } 
    }
}