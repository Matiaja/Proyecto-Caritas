﻿namespace ProyectoCaritas.Models.DTOs
{
    public class ProductStockDTO
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int StockQuantity { get; set; }
    }
}
