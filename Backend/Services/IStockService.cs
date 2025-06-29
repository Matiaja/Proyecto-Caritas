using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Services
{
    public interface IStockService
    {
        Task<Stock> AddStock(StockDTO stockDTO);
        Task<int> ValidateQuantity(int centerId, int productId, int newQuantity, string type);
    }
}