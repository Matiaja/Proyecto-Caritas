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
            if (await _context.Users.AnyAsync() || await _context.Categories.AnyAsync())
            {
                return BadRequest("Database already has data. Seeding aborted.");
            }

            // Seed Categories
            var categoryFaker = new Faker<Category>()
                .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Description, f => f.Lorem.Sentence());
            var categories = categoryFaker.Generate(20);
            _context.Categories.AddRange(categories);
            _context.SaveChanges();

            // Seed Centers
            var centerFaker = new Faker<Center>()
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Location, f => f.Address.FullAddress())
                .RuleFor(c => c.Manager, f => f.Name.FullName())
                .RuleFor(c => c.CapacityLimit, f => f.Random.Int(50, 200))
                .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(c => c.Email, f => f.Internet.Email());
            var centers = centerFaker.Generate(50);
            _context.Centers.AddRange(centers);
            _context.SaveChanges();

            // Seed Users
            var userFaker = new Faker<User>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.UserName))
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

            // Seed Products
            var productFaker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Code, f => f.Commerce.Ean13())
                .RuleFor(p => p.CategoryId, f => f.Random.ListItem(categories).Id);
            var products = productFaker.Generate(300);
            _context.Products.AddRange(products);
            _context.SaveChanges();

            // Seed Stocks
            var stockFaker = new Faker<Stock>()
                .RuleFor(s => s.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(s => s.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(s => s.Quantity, f => f.Random.Int(1, 100));
            var stocks = stockFaker.Generate(300);
            _context.Stocks.AddRange(stocks);
            _context.SaveChanges();

            // Seed Requests
            var requestFaker = new Faker<Request>()
                .RuleFor(r => r.UserId, f => f.Random.ListItem(users).Id)
                .RuleFor(r => r.Title, f => f.Lorem.Sentence(3))
                .RuleFor(r => r.Description, f => f.Lorem.Paragraph())
                .RuleFor(r => r.UrgencyLevel, f => f.Random.Int(1, 5));
            var requests = requestFaker.Generate(300);
            _context.Requests.AddRange(requests);
            _context.SaveChanges();

            // Seed OrderLines
            var orderLineFaker = new Faker<OrderLine>()
                .RuleFor(ol => ol.RequestId, f => f.Random.ListItem(requests).Id)
                .RuleFor(ol => ol.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ol => ol.Quantity, f => f.Random.Int(1, 10));
            var orderLines = orderLineFaker.Generate(300);
            _context.OrderLines.AddRange(orderLines);
            _context.SaveChanges();

            // Seed DonationRequests
            var donationRequestFaker = new Faker<DonationRequest>()
                .RuleFor(dr => dr.UserId, f => f.Random.ListItem(users).Id)
                .RuleFor(dr => dr.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(dr => dr.Description, f => f.Lorem.Paragraph());
            var donationRequests = donationRequestFaker.Generate(300);
            _context.DonationRequests.AddRange(donationRequests);
            _context.SaveChanges();

            // Seed DonationRequestStatus
            var donationRequestStatusFaker = new Faker<DonationRequestStatus>()
                .RuleFor(drs => drs.DonationRequestId, f => f.Random.ListItem(donationRequests).Id)
                .RuleFor(drs => drs.Status, f => f.Random.ListItem(new List<string> { "Pending", "Approved", "Rejected" }));
            var donationRequestStatuses = donationRequestStatusFaker.Generate(300);
            _context.DonationRequestStatus.AddRange(donationRequestStatuses);
            _context.SaveChanges();

            // Seed Purchases
            var purchaseFaker = new Faker<Purchase>()
                .RuleFor(p => p.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(p => p.Date, f => f.Date.Past(2));
            var purchases = purchaseFaker.Generate(300);
            _context.Purchases.AddRange(purchases);
            _context.SaveChanges();

            // Seed ItemPurchase
            var itemPurchaseFaker = new Faker<ItemPurchase>()
                .RuleFor(ip => ip.PurchaseId, f => f.Random.ListItem(purchases).Id)
                .RuleFor(ip => ip.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(ip => ip.Quantity, f => f.Random.Int(1, 20));
            var itemPurchases = itemPurchaseFaker.Generate(300);
            _context.ItemsPurchase.AddRange(itemPurchases);
            _context.SaveChanges();

            // Seed Distributions
            var distributionFaker = new Faker<Distribution>()
                .RuleFor(d => d.CenterId, f => f.Random.ListItem(centers).Id)
                .RuleFor(d => d.Date, f => f.Date.Past(1));
            var distributions = distributionFaker.Generate(300);
            _context.Distributions.AddRange(distributions);
            _context.SaveChanges();

            // Seed ItemDistribution
            var itemDistributionFaker = new Faker<ItemDistribution>()
                .RuleFor(id => id.DistributionId, f => f.Random.ListItem(distributions).Id)
                .RuleFor(id => id.ProductId, f => f.Random.ListItem(products).Id)
                .RuleFor(id => id.Quantity, f => f.Random.Int(1, 15));
            var itemDistributions = itemDistributionFaker.Generate(300);
            _context.ItemsDistribution.AddRange(itemDistributions);
            _context.SaveChanges();

            // Seed Notifications
            var notificationFaker = new Faker<Notification>()
                .RuleFor(n => n.Message, f => f.Lorem.Sentence())
                .RuleFor(n => n.IsRead, f => f.Random.Bool())
                .RuleFor(n => n.CreatedAt, f => f.Date.Past(1))
                .RuleFor(n => n.UserId, f => f.Random.ListItem(users).Id);
            var notifications = notificationFaker.Generate(300);
            _context.Notifications.AddRange(notifications);
            _context.SaveChanges();


            return Ok("Database seeded successfully.");
        }
    }
}
