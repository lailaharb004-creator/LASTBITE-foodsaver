# Last Bite — Technical Documentation

Implementation reference for the Last Bite food-rescue marketplace.
Stack: **ASP.NET Core MVC (.NET 9) · EF Core 9 · ASP.NET Identity · Bootstrap 5 · SQL Server**.

---

## 1. Overview

Last Bite is a digital bridge between food businesses with surplus stock and customers who buy it
at a discount before it is discarded. The transaction model is **reserve → pickup → pay at the
restaurant** (no online payment in the MVP); a 10% commission is recorded per completed reservation.

---

## 2. Architecture

```
ASP.NET Core MVC (.NET 9)
│
├── Identity / Roles ──> Admin · RestaurantOwner · Customer   (ApplicationUser)
│
├── PUBLIC / CUSTOMER (root controllers)
│     Home(+About) · Account(auth) · Packages(browse) · Reservations
│     · Reviews · Restaurants(public profile) · Notifications · Profile
│
├── AREA: Restaurant   (owner portal, /Restaurant/...)
│     Dashboard · Packages(CRUD + image) · Reservations(manage + CSV) · Profile(edit)
│
└── AREA: Admin        (platform portal, /Admin/...)
      Dashboard · Restaurants(moderate) · Users(ban) · Reviews(moderate) · Reports

Cross-cutting:
  Services/NotificationHelper        — queues in-app notifications
  Services/ExpiryHostedService       — background job (auto expiry / no-show)
  ViewComponents/NotificationBell    — bell + unread badge in all layouts

Data flow:  Controllers → ApplicationDbContext (EF Core) → SQL Server
Routing:    Attribute routes (public controllers) + conventional + area routes
```

### Layouts
- `Views/Shared/_Layout.cshtml` — public / customer
- `Areas/Restaurant/Views/Shared/_RestaurantLayout.cshtml` — owner (sidebar)
- `Areas/Admin/Views/Shared/_AdminLayout.cshtml` — admin (sidebar)
- `Views/Shared/_Toasts.cshtml` — shared toast notifications (TempData)

---

## 3. User Roles

| Role | Capabilities |
|------|--------------|
| **Customer** | Browse, reserve, cancel, review, manage profile, notifications |
| **RestaurantOwner** | Publish/manage packages, manage reservations, edit restaurant profile (requires admin approval before publishing) |
| **Admin** | Approve/reject/suspend restaurants, manage users, moderate reviews, view reports |

Roles and an admin account are created at startup by `Data/SeedData.cs`.

---

## 4. Domain Model (EF Core entities)

| Entity | Key fields | Relationships |
|--------|-----------|---------------|
| **ApplicationUser** (IdentityUser) | FullName, ProfilePicturePath, IsActive, CreatedAt | 1–1 Restaurant; 1–* Reservations, Reviews, Notifications |
| **Restaurant** | Name, Category, Address, City, PhoneNumber, Description, LogoPath, CoverImagePath, OperatingHours, Status, AverageRating, TotalReviews | belongs to Owner; 1–* FoodPackages, Reviews |
| **FoodPackage** | Name, Description, OriginalPrice, DiscountedPrice, TotalQuantity, RemainingQuantity, PickupStartTime, PickupEndTime, Status, FoodType, AllergenInfo, ImagePath | belongs to Restaurant; 1–* Reservations |
| **Reservation** | Quantity, TotalPrice, CommissionAmount, Status, ReservationCode, ReservedAt, CompletedAt, CancellationReason | belongs to FoodPackage (via PackageId) + Customer; 1–1 Review |
| **Review** | Rating (1–5), Comment, IsVisible, CreatedAt | belongs to Reservation, Restaurant, Customer |
| **Notification** | Title, Message, Type, IsRead, RelatedEntityId, CreatedAt | belongs to User |

### Enums
- `RestaurantCategory`: Restaurant, Bakery, Cafe, Supermarket, FoodStore
- `RestaurantStatus`: Pending, Approved, Rejected, Suspended
- `PackageStatus`: Active, SoldOut, Expired, Cancelled
- `FoodType`: Mixed, Vegan, Vegetarian, ContainsMeat
- `ReservationStatus`: Pending, Completed, CancelledByCustomer, CancelledByRestaurant, NoShow
- `NotificationType`: Reservation, RestaurantApproval, PackageCancellation, ReviewReminder, SystemAlert

