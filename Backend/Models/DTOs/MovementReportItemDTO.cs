using System;

namespace ProyectoCaritas.Models.DTOs;

public class MovementReportItemDTO
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int TotalIngresos { get; set; }
    public int TotalEgresos { get; set; }
    public int TotalStock { get; set; } 
    public DateTime LastMovementDate { get; set; }
}