using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectBlogNews.Data;
public class Program {
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Author", "Reader" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var userData = new
            {
                Admin = new
                {
                    Email = "admin@admin.com",
                    Password = "Admin123admin!",
                    FirstName = "Admin",
                    LastName = "Adminsky",
                    BirthDate = DateTime.Parse("1970-01-01")
                },
                Author = new
                {
                    Email = "author@author.com",
                    Password = "Author123author!",
                    FirstName = "John",
                    LastName = "Smith",
                    BirthDate = DateTime.Parse("1970-01-01")
                },
                Reader = new
                {
                    Email = "reader@reader.com",
                    Password = "Reader123reader!",
                    FirstName = "Lukas",
                    LastName = "Kowalsky",
                    BirthDate = DateTime.Parse("1970-01-01")
                }
            };

            foreach (var userType in new[] { userData.Admin, userData.Author, userData.Reader })
            {
                var email = userType.Email;
                var password = userType.Password;

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        EmailConfirmed = true,
                        FirstName = userType.FirstName,
                        LastName = userType.LastName,
                        BirthDate = userType.BirthDate
                    };

                    await userManager.CreateAsync(user, password);
                    await userManager.AddToRoleAsync(user, "Admin"); // You may need to adjust the role accordingly
                }
            }
        }

        app.Run();
    }

}