> The `Reservation.FoodPackage` relationship is explicitly mapped to `PackageId` in `ApplicationDbContext`
> to avoid an EF shadow foreign key. Cascade paths on Reservation/Review use `Restrict`.

---

## 5. Business Rules (implemented)

| Rule | Description |
|------|-------------|
| **BR-002** | Packages past their pickup window auto-move to **Expired**; uncollected pending reservations → **No-Show** (background job, every 15 min). |
| **BR-003** | Each reservation decrements quantity; at 0 the package becomes **SoldOut**. |
| **BR-004** | Discounted price must be **at least 30% below** the original (enforced on create/edit). |
| **BR-005** | Cancelling a package cancels all pending reservations and **notifies** those customers. |
| **BR-011** | A customer can cancel **only up to 1 hour before** pickup start. |
| **BR-013/14** | One review per reservation, only after **Completed**, within **48 hours**. |
| **BR-015** | Admin can hide reviews; hidden reviews are excluded from rating calculations. |
| **BR-016/17** | 10% commission recorded on the reservation at creation time. |

---

## 6. Key Flows

### Customer reservation
1. Browse `/Packages` → open a package → **Reserve** (must be logged in as Customer).
2. A `Reservation` is created (`Pending`), quantity decremented, a unique `LB-XXXX` code generated, owner notified.
3. **My Reservations** shows the code. Customer may **Cancel** (>1h before pickup) — quantity is returned, owner notified.
4. After the owner marks **Completed**, the customer may **Leave a Review** (within 48h).

### Restaurant owner
1. Register as a restaurant → status **Pending**; admins notified.
2. After admin **approval**, publish packages (with photo, ≥30% discount).
3. Manage reservations: **Complete** (customer notified) or **No-Show**; filter by date/package; **Export CSV**.

### Admin
1. **Pending Approvals** → Approve/Reject (owner notified).
2. **Users** → Ban/Unban (banned users are blocked at login).
3. **Reviews** → Hide/Show (rating recalculated).
4. **Reports** → revenue, commission, meals rescued, status chart, top restaurants.

---

## 7. Notification System (in-app)

- `Services/NotificationHelper.Add(...)` queues a `Notification` on the current `DbContext`
  (persisted by the caller's `SaveChangesAsync`).
- `ViewComponents/NotificationBellViewComponent` renders the bell + unread count + recent items;
  embedded in all three layouts.
- `/Notifications` lists all notifications and marks them read on view.
- Events wired: restaurant registration → admins; approval/rejection → owner; new reservation → owner;
  completion → customer; customer cancel → owner; package cancel → affected customers.

> Email notifications are **out of scope** in this build.

---

## 8. Background Job

`Services/ExpiryHostedService` (registered via `AddHostedService`) runs at startup and every 15 minutes:
- Moves `Active`/`SoldOut` packages whose `PickupEndTime` has passed → `Expired`.
- Moves still-`Pending` reservations on those packages → `NoShow` (quantity not restored).

---

## 9. Security & Auth

- ASP.NET Core Identity with role-based authorization (`[Authorize(Roles="...")]`).
- Anti-forgery tokens on all POST forms.
- Banned accounts (`IsActive = false`) are rejected at login.
- Image uploads are extension-validated and stored under `wwwroot/uploads/`.
- Password policy: min 8 chars, upper + lower + digit.

---

## 10. Configuration & Setup

1. Set `ConnectionStrings:DefaultConnection` in `appsettings.json`.
2. `dotnet ef database update` (or run the app — migrations + seed run on startup).
3. `dotnet run`.

### Seeded data
Roles, an admin, 3 approved restaurants, 5 active packages (with photos + future pickup windows),
2 customers, and one completed reservation with a 5★ review. Demo credentials are in the README.

---

## 11. Requirement Status

See **ARCHITECTURE-STATUS.md** for the full FR/Page/BR checklist. Summary: all 13 functional
requirements and 22 pages implemented, except **email** and **forgot/reset password** (out of scope).
