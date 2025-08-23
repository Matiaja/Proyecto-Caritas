using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoCaritas.Models.Entities
{
    // Entidad Compra
    public class Purchase
    {
        public int Id { get; set; }
        [Column(TypeName = "date")]
        public DateTime PurchaseDate { get; set; } // Fecha de compra
        public string Type { get; set; } = "General"; // Tipo de compra (e.g., "PNUD", "Diocesana")
        public int CenterId { get; set; } // FK a Centro donde está la compra / bolsón
        public int OriginalCenterId { get; set; } // FK: centro que generó originalmente la compra
        public string? BuyerName { get; set; } // Quién compró (persona/responsable)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public Center Center { get; set; } = null!;
        public Center OriginalCenter { get; set; } = null!; // Centro original de la compra
        public ICollection<ItemPurchase> Items { get; set; } = new List<ItemPurchase>();
        public ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
    }

    // Entidad ItemCompra
    public class ItemPurchase
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; } // FK a Compra
        public int ProductId { get; set; } // FK a Producto
        public int Quantity { get; set; } // Cantidad comprada
        public string? Description { get; set; } // Descripción opcional para notas

        // Propiedad calculada para RemainingQuantity
        [NotMapped]
        public int RemainingQuantity => Quantity - ItemsDistribution.Sum(itd => itd.Quantity);

        // Propiedades de navegación
        public Purchase Purchase { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ICollection<ItemDistribution> ItemsDistribution { get; set; } = new List<ItemDistribution>();
    }

    // Entidad Salida
    public class Distribution
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; } // FK a Compra (origen)
        public DateTime DeliveryDate { get; set; } // Fecha de salida
        public int? CenterId { get; set; } // FK a Centro (destino)
        public string? PersonName { get; set; } // Nombre de la persona que recibe
        public string? PersonDNI { get; set; } // DNI de la persona que recibe
        public string? PersonMemberFamily { get; set; } // Integrante de la familia
        public string? PersonLocation { get; set; } // Direccion o ciudad de la persona que recibe
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Entregado";

        // Propiedades de navegación
        public Center? Center { get; set; } = null!;
        public Purchase? Purchase { get; set; }
        public ICollection<ItemDistribution> Items { get; set; } = new List<ItemDistribution>();
    }

    // Entidad ItemSalida
    public class ItemDistribution
    {
        public int Id { get; set; }
        public int DistributionId { get; set; } // FK a Distribution
        public int ItemPurchaseId { get; set; } // FK a ItemCompra (origen)
        public int Quantity { get; set; } // Cantidad entregada
        public string? Description { get; set; } // Descripción opcional para notas

        // Propiedades de navegación
        public Distribution Distribution { get; set; } = null!;
        public ItemPurchase ItemPurchase { get; set; } = null!;
    }
}