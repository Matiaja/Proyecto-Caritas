using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoCaritas.Services
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;

        public StockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock> AddStock(StockDTO stockDTO)
        {
            // Validaciones básicas que no dependen del usuario
            if (!stockDTO.ProductId.HasValue)
            {
                throw new ArgumentException("ProductId is required.");
            }

            var product = await _context.Products.FindAsync(stockDTO.ProductId);
            if (product == null)
            {
                throw new ArgumentException("Product not found.");
            }

            var center = await _context.Centers.FindAsync(stockDTO.CenterId);
            if (center == null)
            {
                throw new ArgumentException("Center not found.");
            }

            // Validar tipo
            if (string.IsNullOrEmpty(stockDTO.Type) ||
                (stockDTO.Type != "Ingreso" && stockDTO.Type != "Egreso"))
            {
                throw new ArgumentException("Invalid stock type. It must be 'Ingreso' or 'Egreso'.");
            }

            // Validar cantidad
            var validationResult = await ValidateQuantity(stockDTO.CenterId, (int)stockDTO.ProductId, stockDTO.Quantity, stockDTO.Type);

            if (validationResult >= 0)
            {
                var stock = new Stock
                {
                    CenterId = stockDTO.CenterId,
                    ProductId = stockDTO.ProductId,
                    Date = stockDTO.Date,
                    ExpirationDate = stockDTO.ExpirationDate,
                    Description = stockDTO.Description,
                    Quantity = stockDTO.Quantity,
                    Weight = stockDTO.Weight,
                    Type = stockDTO.Type,
                    Origin = stockDTO.Origin,
                    Center = center,
                    Product = product
                };

                _context.Stocks.Add(stock);
                await _context.SaveChangesAsync();

                return stock;
            }

            throw new InvalidOperationException("Invalid stock operation.");
        }

        public async Task<int> ValidateQuantity(int centerId, int productId, int newQuantity, string type)
        {
            var stocks = await _context.Stocks
                .Where(s => s.CenterId == centerId && s.ProductId == productId)
                .ToListAsync();

            int totalStock = stocks
                .Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity);

            if (type == "Egreso")
            {
                if (totalStock < newQuantity)
                {
                    throw new InvalidOperationException("El stock no puede ser negativo");
                }
            }

            return type == "Egreso" ? totalStock - newQuantity : totalStock + newQuantity;
        }
    }
}