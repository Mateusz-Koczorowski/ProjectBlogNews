using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectBlogNews.Data;
using ProjectBlogNews.Models;
using System.Globalization;

namespace ProjectBlogNews.Controllers
{
    [Authorize]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Subscription.Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Subscription == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["User"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SubscriptionStartDate,SubscriptionEndDate,UserId,Price")] Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                // Ensure that UserId is set
                subscription.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate SubscriptionStartDate and SubscriptionEndDate
                if (subscription.SubscriptionStartDate == null || subscription.SubscriptionEndDate == null)
                {
                    ModelState.AddModelError("", "Subscription start date and end date are required.");
                    ViewData["User"] = new SelectList(_context.Users, "Id", "Email", subscription.UserId);
                    return View(subscription);
                }

                // Ensure that SubscriptionEndDate is later than SubscriptionStartDate
                if (subscription.SubscriptionEndDate <= subscription.SubscriptionStartDate)
                {
                    ModelState.AddModelError("", "Subscription end date must be later than the start date.");
                    ViewData["User"] = new SelectList(_context.Users, "Id", "Email", subscription.UserId);
                    return View(subscription);
                }

                _context.Add(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, repopulate the User dropdown
            ViewData["User"] = new SelectList(_context.Users, "Id", "Email", subscription.UserId);
            return View(subscription);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Subscription == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription.FindAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", subscription.UserId);
            return View(subscription);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,SubscriptionStartDate,SubscriptionEndDate,UserId,Price")] Subscription subscription)
        {
            if (id != subscription.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Try to parse the Price using the current culture's decimal separator
                if (!decimal.TryParse(subscription.Price.ToString(), NumberStyles.Number, CultureInfo.CurrentCulture, out _))
                {
                    // If parsing fails, add a model error for the Price field
                    ModelState.AddModelError("Price", "The value is not a valid decimal number.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(subscription);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SubscriptionExists(subscription.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", subscription.User?.Id);
            return View(subscription);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Subscription == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (_context.Subscription == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Subscription' is null.");
            }
            var subscription = await _context.Subscription.FindAsync(id);
            if (subscription != null)
            {
                _context.Subscription.Remove(subscription);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionExists(int? id)
        {
            return (_context.Subscription?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult UserSubscriptions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userSubscriptions = _context.Subscription
                .Where(s => s.UserId == userId)
                .ToList();

            return View(userSubscriptions);
        }

        public IActionResult SelectSubscriptionDuration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                ModelState.AddModelError("", "The end date is earlier than the start date.");
                return View("SelectSubscriptionDuration");
            }
            if (endDate == startDate)
            {
                ModelState.AddModelError("", "Start date must be different from the end date.");
            }
            if (DateTime.Now > startDate)
            {
                ModelState.AddModelError("", "The chosen date cannot be earlier than today.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentDate = DateTime.Now;

            var overlappingSubscription = _context.Subscription
                .Any(s => s.UserId == userId &&
                          s.SubscriptionStartDate <= currentDate &&
                          s.SubscriptionEndDate >= currentDate &&
                          ((s.SubscriptionStartDate <= startDate && s.SubscriptionEndDate >= startDate) ||
                           (s.SubscriptionStartDate <= endDate && s.SubscriptionEndDate >= endDate)));

            if (overlappingSubscription)
            {
                ModelState.AddModelError("", "The selected period overlaps with another active subscription.");
                return View("SelectSubscriptionDuration");
            }

            var durationInDays = (endDate - startDate).Days;
            double price = CalculatePrice(durationInDays);

            TempData["StartDate"] = startDate;
            TempData["EndDate"] = endDate;
            TempData["DurationInDays"] = durationInDays;
            TempData["Price"] = price.ToString();

            return RedirectToAction("ConfirmPayment", TempData);
        }

        public IActionResult PaymentSuccess()
        {
            return View();
        }

        private double CalculatePrice(int durationInDays)
        {
            return 0.5 * durationInDays;
        }

        public IActionResult ConfirmPayment()
        {
            double.TryParse(TempData["Price"].ToString(), out var price);

            var model = new Subscription
            {
                SubscriptionStartDate = (DateTime)TempData["StartDate"],
                SubscriptionEndDate = (DateTime)TempData["EndDate"],
                Price = (decimal)price,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult FinalizePayment(DateTime startDate, DateTime endDate, decimal price)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subscription = new Subscription
            {
                SubscriptionStartDate = startDate,
                SubscriptionEndDate = endDate,
                UserId = userId,
                Price = price
            };

            _context.Subscription.Add(subscription);
            _context.SaveChanges();

            return RedirectToAction("PaymentSuccess");
        }
    }
}
