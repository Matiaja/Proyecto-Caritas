using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;
using ProyectoCaritas.Models.Entities;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoCaritas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataSeedingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public DataSeedingController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            // Guard clause removed to allow additive seeding

            var faker = new Faker("es");

            // 1. Seed Categories (Idempotent)
            var existingCategoryNames = await _context.Categories.Select(c => c.Name).ToListAsync();
            var categoryFaker = new Faker<Category>("es")
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence());

            var potentialCategories = categoryFaker.Generate(15);
            var newCategories = potentialCategories
                .Where(c => !existingCategoryNames.Contains(c.Name))
                .DistinctBy(c => c.Name)
                .ToList();

            if (newCategories.Any())
            {
                await _context.Categories.AddRangeAsync(newCategories);
                await _context.SaveChangesAsync();
            }
            var allCategories = await _context.Categories.ToListAsync();


            // 2. Seed Centers (Idempotent)
            var existingCenterNames = await _context.Centers.Select(c => c.Name).ToListAsync();
            var centerFaker = new Faker<Center>("es")
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Location, f => f.Address.FullAddress())
                .RuleFor(c => c.Manager, f => f.Name.FullName())
                .RuleFor(c => c.CapacityLimit, 1000)
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.Email, f => f.Internet.Email());

            var potentialCenters = centerFaker.Generate(3);
            var newCenters = potentialCenters
                .Where(c => !existingCenterNames.Contains(c.Name))
                .DistinctBy(c => c.Name)
                .ToList();

            if (newCenters.Any())
            {
                await _context.Centers.AddRangeAsync(newCenters);
                await _context.SaveChangesAsync();
            }
            var allCenters = await _context.Centers.ToListAsync();


            // 3. Seed Users (Already Idempotent)
            var usersToCreate = new Faker<User>("es")
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName).ToLower())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Role, "User")
                .RuleFor(u => u.CenterId, f => f.Random.ListItem(allCenters).Id)
                .Generate(10);

            foreach (var user in usersToCreate)
            {
                if (await _userManager.FindByNameAsync(user.UserName) == null)
                {
                    var result = await _userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }

            // 4. Seed Products (Idempotent)
            var existingProductNames = await _context.Products.Select(p => p.Name).ToListAsync();
            var productFaker = new Faker<Product>("es")
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Code, f => f.Commerce.Ean13())
                .RuleFor(p => p.CategoryId, f => f.Random.ListItem(allCategories).Id);

            var potentialProducts = productFaker.Generate(10);
            var newProducts = potentialProducts
                .Where(p => !existingProductNames.Contains(p.Name))
                .DistinctBy(p => p.Name)
                .ToList();

            if (newProducts.Any())
            {
                await _context.Products.AddRangeAsync(newProducts);
                await _context.SaveChangesAsync();
            }
            var allProducts = await _context.Products.ToListAsync();

            // 5. Seed Requests (Transactional)
            var orderLineFaker = new Faker<OrderLine>("es")
                .RuleFor(ol => ol.ProductId, f => f.Random.ListItem(allProducts).Id)
                .RuleFor(ol => ol.Quantity, f => f.Random.Int(1, 5))
                .RuleFor(ol => ol.Description, f => f.Lorem.Sentence())
                .RuleFor(ol => ol.Status, "Pendiente");

            var requests = new Faker<Request>("es")
                .RuleFor(r => r.RequestingCenterId, f => f.Random.ListItem(allCenters).Id)
                .RuleFor(r => r.UrgencyLevel, f => f.Random.ListItem(new List<string> { "Alto", "Bajo" }))
                .RuleFor(r => r.RequestDate, f => f.Date.Past(2))
                .RuleFor(r => r.OrderLines, f => orderLineFaker.Generate(f.Random.Int(1, 2)).ToList())
                .Generate(10);
            await _context.Requests.AddRangeAsync(requests);
            await _context.SaveChangesAsync();

            var allOrderLines = await _context.OrderLines.ToListAsync();

            // 6. Seed DonationRequests (Transactional)
            var donationRequestStatusFaker = new Faker<DonationRequestStatus>("es")
                .RuleFor(s => s.Status, f => f.Random.ListItem(new List<string> { "Pendiente", "Aceptada", "Enviada", "Recibida" }))
                .RuleFor(s => s.ChangeDate, f => f.Date.Recent());

            var donationRequests = new Faker<DonationRequest>("es")
                .RuleFor(dr => dr.AssignedCenterId, f => f.Random.ListItem(allCenters).Id)
                .RuleFor(dr => dr.OrderLineId, f => f.Random.ListItem(allOrderLines).Id)
                .RuleFor(dr => dr.Quantity, f => f.Random.Int(1, 5))
                .RuleFor(dr => dr.AssignmentDate, f => f.Date.Recent())
                .RuleFor(dr => dr.Status, "Pendiente")
                .RuleFor(dr => dr.StatusHistory, f => donationRequestStatusFaker.Generate(1).ToList())
                .Generate(10);
            await _context.DonationRequests.AddRangeAsync(donationRequests);
            await _context.SaveChangesAsync();

            // 7. Seed Purchases and 'Ingreso' Stock (Transactional)
            var itemPurchaseFaker = new Faker<ItemPurchase>("es")
                .RuleFor(ip => ip.ProductId, f => f.Random.ListItem(allProducts).Id)
                .RuleFor(ip => ip.Quantity, f => f.Random.Int(10, 50))
                .RuleFor(ip => ip.Description, f => f.Lorem.Sentence());

            var purchases = new Faker<Purchase>("es")
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past(1))
                .RuleFor(p => p.CenterId, f => f.Random.ListItem(allCenters).Id)
                .RuleFor(p => p.Type, f => f.Random.ListItem(new List<string> { "PNUD", "Diocesana", "General" }))
                .RuleFor(p => p.Items, f => itemPurchaseFaker.Generate(f.Random.Int(2, 5)).ToList())
                .Generate(10);

            await _context.Purchases.AddRangeAsync(purchases);
            await _context.SaveChangesAsync();

            var ingresoStocks = new List<Stock>();
            foreach (var purchase in purchases)
            {
                foreach (var item in purchase.Items)
                {
                    ingresoStocks.Add(new Stock
                    {
                        CenterId = purchase.CenterId,
                        Center = allCenters.First(c => c.Id == purchase.CenterId),
                        ProductId = item.ProductId,
                        Date = purchase.PurchaseDate,
                        Quantity = item.Quantity,
                        Type = "Ingreso",
                        Description = $"Ingreso por compra #{purchase.Id}"
                    });
                }
            }
            await _context.Stocks.AddRangeAsync(ingresoStocks);
            await _context.SaveChangesAsync();

            // 8. Seed Distributions and 'Egreso' Stock (Transactional)
            var allItemPurchases = await _context.ItemsPurchase.AsNoTracking().ToListAsync();
            var remainingQuantities = allItemPurchases.ToDictionary(ip => ip.Id, ip => ip.Quantity);
            var distributions = new List<Distribution>();

            for (int i = 0; i < 15; i++)
            {
                var availableItems = remainingQuantities.Where(rq => rq.Value > 1).ToList();
                if (!availableItems.Any()) break;

                var randomItemPurchaseEntry = faker.Random.ListItem(availableItems);
                var itemPurchaseId = randomItemPurchaseEntry.Key;
                var itemPurchase = allItemPurchases.First(ip => ip.Id == itemPurchaseId);

                var distribution = new Faker<Distribution>("es")
                    .RuleFor(d => d.PurchaseId, itemPurchase.PurchaseId)
                    .RuleFor(d => d.DeliveryDate, f => f.Date.Recent())
                    .RuleFor(d => d.CenterId, f => f.Random.ListItem(allCenters).Id)
                    .RuleFor(d => d.PersonName, f => f.Name.FullName())
                    .RuleFor(d => d.PersonDNI, f => f.Random.Replace("########"))
                    .Generate();

                var maxQuantity = remainingQuantities[itemPurchaseId] - 1;
                var quantityToDistribute = (maxQuantity > 0) ? faker.Random.Int(1, maxQuantity) : 0;

                if(quantityToDistribute > 0)
                {
                    var itemDistribution = new ItemDistribution
                    {
                        Distribution = distribution,
                        ItemPurchaseId = itemPurchaseId,
                        Quantity = quantityToDistribute,
                        Description = "Salida por donación"
                    };

                    distribution.Items.Add(itemDistribution);
                    distributions.Add(distribution);

                    remainingQuantities[itemPurchaseId] -= quantityToDistribute;
                }
            }
            await _context.Distributions.AddRangeAsync(distributions);
            await _context.SaveChangesAsync();

            var egresoStocks = new List<Stock>();
            foreach (var dist in distributions)
            {
                foreach (var item in dist.Items)
                {
                    var originalItemPurchase = allItemPurchases.First(ip => ip.Id == item.ItemPurchaseId);
                    egresoStocks.Add(new Stock
                    {
                        CenterId = dist.CenterId.Value,
                        Center = allCenters.First(c => c.Id == dist.CenterId.Value),
                        ProductId = originalItemPurchase.ProductId,
                        Date = dist.DeliveryDate,
                        Quantity = item.Quantity,
                        Type = "Egreso",
                        Description = $"Egreso por distribución #{dist.Id}"
                    });
                }
            }
            await _context.Stocks.AddRangeAsync(egresoStocks);
            await _context.SaveChangesAsync();

            // Notification seeding removed

            return Ok("Seeder ran successfully. New data was added without creating duplicates.");
        }
    }
}
