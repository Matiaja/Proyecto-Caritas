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
            if (await _context.Products.AnyAsync())
            {
                return BadRequest("Database already has data (Products found). Seeding aborted.");
            }

            var faker = new Faker("es");

            // 1. Seed Categories
            var categories = new Faker<Category>("es")
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence())
                .Generate(20);
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // 2. Seed Centers
            var centers = new Faker<Center>("es")
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Location, f => f.Address.FullAddress())
                .RuleFor(c => c.Manager, f => f.Name.FullName())
                .RuleFor(c => c.CapacityLimit, f => f.Random.Int(50, 200))
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .Generate(50);
            await _context.Centers.AddRangeAsync(centers);
            await _context.SaveChangesAsync();

            // 3. Seed Users (non-admin)
            var usersToCreate = new Faker<User>("es")
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Role, "User")
                .RuleFor(u => u.CenterId, f => f.Random.ListItem(centers).Id)
                .Generate(300);

            var createdUsers = new List<User>();
            foreach (var user in usersToCreate)
            {
                if (await _userManager.FindByNameAsync(user.UserName) == null)
                {
                    var result = await _userManager.CreateAsync(user, "Password123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                        createdUsers.Add(user);
                    }
                }
            }
            // Add admin user to the list if it exists, for relationship assignment
            var adminUser = await _userManager.FindByNameAsync("admin");
            if (adminUser != null) createdUsers.Add(adminUser);

            // 4. Seed Products
            var products = new Faker<Product>("es")
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Code, f => f.Commerce.Ean13())
                .RuleFor(p => p.CategoryId, f => f.Random.ListItem(categories).Id)
                .Generate(300);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // 5. Seed Requests & OrderLines
            var orderLineFaker = new Faker<OrderLine>("es")
                .RuleFor(ol => ol.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ol => ol.Quantity, f => f.Random.Int(1, 20))
                .RuleFor(ol => ol.Status, "Pendiente");

            var requests = new Faker<Request>("es")
                .RuleFor(r => r.RequestingCenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(r => r.UrgencyLevel, f => f.Random.ListItem(new List<string> { "Baja", "Media", "Alta", "Crítica" }))
                .RuleFor(r => r.RequestDate, f => f.Date.Past(2))
                .RuleFor(r => r.OrderLines, f => orderLineFaker.Generate(f.Random.Int(1, 5)).ToList())
                .Generate(150);
            await _context.Requests.AddRangeAsync(requests);
            await _context.SaveChangesAsync();

            var allOrderLines = await _context.OrderLines.ToListAsync();

            // 6. Seed DonationRequests & Status
            var donationRequestStatusFaker = new Faker<DonationRequestStatus>("es")
                .RuleFor(s => s.Status, f => f.Random.ListItem(new List<string> { "Pendiente", "Aceptada", "Enviada", "Recibida" }))
                .RuleFor(s => s.ChangeDate, f => f.Date.Recent());

            var donationRequests = new Faker<DonationRequest>("es")
                .RuleFor(dr => dr.AssignedCenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(dr => dr.OrderLineId, f => f.Random.ListItem(allOrderLines).Id)
                .RuleFor(dr => dr.Quantity, f => f.Random.Int(1, 10))
                .RuleFor(dr => dr.AssignmentDate, f => f.Date.Recent())
                .RuleFor(dr => dr.Status, "Pendiente")
                .RuleFor(dr => dr.StatusHistory, f => donationRequestStatusFaker.Generate(f.Random.Int(1, 3)).ToList())
                .Generate(150);
            await _context.DonationRequests.AddRangeAsync(donationRequests);
            await _context.SaveChangesAsync();

            // 7. Seed Purchases -> ItemPurchases
            var itemPurchaseFaker = new Faker<ItemPurchase>("es")
                .RuleFor(ip => ip.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ip => ip.Quantity, f => f.Random.Int(10, 200))
                .RuleFor(ip => ip.Description, f => f.Lorem.Sentence());

            var purchases = new Faker<Purchase>("es")
                .RuleFor(p => p.PurchaseDate, f => f.Date.Past(1))
                .RuleFor(p => p.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(p => p.Items, f => itemPurchaseFaker.Generate(f.Random.Int(1, 10)).ToList())
                .Generate(150);
            await _context.Purchases.AddRangeAsync(purchases);
            await _context.SaveChangesAsync();

            // 8. Seed Distributions with correct stock logic
            var allItemPurchases = await _context.ItemsPurchase.AsNoTracking().ToListAsync();
            var remainingQuantities = allItemPurchases.ToDictionary(ip => ip.Id, ip => ip.Quantity);
            var distributions = new List<Distribution>();

            for (int i = 0; i < 200; i++) // Create 200 distributions
            {
                var availableItems = remainingQuantities.Where(rq => rq.Value > 0).ToList();
                if (!availableItems.Any()) break;

                var randomItemPurchaseEntry = faker.Random.ListItem(availableItems);
                var itemPurchaseId = randomItemPurchaseEntry.Key;
                var itemPurchase = allItemPurchases.First(ip => ip.Id == itemPurchaseId);

                var distribution = new Faker<Distribution>("es")
                    .RuleFor(d => d.PurchaseId, itemPurchase.PurchaseId)
                    .RuleFor(d => d.DeliveryDate, f => f.Date.Recent())
                    .RuleFor(d => d.CenterId, f => f.Random.ListItem(centers).Id)
                    .RuleFor(d => d.PersonName, f => f.Name.FullName())
                    .RuleFor(d => d.PersonDNI, f => f.Random.Replace("########"))
                    .Generate();

                var quantityToDistribute = faker.Random.Int(1, remainingQuantities[itemPurchaseId]);

                var itemDistribution = new ItemDistribution
                {
                    Distribution = distribution,
                    ItemPurchaseId = itemPurchaseId,
                    Quantity = quantityToDistribute,
                    Description = "Seeded distribution"
                };

                distribution.Items.Add(itemDistribution);
                distributions.Add(distribution);

                remainingQuantities[itemPurchaseId] -= quantityToDistribute;
            }
            await _context.Distributions.AddRangeAsync(distributions);
            await _context.SaveChangesAsync();


            // 9. Seed Stocks
            var stocks = new Faker<Stock>("es")
                .RuleFor(s => s.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(s => s.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(s => s.Quantity, f => f.Random.Int(0, 500))
                .RuleFor(s => s.Date, f => f.Date.Past(1))
                .Generate(300);
            await _context.Stocks.AddRangeAsync(stocks);
            await _context.SaveChangesAsync();

            // 10. Seed Notifications
            var notificationFaker = new Faker<Notification>("es")
                .RuleFor(n => n.Title, f => f.Lorem.Sentence(3))
                .RuleFor(n => n.Message, f => f.Lorem.Paragraph(1))
                .RuleFor(n => n.Type, f => f.Random.Enum<NotificationType>())
                .RuleFor(n => n.OrderLineId, f => f.Random.ListItem(allOrderLines).Id)
                .RuleFor(n => n.DonationRequestId, f => f.Random.ListItem(donationRequests).Id)
                .RuleFor(n => n.RecipientUserId, f => f.Random.ListItem(createdUsers).Id)
                .RuleFor(n => n.UserId, f => f.Random.ListItem(createdUsers).Id)
                .RuleFor(n => n.CreatedAt, f => f.Date.Past(1))
                .RuleFor(n => n.IsRead, f => f.Random.Bool())
                .RuleFor(n => n.Status, f => f.Random.ListItem(new List<string> { "Active", "Completed", "Expired" }));
            var notifications = notificationFaker.Generate(300);
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            return Ok("Database seeded successfully with a large amount of data and correct stock logic.");
        }
    }
}
