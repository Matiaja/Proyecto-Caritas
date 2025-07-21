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
        private readonly DonationRequestService _donationRequestService;

        public StockService(ApplicationDbContext context, DonationRequestService donationRequestService)
        {
            _context = context;
            _donationRequestService = donationRequestService;
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
                    Center = center
                };

                _context.Stocks.Add(stock);
                await _context.SaveChangesAsync();

                return stock;
            }

            throw new InvalidOperationException("Invalid stock operation.");
        }

        public async Task<int> ValidateQuantity(int centerId, int productId, int newQuantity, string type)
        {
            var totalStock = await GetStockAsync(centerId, productId);

            if (type == "Egreso")
            {
                if (totalStock < newQuantity)
                {
                    throw new InvalidOperationException($"Sin stock suficiente. Solo hay {totalStock} {(totalStock == 1 ? "unidad disponible" : "unidades disponibles")} en el centro.");
                }
                // Verificar si hay suficientes stock verdadero descontando lo asignado
                await ValidateEgreso(centerId, productId, newQuantity);
            }

            return type == "Egreso" ? totalStock - newQuantity : totalStock + newQuantity;
        }

        public async Task<int> GetStockAsync(int centerId, int productId)
        {
            if (centerId <= 0 || productId <= 0)
            {
                throw new ArgumentException("CenterId and ProductId must be greater than zero.");
            }

            var stocks = await _context.Stocks
                .Where(s => s.CenterId == centerId && s.ProductId == productId)
                .ToListAsync();

            int totalStock = stocks
                .Sum(s => s.Type == "Ingreso" ? s.Quantity : -s.Quantity);

            return totalStock;
        }

        public async Task<int> GetQuantityAssigned(int centerId, int productId)
        {
            // Obtener donationRequest asignadas, aceptadas o enviadas (no rechazadas) del centro y producto
            var donationRequests = await _donationRequestService.GetDonationRequestsPending(centerId, productId);

            // Calcular la cantidad total de solicitudes de donación pendientes
            int totalPendingRequests = donationRequests.Sum(dr => dr.Quantity);

            return totalPendingRequests < 0 ? 0 : totalPendingRequests; // Asegurarse de que no sea negativo
        }

        public async Task<int> GetQuantityAvailable(int centerId, int productId)
        {
            var stockActual = await GetStockAsync(centerId, productId);

            var cantidadAsignada = await GetQuantityAssigned(centerId, productId);

            int disponible = stockActual - cantidadAsignada;

            return disponible < 0 ? 0 : disponible; // Asegurarse de que no sea negativo
        }

        public async Task<bool> CanAssignDonation(int centerId, int productId, int quantityAssign)
        {
            // Obtener la cantidad disponible en el centro
            var disponible = await GetQuantityAvailable(centerId, productId);

            // Verificar si hay suficiente stock disponible para asignar
            return disponible >= quantityAssign;
        }

        public async Task ValidateEgreso(int centerId, int productId, int cantidadAEgresar)
        {
            int disponible = await GetQuantityAvailable(centerId, productId);

            // Validar que la cantidad a egresar no supere el stock disponible
            if (cantidadAEgresar > disponible)
            {
                var cantidadAsignada = await GetQuantityAssigned(centerId, productId);

                throw new InvalidOperationException(
                    $"No puedes retirar {cantidadAEgresar} unidad{(cantidadAEgresar == 1 ? "" : "es")}. " +
                    $"Solo hay {disponible} unidad{(disponible == 1 ? "" : "es")} disponible{(disponible == 1 ? "" : "s")} " +
                    $"debido a {cantidadAsignada} unidad{(cantidadAsignada == 1 ? "" : "es")} reservada{(cantidadAsignada == 1 ? "" : "s")} para donar."
                );
            }
        }
    }
}