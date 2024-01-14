using DotNetEnv;
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

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
            Env.Load();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Console.WriteLine($"Admin_BirthDate: {Environment.GetEnvironmentVariable("Admin_Email")}");
            var userData = new
            {
                Admin = new
                {
                    Email = Environment.GetEnvironmentVariable("Admin_Email"),
                    Password = Environment.GetEnvironmentVariable("Admin_Password"),
                    FirstName = Environment.GetEnvironmentVariable("Admin_FirstName"),
                    LastName = Environment.GetEnvironmentVariable("Admin_LastName"),
                    BirthDate = DateTime.Parse(Environment.GetEnvironmentVariable("Admin_BirthDate"))
                },
                Author = new
                {
                    Email = Environment.GetEnvironmentVariable("Author_Email"),
                    Password = Environment.GetEnvironmentVariable("Author_Password"),
                    FirstName = Environment.GetEnvironmentVariable("Author_FirstName"),
                    LastName = Environment.GetEnvironmentVariable("Author_LastName"),
                    BirthDate = DateTime.Parse(Environment.GetEnvironmentVariable("Author_BirthDate"))
                },
                Reader = new
                {
                    Email = Environment.GetEnvironmentVariable("Reader_Email"),
                    Password = Environment.GetEnvironmentVariable("Reader_Password"),
                    FirstName = Environment.GetEnvironmentVariable("Reader_FirstName"),
                    LastName = Environment.GetEnvironmentVariable("Reader_LastName"),
                    BirthDate = DateTime.Parse(Environment.GetEnvironmentVariable("Reader_BirthDate"))
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