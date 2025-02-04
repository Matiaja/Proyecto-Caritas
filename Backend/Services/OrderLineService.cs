using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models.DTOs;

public class OrderLineService
{
    private readonly ApplicationDbContext _context;

    public OrderLineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddOrderLineAsync(OrderLineDTO dto, int requestId)
    {
        // Validaciones
        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0.");
        }

        // Validar RequestId, DonationRequestId o ProductId si existen
        var request = dto.RequestId.HasValue
            ? await _context.Requests.FindAsync(dto.RequestId)
            : null;
        if (dto.RequestId.HasValue && request == null)
        {
            throw new ArgumentException("Request not found.");
        }

        var product = dto.ProductId.HasValue
            ? await _context.Products.FindAsync(dto.ProductId)
            : null;
        if (dto.ProductId.HasValue && product == null)
        {
            throw new ArgumentException($"Product ID {dto.ProductId} not found.");
        }

        var orderLine = new OrderLine
        {
            RequestId = requestId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            Description = dto.Description
        };

        _context.OrderLines.Add(orderLine);
        await _context.SaveChangesAsync();
    }
}
