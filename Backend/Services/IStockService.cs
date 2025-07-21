using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Services
{
    public interface IStockService
    {
        Task<Stock> AddStock(StockDTO stockDTO);
        Task<int> ValidateQuantity(int centerId, int productId, int newQuantity, string type);
        Task<int> GetStockAsync(int centerId, int productId);
        Task<int> GetQuantityAssigned(int centerId, int productId);
        Task<int> GetQuantityAvailable(int centerId, int productId);
        Task<bool> CanAssignDonation(int centerId, int productId, int quantityAssign);
        Task ValidateEgreso(int centerId, int productId, int cantidadAEgresar);
    }
}