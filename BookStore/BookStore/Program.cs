using BookStore.Data;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { Name = "Fiction" },
            new Category { Name = "Science" },
            new Category { Name = "History" },
            new Category { Name = "Technology" }
        );
        db.SaveChanges();
    }

    if (!db.Books.Any())
    {
        var categoryIdsByName = db.Categories
            .AsNoTracking()
            .ToDictionary(c => c.Name, c => c.Id);

        var books = new List<Book>
        {
            CreateSeedBook("The Silent Library", "A. Winters", 14.99m, "A quiet town hides a loud secret in its old library.", "Fiction"),
            CreateSeedBook("Orbit & Beyond", "Dr. L. Nguyen", 19.50m, "A friendly introduction to space science and modern astronomy.", "Science"),
            CreateSeedBook("Blueprints of Code", "M. Patel", 29.99m, "Practical patterns for building reliable software systems.", "Technology"),
            CreateSeedBook("Empires of the Ancient World", "S. Carter", 24.00m, "A concise tour of major civilizations and how they shaped history.", "History"),
            CreateSeedBook("Data Structures Made Simple", "K. Romero", 27.75m, "Clear explanations and examples of core data structures.", "Technology"),
            CreateSeedBook("The Biology of Everyday Life", "Prof. J. Stein", 21.25m, "How biology explains the world around us, from sleep to food to stress.", "Science"),
            CreateSeedBook("Letters From Yesterday", "E. Hart", 12.95m, "A story told through letters, memory, and unexpected reunions.", "Fiction"),
            CreateSeedBook("Turning Points: Modern History", "R. Ahmed", 18.40m, "Key events of the last two centuries that changed the world.", "History"),
        };

        db.Books.AddRange(books);
        db.SaveChanges();

        Book CreateSeedBook(string title, string author, decimal price, string description, string categoryName)
        {
            var book = new Book
            {
                Title = title,
                Price = price,
                StockQuantity = 10,
                CategoryId = categoryIdsByName[categoryName]
            };

            // Only set these if the properties exist on the current Book model.
            SetOptionalString(book, "Author", author);
            SetOptionalString(book, "Description", description);

            return book;
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void SetOptionalString(object target, string propertyName, string value)
{
    var prop = target.GetType().GetProperty(propertyName);
    if (prop is null || !prop.CanWrite || prop.PropertyType != typeof(string))
        return;

    prop.SetValue(target, value);
}