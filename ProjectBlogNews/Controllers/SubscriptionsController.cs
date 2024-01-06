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
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Subscription.Include(s => s.User);
            return View(await applicationDbContext.ToListAsync());
        }

        
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

        
        public IActionResult Create()
        {
            ViewData["User"] = new SelectList(_context.Users, "Email", "Id");
            return View();
        }

       
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

        
        public IActionResult SelectSubscriptionDuration()
        {
            return View();
        }

       
        [HttpPost]
        public IActionResult ProcessPayment(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                
                ModelState.AddModelError("", "Data zakończenia jest wcześniejsza niż data rozpoczęcia.");
                return View("SelectSubscriptionDuration");
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
                
                ModelState.AddModelError("", "Wybrany został termin pokrywający się z inną aktywną subskrypcją.");
                return View("SelectSubscriptionDuration");
            }

            var durationInDays = (endDate - startDate).Days;
            decimal price = CalculatePrice(durationInDays);

            //test
            Console.WriteLine($"StartDate: {startDate}, EndDate: {endDate}");

            TempData["StartDate"] = startDate.ToShortDateString();
            TempData["EndDate"] = endDate.ToShortDateString();
            TempData["DurationInDays"] = durationInDays;
            TempData["Price"] = price.ToString("F2");

            
            
            return RedirectToAction("ConfirmPayment");
        }


        
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        
        private decimal CalculatePrice(int durationInDays)
        {
            
            return 0.5m * durationInDays;
        }

        public IActionResult ConfirmPayment()
        {
        
            var startDate = DateTime.Parse((string)TempData["StartDate"]);
            var endDate = DateTime.Parse((string)TempData["EndDate"]);
            decimal price;

            try
            {
                price = decimal.Parse((string)TempData["Price"], CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                
                return RedirectToAction("SelectSubscriptionDuration");
            }

            var model = new SubscriptionConfirmationViewModel
            {
                StartDate = (string)TempData["StartDate"],
                EndDate = (string)TempData["EndDate"],
                DurationInDays = (int)TempData["DurationInDays"],
                Price = price
            };

            return View(model);
        }
        [HttpPost]
        public IActionResult FinalizePayment()
        {
                       
            var startDate = DateTime.Parse((string)TempData["StartDate"]);
            var endDate = DateTime.Parse((string)TempData["EndDate"]);
            var price = decimal.Parse((string)TempData["Price"], CultureInfo.InvariantCulture);

            
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
