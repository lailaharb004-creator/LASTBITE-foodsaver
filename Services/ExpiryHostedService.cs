using LastBiteNew.Models;
using Microsoft.EntityFrameworkCore;

namespace LastBiteNew.Services
{
    // BR-002: periodically expire packages whose pickup window has ended,
    // and mark still-Pending reservations on them as No-Show.
    public class ExpiryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiryHostedService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

        public ExpiryHostedService(IServiceScopeFactory scopeFactory, ILogger<ExpiryHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SweepAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Expiry sweep failed");
                }

                try { await Task.Delay(Interval, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }

        private async Task SweepAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.Now;

            // Expire active/sold-out packages past their pickup window.
            var expiredPackages = await ctx.FoodPackages
                .Where(p => p.PickupEndTime < now
                         && (p.Status == PackageStatus.Active || p.Status == PackageStatus.SoldOut))
                .ToListAsync(ct);
            foreach (var p in expiredPackages)
                p.Status = PackageStatus.Expired;

            // Mark uncollected pending reservations as No-Show (quantity is NOT restored — BR-008).
            var noShows = await ctx.Reservations
                .Include(r => r.FoodPackage)
                .Where(r => r.Status == ReservationStatus.Pending && r.FoodPackage.PickupEndTime < now)
                .ToListAsync(ct);
            foreach (var r in noShows)
                r.Status = ReservationStatus.NoShow;

            if (expiredPackages.Count > 0 || noShows.Count > 0)
            {
                await ctx.SaveChangesAsync(ct);
                _logger.LogInformation("Expiry sweep: {Pkgs} packages expired, {Res} reservations marked no-show.",
                    expiredPackages.Count, noShows.Count);
            }
        }
    }
}
