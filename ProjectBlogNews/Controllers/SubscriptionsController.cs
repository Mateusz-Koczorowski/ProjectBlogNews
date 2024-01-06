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


namespace ProjectBlogNews.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Subscription.Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Subscriptions/Details/5
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

        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            ViewData["User"] = new SelectList(_context.Users, "Email", "Id");
            return View();
        }

        // POST: Subscriptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SubscriptionStartDate,SubscriptionEndDate,UserId,Price")] Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subscription);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["User"] = new SelectList(_context.Users, "Id", "Id", subscription.User.Email);
            return View(subscription);
        }

        // GET: Subscriptions/Edit/5
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
            ViewData["User"] = new SelectList(_context.Users, "Id", "Id", subscription.User.Email);
            return View(subscription);
        }

        // POST: Subscriptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                try
                {
                    _context.Update(subscription);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["User"] = new SelectList(_context.Users, "Id", "Id", subscription.User.Email);
            return View(subscription);
        }

        // GET: Subscriptions/Delete/5
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

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (_context.Subscription == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Subscription'  is null.");
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

        // Action to display subscription selection form
        public IActionResult SelectSubscriptionDuration()
        {
            return View();
        }

        // Action to process subscription payment
        [HttpPost]
        public IActionResult ProcessPayment(int durationInDays)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Calculate price based on durationInDays
            decimal price = CalculatePrice(durationInDays);

            // Create a new subscription record
            var subscription = new Subscription
            {
                SubscriptionStartDate = DateTime.Now,
                SubscriptionEndDate = DateTime.Now.AddDays(durationInDays),
                UserId = userId,
                Price = price
            };

            _context.Subscription.Add(subscription);
            _context.SaveChanges();

            // Redirect to a confirmation page or show a success message
            return RedirectToAction("PaymentSuccess");
        }

        // Action for payment success
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        // Helper method to calculate subscription price
        private decimal CalculatePrice(int durationInDays)
        {
            // Add your price calculation logic here
            // Example: $1 per day
            return 1.0m * durationInDays;
        }

    }
}
