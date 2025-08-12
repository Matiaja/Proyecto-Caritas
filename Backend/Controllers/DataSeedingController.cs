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
            // Safety check to prevent seeding a populated database
            if (await _context.Users.AnyAsync() || await _context.Categories.AnyAsync())
            {
                return BadRequest("Database already has data. Seeding aborted.");
            }

            // --- Seeding Order ---
            // 1. Categories
            // 2. Centers
            // 3. Users
            // 4. Products
            // 5. Requests
            // 6. OrderLines
            // 7. DonationRequests
            // 8. DonationRequestStatus
            // 9. Purchases -> ItemPurchases
            // 10. Distributions -> ItemDistributions
            // 11. Stocks
            // 12. Notifications

            // 1. Seed Categories
            var categoryFaker = new Faker<Category>("es")
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence());
            var categories = categoryFaker.Generate(20);
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // 2. Seed Centers
            var centerFaker = new Faker<Center>("es")
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Location, f => f.Address.FullAddress())
                .RuleFor(c => c.Manager, f => f.Name.FullName())
                .RuleFor(c => c.CapacityLimit, f => f.Random.Int(50, 200))
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.Email, f => f.Internet.Email());
            var centers = centerFaker.Generate(50);
            await _context.Centers.AddRangeAsync(centers);
            await _context.SaveChangesAsync();

            // 3. Seed Users
            var userFaker = new Faker<User>("es")
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Role, f => f.Random.ListItem(new List<string> { "Admin", "User" }))
                .RuleFor(u => u.CenterId, f => f.Random.ListItem(centers).Id);

            var users = userFaker.Generate(300);
            var createdUsers = new List<User>();
            foreach (var user in users)
            {
                var result = await _userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, user.Role);
                    createdUsers.Add(user);
                }
            }

            // 4. Seed Products
            var productFaker = new Faker<Product>("es")
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Code, f => f.Commerce.Ean13())
                .RuleFor(p => p.CategoryId, f => f.Random.ListItem(categories).Id);
            var products = productFaker.Generate(300);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // 5. Seed Requests
            var requestFaker = new Faker<Request>("es")
                .RuleFor(r => r.RequestingCenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(r => r.UrgencyLevel, f => f.Random.ListItem(new List<string> { "Baja", "Media", "Alta", "Crítica" }))
                .RuleFor(r => r.RequestDate, f => f.Date.Past(2));
            var requests = requestFaker.Generate(300);
            await _context.Requests.AddRangeAsync(requests);
            await _context.SaveChangesAsync();

            // 6. Seed OrderLines
            var orderLineFaker = new Faker<OrderLine>("es")
                .RuleFor(ol => ol.RequestId, f => f.Random.ListItem(requests).Id)
                .RuleFor(ol => ol.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ol => ol.Quantity, f => f.Random.Int(1, 20))
                .RuleFor(ol => ol.Status, f => "Pendiente");
            var orderLines = orderLineFaker.Generate(300);
            await _context.OrderLines.AddRangeAsync(orderLines);
            await _context.SaveChangesAsync();

            // 7. Seed DonationRequests
            var donationRequestFaker = new Faker<DonationRequest>("es")
                .RuleFor(dr => dr.AssignedCenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(dr => dr.OrderLineId, f => f.Random.ListItem(orderLines).Id)
                .RuleFor(dr => dr.Quantity, f => f.Random.Int(1, 10))
                .RuleFor(dr => dr.AssignmentDate, f => f.Date.Recent())
                .RuleFor(dr => dr.Status, f => "Pendiente");
            var donationRequests = donationRequestFaker.Generate(300);
            await _context.DonationRequests.AddRangeAsync(donationRequests);
            await _context.SaveChangesAsync();

            // 8. Seed DonationRequestStatus
            var drStatusFaker = new Faker<DonationRequestStatus>("es")
                .RuleFor(s => s.DonationRequestId, f => f.Random.ListItem(donationRequests).Id)
                .RuleFor(s => s.Status, f => f.Random.ListItem(new List<string> { "Pendiente", "Aceptada", "Enviada", "Recibida" }))
                .RuleFor(s => s.ChangeDate, f => f.Date.Recent());
            var donationRequestStatuses = drStatusFaker.Generate(300);
            await _context.DonationRequestStatus.AddRangeAsync(donationRequestStatuses);
            await _context.SaveChangesAsync();

            // 9. Seed Purchases and ItemPurchases
            var itemPurchaseFaker = new Faker<ItemPurchase>("es")
                .RuleFor(ip => ip.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ip => ip.Quantity, f => f.Random.Int(1, 100))
                .RuleFor(ip => ip.Description, f => f.Lorem.Sentence());

            var purchaseFaker = new Faker<Purchase>("es")
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past(1))
                .RuleFor(p => p.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(p => p.Items, f => itemPurchaseFaker.Generate(f.Random.Int(1, 10)).ToList());

            var purchases = purchaseFaker.Generate(150);
            await _context.Purchases.AddRangeAsync(purchases);
            await _context.SaveChangesAsync();

            // 10. Seed Distributions and ItemDistributions
            var allItemPurchases = await _context.ItemsPurchase.ToListAsync();

            var itemDistributionFaker = new Faker<ItemDistribution>("es")
                .RuleFor(id => id.ItemPurchaseId, f => f.Random.ListItem(allItemPurchases).Id)
                .RuleFor(id => id.Quantity, (f, id) => {
                    var itemPurchase = allItemPurchases.First(ip => ip.Id == id.ItemPurchaseId);
                    return f.Random.Int(1, itemPurchase.RemainingQuantity > 0 ? itemPurchase.RemainingQuantity : 1);
                 })
                .RuleFor(id => id.Description, f => f.Lorem.Sentence());

            var distributionFaker = new Faker<Distribution>("es")
                .RuleFor(d => d.PurchaseId, f => f.Random.ListItem(purchases).Id)
                .RuleFor(d => d.DeliveryDate, f => f.Date.Recent())
                .RuleFor(d => d.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(d => d.PersonName, f => f.Name.FullName())
                .RuleFor(d => d.PersonDNI, f => f.Random.Replace("########"))
                .RuleFor(d => d.Items, f => itemDistributionFaker.Generate(f.Random.Int(1, 5)).ToList());

            var distributions = distributionFaker.Generate(150);
            await _context.Distributions.AddRangeAsync(distributions);
            await _context.SaveChangesAsync();


            // 11. Seed Stocks
            var stockFaker = new Faker<Stock>("es")
                .RuleFor(s => s.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(s => s.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(s => s.Quantity, f => f.Random.Int(1, 500))
                .RuleFor(s => s.EntryDate, f => f.Date.Past(1));
            var stocks = stockFaker.Generate(300);
            await _context.Stocks.AddRangeAsync(stocks);
            await _context.SaveChangesAsync();

            // 12. Seed Notifications
            var notificationFaker = new Faker<Notification>("es")
                .RuleFor(n => n.Message, f => f.Lorem.Sentence())
                .RuleFor(n => n.IsRead, f => f.Random.Bool())
                .RuleFor(n => n.CreatedAt, f => f.Date.Past(1))
                .RuleFor(n => n.UserId, f => f.Random.ListItem(createdUsers).Id);
            var notifications = notificationFaker.Generate(300);
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            return Ok("Database seeded successfully with a large amount of data.");
        }
    }
}
