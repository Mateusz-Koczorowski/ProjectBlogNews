using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectBlogNews.Data;
using ProjectBlogNews.Models;

namespace ProjectBlogNews.Controllers
{
    [Authorize(Roles = "Admin, Author")]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ArticlesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Article.Include(a => a.Author);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Article == null)
            {
                return NotFound();
            }

            var article = await _context.Article
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,FreeContent,PremiumContent,AuthorId,ImageFileName")] Article article, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;
                article.AuthorId = userId;
                article.Author = user;
                article.ReleaseDate = DateTime.Now;

                // Get the wwwroot path
                var webRootPath = _webHostEnvironment.WebRootPath;

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(imageFile.FileName)}";
                    var filePath = Path.Combine(webRootPath, "ArticleImages", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    article.ImageFileName = fileName;
                }

                _context.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", article.AuthorId);
            return View(article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Article == null)
            {
                return NotFound();
            }

            var article = await _context.Article.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", article.AuthorId);
            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,ReleaseDate,FreeContent,PremiumContent,AuthorId,ImageFileName")] Article article, IFormFile imageFile)
        {
            if (id == null)
            {
                return NotFound();
            }

            var existingArticle = await _context.Article.FindAsync(id);

            if (existingArticle == null)
            {
                return NotFound();
            }

            // Store the original AuthorId
            var originalAuthorId = existingArticle.AuthorId;

            // Update other properties
            existingArticle.Title = article.Title;
            existingArticle.ReleaseDate = article.ReleaseDate;
            existingArticle.FreeContent = article.FreeContent;
            existingArticle.PremiumContent = article.PremiumContent;

            // Handle image update
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(imageFile.FileName)}";
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "ArticleImages", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Delete the previous image file
                if (!string.IsNullOrEmpty(existingArticle.ImageFileName))
                {
                    var previousFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "ArticleImages", existingArticle.ImageFileName);
                    if (System.IO.File.Exists(previousFilePath))
                    {
                        System.IO.File.Delete(previousFilePath);
                    }
                }

                existingArticle.ImageFileName = fileName;
            }

            // Restore the original AuthorId
            existingArticle.AuthorId = originalAuthorId;

            try
            {
                _context.Update(existingArticle);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(article.Id))
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

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Article
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Article.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            // Remove the image file
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "ArticleImages", article.ImageFileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool ArticleExists(int? id)
        {
            return (_context.Article?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [AllowAnonymous]
        public async Task<IActionResult> DetailsView(int? id)
        {
            var subscriptionIsActive = false;

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user != null)
                {
                    var userSubscriptions = await _context.Subscription
                        .Where(x => x.UserId == user.Id)
                        .ToListAsync();

                    subscriptionIsActive = userSubscriptions.Any(item => item.IsActive);
                }
            }

            ViewData["userHasPremium"] = subscriptionIsActive;

            if (id == null || _context.Article == null)
            {
                return NotFound();
            }

            var article = await _context.Article
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            ViewData["AvailablePremiumContentForNonPremium"] = article.GetAvailablePremiumContentForNonPremium();

            if (article == null)
            {
                return NotFound();
            }

            return View("DetailsView", article);
        }
    }
}
