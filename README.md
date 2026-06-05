# 🌿 Last Bite — Food Rescue Marketplace

Last Bite is a web-based **food rescue marketplace** that connects restaurants, bakeries, cafes
and supermarkets with customers who want to buy surplus, unsold food at a discount before it is
thrown away — cutting food waste while saving people money.

Built with **ASP.NET Core MVC (.NET 9)**, **Entity Framework Core 9**, **ASP.NET Identity**, and **Bootstrap 5**.

---

## ✨ Features

### 👤 Customer
- Browse surplus food packages with a filter sidebar: keyword, city, restaurant type, food type, **pickup time**, price range, and sorting
- Paginated results (12 per page) with food photos
- Package details + **reserve** with a unique pickup code (no upfront payment)
- **My Reservations** — view code, **cancel** (up to 1h before pickup), and **leave a review**
- Submit **star ratings + reviews** after a completed pickup
- View public **restaurant profiles** (info, active packages, reviews)
- **In-app notifications** and a personal **profile** (name, phone, photo, password)

### 🏪 Restaurant Owner
- Self-service **dashboard** with KPIs
- **Publish / edit packages** with image upload (30% minimum discount enforced)
- Manage **reservations** per package, mark **completed** / **no-show**, filter by date, **export CSV**
- Cancel a package (auto-cancels pending reservations + notifies customers)
- Edit the public **restaurant profile** (logo, cover image, hours, description)

### 🛡️ Admin
- Platform **dashboard** + pending approval queue
- **Approve / reject / suspend** restaurants
- **All Users** management with **ban / unban**
- **Review moderation** (hide/show, with automatic rating recalculation)
- **Reports & Analytics** (revenue, commission, meals rescued, status breakdown chart, top restaurants)

### ⚙️ Platform
- **In-app notification system** (bell + dropdown) across all roles
- **Background job** that auto-expires packages and marks no-shows
- Commission tracking (10% on completed reservations)

---

## 🧰 Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core MVC (.NET 9) |
| ORM | Entity Framework Core 9 (SQL Server) |
| Auth | ASP.NET Core Identity (roles: Admin, RestaurantOwner, Customer) |
| UI | Razor Views, Bootstrap 5, Chart.js, SweetAlert2 |
| DB | SQL Server / SQL Server Express |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server / SQL Server Express (or LocalDB)
- (Optional) `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

### 1. Clone
```bash
git clone <your-repo-url>
cd LastBiteNew
```

### 2. Configure the database
Edit `appsettings.json` and set your connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=LastBiteNew;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Apply migrations
```bash
dotnet ef database update
```

### 4. Run
```bash
dotnet run
```
Then open the URL shown in the console (e.g. `https://localhost:7xxx`).

> On first run the app **seeds** roles, an admin account, and demo data (restaurants, packages with photos, customers, a sample review).

---

## 🔑 Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@lastbite.com` | `Admin@123` |
| Restaurant Owner | `owner1@lastbite.com` (also owner2 / owner3) | `Owner@123` |
| Customer | `customer@lastbite.com` (also customer2) | `Customer@123` |

---

## 📂 Project Structure

```
LastBiteNew/
├── Controllers/            # Public + customer controllers (Home, Account, Packages, Reservations, Reviews, Restaurants, Notifications, Profile)
├── Areas/
│   ├── Admin/              # Admin portal (Dashboard, Restaurants, Users, Reviews, Reports)
│   └── Restaurant/         # Owner portal (Dashboard, Packages, Reservations, Profile)
├── Models/                 # EF Core entities + enums
├── ViewModels/             # Account, Customer, Owner, Admin view models
├── Views/                  # Razor views + shared layouts/partials
├── Data/                   # ApplicationDbContext, SeedData, Migrations
├── Services/               # NotificationHelper, ExpiryHostedService (background job)
├── ViewComponents/         # NotificationBell
├── wwwroot/                # CSS, uploads (restaurant/package/user images)
├── ARCHITECTURE-STATUS.md  # Build status vs. the software documentation
└── DOCUMENTATION.md        # Technical documentation
```

---

## 🗺️ Scope

Implemented to MVP per the *Last Bite Software Documentation v1.0*. **Out of scope** (by team decision):
- Email notifications (in-app notifications are implemented)
- Forgot / reset password

See **[DOCUMENTATION.md](DOCUMENTATION.md)** for full details and **[ARCHITECTURE-STATUS.md](ARCHITECTURE-STATUS.md)** for the requirement-by-requirement status.

---

## 📄 License
For educational use.
