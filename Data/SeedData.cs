namespace LastBiteNew.Data
{
    using LastBiteNew.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Seed roles first
            foreach (var role in new[] { "Admin", "RestaurantOwner", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin account
            const string adminEmail = "admin@lastbite.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Platform Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };
                if ((await userManager.CreateAsync(admin, "Admin@123")).Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── Demo data (seeded once) ───────────────────────────────
            if (await userManager.FindByEmailAsync("owner1@lastbite.com") != null)
                return; // already seeded

            async Task<ApplicationUser> CreateUserAsync(string email, string name, string role, string password)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
                return user;
            }

            var owner1 = await CreateUserAsync("owner1@lastbite.com", "Layla Owner", "RestaurantOwner", "Owner@123");
            var owner2 = await CreateUserAsync("owner2@lastbite.com", "Sami Owner", "RestaurantOwner", "Owner@123");
            var owner3 = await CreateUserAsync("owner3@lastbite.com", "Nadia Owner", "RestaurantOwner", "Owner@123");

            var sara = await CreateUserAsync("customer@lastbite.com", "Sara Hassan", "Customer", "Customer@123");
            await CreateUserAsync("customer2@lastbite.com", "Omar Khalid", "Customer", "Customer@123");

            var sunrise = new Restaurant
            {
                OwnerId = owner1.Id,
                Name = "Sunrise Bakery",
                Category = RestaurantCategory.Bakery,
                Address = "Rainbow St 12",
                City = "Amman",
                PhoneNumber = "+962790000001",
                Description = "Fresh bread, pastries and cakes baked daily. We hate seeing good food go to waste!",
                OperatingHours = "Daily 8:00 – 20:00",
                Status = RestaurantStatus.Approved,
                ApprovedAt = DateTime.UtcNow
            };
            var olive = new Restaurant
            {
                OwnerId = owner2.Id,
                Name = "Olive Tree Cafe",
                Category = RestaurantCategory.Cafe,
                Address = "Abdoun Circle 5",
                City = "Amman",
                PhoneNumber = "+962790000002",
                Description = "Cozy cafe serving sandwiches, salads and great coffee.",
                OperatingHours = "Daily 9:00 – 23:00",
                Status = RestaurantStatus.Approved,
                ApprovedAt = DateTime.UtcNow
            };
            var grocer = new Restaurant
            {
                OwnerId = owner3.Id,
                Name = "Daily Grocer",
                Category = RestaurantCategory.Supermarket,
                Address = "University St 30",
                City = "Irbid",
                PhoneNumber = "+962790000003",
                Description = "Neighborhood grocery rescuing fresh produce every evening.",
                OperatingHours = "Daily 7:00 – 22:00",
                Status = RestaurantStatus.Approved,
                ApprovedAt = DateTime.UtcNow
            };
            context.Restaurants.AddRange(sunrise, olive, grocer);
            await context.SaveChangesAsync();

            var now = DateTime.Now;
            FoodPackage P(Restaurant r, string name, FoodType type, decimal orig, decimal disc, int qty, double startHrs, double endHrs, string desc, string img) =>
                new FoodPackage
                {
                    RestaurantId = r.RestaurantId,
                    Name = name,
                    Description = desc,
                    FoodType = type,
                    OriginalPrice = orig,
                    DiscountedPrice = disc,
                    TotalQuantity = qty,
                    RemainingQuantity = qty,
                    PickupStartTime = now.AddHours(startHrs),
                    PickupEndTime = now.AddHours(endHrs),
                    ImagePath = img,
                    Status = PackageStatus.Active
                };

            context.FoodPackages.AddRange(
                P(sunrise, "Bakery Surprise Box", FoodType.Mixed, 9.00m, 3.00m, 5, 2, 5, "A mix of today's unsold pastries and bread.", "/uploads/packages/seed-bakerybox.jpg"),
                P(sunrise, "Vegan Pastry Bag", FoodType.Vegan, 8.00m, 2.50m, 3, 3, 6, "Plant-based pastries and rolls.", "/uploads/packages/seed-pastry.jpg"),
                P(olive, "Lunch Leftover Box", FoodType.ContainsMeat, 12.00m, 4.00m, 4, 4, 7, "Hearty sandwiches and sides from today's menu.", "/uploads/packages/seed-lunchbox.jpg"),
                P(olive, "Veggie Sandwich Combo", FoodType.Vegetarian, 7.00m, 2.00m, 6, 5, 8, "Vegetarian sandwiches plus a drink.", "/uploads/packages/seed-veggiesandwich.jpg"),
                P(grocer, "Fresh Veg Basket", FoodType.Vegan, 15.00m, 5.00m, 8, 2, 6, "Assorted fresh vegetables nearing sell-by.", "/uploads/packages/seed-vegbasket.jpg")
            );

            // A past package + completed reservation + review (so a profile shows a rating)
            var pastLoaf = P(sunrise, "Yesterday's Loaf", FoodType.Mixed, 6.00m, 2.00m, 5, -24, -22, "End-of-day bread.", "/uploads/packages/seed-loaf.jpg");
            pastLoaf.Status = PackageStatus.Expired;
            pastLoaf.RemainingQuantity = 4;
            context.FoodPackages.Add(pastLoaf);
            await context.SaveChangesAsync();

            var demoReservation = new Reservation
            {
                PackageId = pastLoaf.PackageId,
                CustomerId = sara.Id,
                Quantity = 1,
                TotalPrice = 2.00m,
                CommissionAmount = 0.20m,
                Status = ReservationStatus.Completed,
                ReservationCode = "LB-DEMO0001",
                ReservedAt = DateTime.UtcNow.AddDays(-1),
                CompletedAt = DateTime.UtcNow.AddHours(-20)
            };
            context.Reservations.Add(demoReservation);
            await context.SaveChangesAsync();

            context.Reviews.Add(new Review
            {
                ReservationId = demoReservation.ReservationId,
                RestaurantId = sunrise.RestaurantId,
                CustomerId = sara.Id,
                Rating = 5,
                Comment = "Amazing fresh bread for a fraction of the price. Highly recommend!",
                IsVisible = true,
                CreatedAt = DateTime.UtcNow.AddHours(-19)
            });
            await context.SaveChangesAsync();

            sunrise.AverageRating = 5.00m;
            sunrise.TotalReviews = 1;
            await context.SaveChangesAsync();
        }
    }
}
