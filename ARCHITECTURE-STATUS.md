# Last Bite — Architecture & Build Status

Status of the implementation vs. **LastBite Complete Software Documentation v1.0**
(FR = Functional Requirements §7, P = Website Pages §10, BR = Business Rules §6).
Last updated: 2026-06-05.

Legend: ✅ done · 🟡 partial · ⛔ not started · ❌ out of scope (per team decision)

> Out of scope: **Email notifications** and **Forgot/Reset password (P-08)** — intentionally excluded.

## Architecture

```
ASP.NET Core MVC (.NET 9) + EF Core 9 + ASP.NET Identity + Bootstrap 5
Roles: Admin · RestaurantOwner · Customer  (ApplicationUser)

PUBLIC / CUSTOMER (root)   Home(+About) · Account(auth) · Packages(browse) · Reservations
                           · Reviews · Restaurants(profile) · Notifications · Profile
AREA: Restaurant           Dashboard · Packages(CRUD+image) · Reservations · Profile(edit)
AREA: Admin                Dashboard · Restaurants(moderate) · Users · Reviews · Reports

Cross-cutting: NotificationHelper + NotificationBell view component (in-app)
Data: Models → ApplicationDbContext → EF Core → DB (dotnet-ef installed)
```

## Functional Requirements

| FR | Requirement | Status |
|----|-------------|--------|
| FR-01 | Restaurant registration (+logo) | ✅ |
| FR-02 | Admin approve/reject | ✅ |
| FR-03 | Package publishing (+image) | 🟡 BR-004 30% rule not enforced |
| FR-04 | Browsing + filters | ✅ (no pagination) |
| FR-05 | Reservation | ✅ |
| FR-06 | Cancellation (customer) | ✅ |
| FR-07 | Completion (restaurant) | ✅ |
| FR-08 | No-show handling | 🟡 manual only (no auto job, BR-002) |
| FR-09 | Customer review submission | ✅ |
| FR-10 | Restaurant public profile | ✅ |
| FR-11 | My Reservations | ✅ |
| FR-12 | Restaurant dashboard | ✅ |
| FR-13 | Admin dashboard | ✅ |

## Pages

| Page | Status | Page | Status |
|------|--------|------|--------|
| P-01 Home | ✅ | P-12 Restaurant Dashboard | ✅ |
| P-02 Browse | ✅ | P-13 My Packages | ✅ |
| P-03 Package Detail | ✅ | P-14 Publish/Edit Package | ✅ |
| P-04 Restaurant Public Profile | ✅ | P-15 Package Reservations | ✅ per-package view |
| P-05 About/How It Works | ✅ | P-16 All Reservations | ✅ date filter + CSV |
| P-06 Login | ✅ | P-17 Restaurant Profile Edit | ✅ |
| P-07 Customer Registration | ✅ | P-18 Admin Dashboard | ✅ |
| P-08 Forgot/Reset Password | ❌ out of scope | P-19 Pending Approvals | ✅ |
| P-09 My Reservations | ✅ | P-20 All Restaurants Mgmt | ✅ |
| P-10 Submit Review | ✅ | P-21 All Users Mgmt | ✅ |
| P-11 Customer Profile | ✅ | P-22 Platform Reports | ✅ |

## Cross-cutting
- Notifications §19: ✅ in-app (bell + /Notifications page; FR-01/02/05/06/07 + BR-005)
- BR-005 restaurant package cancel: ✅
- BR-015 admin review moderation (hide/show + rating recalc): ✅
- Email: ❌ out of scope

## Gaps — done
- ✅ BR-004 30% minimum discount enforced (package create/edit)
- ✅ Browse pagination (12/page)
- ✅ BR-002 automatic package expiry + no-show (ExpiryHostedService, every 15 min)
- ✅ Toast notifications (replaced alert banners across all layouts)
- ✅ Home testimonials section (P-01)
- ✅ Restaurant reservations: per-package view + date filter + CSV export (P-15/P-16)
- ✅ My Reservations: "last 7 days" recent-activity window (P-09)
- ✅ Cancel UX: tooltip explaining the 1-hour rule

## Remaining work
**None** — all documentation items are implemented, except the two intentionally out of scope:
- ❌ Email notifications
- ❌ Forgot/Reset password (P-08)

The app is feature-complete against LastBite Software Documentation v1.0 (MVP).
