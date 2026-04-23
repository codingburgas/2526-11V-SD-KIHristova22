using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
        .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
        // We intentionally apply small schema adjustments via SQLite ALTER TABLE in startup seeding.
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await EnsureSqliteBookColumnsAsync(db);

    await SeedRolesAndAdminAsync(sp);

    if (!await db.Categories.AnyAsync())
    {
        db.Categories.AddRange(
            new Category { Name = "Fiction" },
            new Category { Name = "Science" },
            new Category { Name = "History" },
            new Category { Name = "Technology" }
        );
        await db.SaveChangesAsync();
    }

    // Seed additional realistic books without duplicating by title (works even when DB already has books).
    var categoryIdsByName = db.Categories
        .AsNoTracking()
        .ToDictionary(c => c.Name, c => c.Id);

    var existingTitles = await db.Books
        .AsNoTracking()
        .Select(b => b.Title)
        .ToListAsync();

    var existingTitlesSet = new HashSet<string>(
        existingTitles
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim()),
        StringComparer.OrdinalIgnoreCase);

    var seedBooks = new (string Title, string Author, string Description, decimal Price, string CategoryName)[]
    {
        ("The Great Gatsby", "F. Scott Fitzgerald",
            "A dazzling portrait of ambition and heartbreak in Jazz Age New York, told through the enigmatic Jay Gatsby. A classic story of longing, wealth, and the illusions we chase.",
            12.99m, "Fiction"),
        ("1984", "George Orwell",
            "A chilling dystopian novel about surveillance, propaganda, and the struggle for truth. Winston Smith risks everything by questioning a regime that controls reality itself.",
            11.49m, "Fiction"),
        ("To Kill a Mockingbird", "Harper Lee",
            "Seen through a child's eyes, a small Southern town confronts racism and injustice in a landmark court case. A humane, enduring novel about conscience and compassion.",
            13.75m, "Fiction"),
        ("Brave New World", "Aldous Huxley",
            "A sleek, unsettling dystopia where comfort and conditioning replace freedom and individuality. A sharp look at what people trade for stability.",
            12.50m, "Fiction"),
        ("The Hobbit", "J.R.R. Tolkien",
            "Bilbo Baggins is swept into an adventure filled with dwarves, dragons, and hidden courage. A timeless journey from homebody to hero.",
            14.25m, "Fiction"),
        ("Pride and Prejudice", "Jane Austen",
            "A witty romance about first impressions, family expectations, and personal growth. Elizabeth Bennet and Mr. Darcy learn to see beyond pride and prejudice.",
            10.99m, "Fiction"),
        ("The Catcher in the Rye", "J.D. Salinger",
            "A restless teenage voice wanders New York City, wrestling with grief, belonging, and adulthood. A candid portrait of alienation and longing.",
            11.99m, "Fiction"),
        ("The Handmaid's Tale", "Margaret Atwood",
            "In a theocratic regime, one woman fights to hold onto identity and memory. A powerful warning about control, language, and resistance.",
            13.49m, "Fiction"),
        ("The Book Thief", "Markus Zusak",
            "In Nazi Germany, a girl finds refuge in stolen books and the people she loves. A moving story about words, loss, and small acts of defiance.",
            15.99m, "Fiction"),
        ("The Alchemist", "Paulo Coelho",
            "A shepherd boy follows a dream across deserts and cities in search of treasure and purpose. A short fable about risk, faith, and listening to your life.",
            12.25m, "Fiction"),

        ("Clean Code", "Robert C. Martin",
            "Practical guidance on writing readable, maintainable software, with concrete examples and clear principles. A staple for developers who want codebases that age well.",
            33.99m, "Technology"),
        ("The Pragmatic Programmer", "Andrew Hunt and David Thomas",
            "A collection of actionable ideas for building better software and working effectively as a developer. Covers habits, design thinking, and pragmatic decision-making.",
            34.50m, "Technology"),
        ("Refactoring", "Martin Fowler",
            "A hands-on guide to improving existing code without changing behavior, using disciplined, incremental steps. Focuses on reducing complexity and increasing clarity.",
            39.99m, "Technology"),
        ("Design Patterns: Elements of Reusable Object-Oriented Software", "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
            "Classic patterns for solving common design problems in object-oriented systems. Emphasizes shared vocabulary and proven structure over clever tricks.",
            44.99m, "Technology"),
        ("Introduction to Algorithms", "Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein",
            "A comprehensive reference for fundamental algorithms and data structures, balancing theory with practical reasoning. Widely used in university courses and interviews.",
            59.99m, "Technology"),
        ("The Mythical Man-Month", "Frederick P. Brooks Jr.",
            "Essays on software engineering management, including why adding people to a late project can make it later. Still relevant for planning, estimation, and communication.",
            24.99m, "Technology"),

        ("A Brief History of Time", "Stephen Hawking",
            "An accessible tour of modern cosmology, from black holes to the origin of the universe. Hawking explains complex ideas with clarity and curiosity.",
            16.25m, "Science"),
        ("Cosmos", "Carl Sagan",
            "A sweeping introduction to the universe and our place in it, blending science with wonder. Sagan connects discoveries to culture, history, and human curiosity.",
            18.50m, "Science"),
        ("The Gene: An Intimate History", "Siddhartha Mukherjee",
            "A narrative history of genetics that weaves scientific breakthroughs with personal and ethical questions. Explains how genes shape traits and how society shapes science.",
            19.75m, "Science"),
        ("The Immortal Life of Henrietta Lacks", "Rebecca Skloot",
            "The story behind the HeLa cells that transformed medicine and the family whose consent was never asked. A compelling look at ethics, race, and scientific progress.",
            16.99m, "Science"),
        ("Silent Spring", "Rachel Carson",
            "A landmark work on the ecological effects of pesticides and the costs of industrial convenience. Helped spark modern environmental awareness and regulation.",
            14.75m, "Science"),
        ("The Selfish Gene", "Richard Dawkins",
            "A seminal explanation of evolution from the gene's-eye view, introducing ideas like memes and kin selection. A thought-provoking look at how natural selection shapes behavior.",
            15.95m, "Science"),

        ("Sapiens: A Brief History of Humankind", "Yuval Noah Harari",
            "A wide-ranging narrative of how Homo sapiens came to dominate the planet, from early foragers to modern economies. Raises big questions about culture, power, and meaning.",
            18.99m, "History"),
        ("Guns, Germs, and Steel", "Jared Diamond",
            "An ambitious attempt to explain why some societies advanced faster than others, emphasizing geography and environment. A popular, debated work that connects history with ecology.",
            17.40m, "History"),
        ("The Diary of a Young Girl", "Anne Frank",
            "A young girl's diary captures daily life, fear, and hope while hiding during World War II. A profound, personal record of history seen from inside a small room.",
            10.50m, "History"),
        ("Team of Rivals", "Doris Kearns Goodwin",
            "A deeply researched account of Abraham Lincoln and the cabinet he built from political opponents. Highlights leadership, strategy, and the costs of civil war.",
            22.99m, "History"),
        ("The Wright Brothers", "David McCullough",
            "A vivid biography of the inventors who solved powered flight through persistence and careful experimentation. A story of ingenuity, setbacks, and family support.",
            16.50m, "History"),
        ("The Silk Roads", "Peter Frankopan",
            "A re-centering of world history around trade routes linking East and West. Shows how commerce, conquest, and ideas reshaped societies across centuries.",
            18.25m, "History"),

        ("The Code Book", "Simon Singh",
            "A lively history of cryptography, from ancient ciphers to modern encryption and public-key systems. Blends real stories with the ideas that keep information secure.",
            14.99m, "Technology"),
    };

    foreach (var seed in seedBooks)
    {
        if (existingTitlesSet.Contains(seed.Title))
            continue;

        if (!categoryIdsByName.TryGetValue(seed.CategoryName, out var categoryId))
            continue;

        db.Books.Add(new Book
        {
            Title = seed.Title,
            Author = seed.Author,
            Description = seed.Description,
            Price = seed.Price,
            StockQuantity = 10,
            CategoryId = categoryId
        });

        existingTitlesSet.Add(seed.Title);
    }

    if (db.ChangeTracker.HasChanges())
        await db.SaveChangesAsync();

    await BackfillMissingBookMetadataAsync(db);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

static async Task EnsureSqliteBookColumnsAsync(ApplicationDbContext db)
{
    if (!db.Database.IsSqlite())
        return;

    // The project stores data in SQLite. We keep this simple and safe by ensuring the new columns exist.
    // If the column already exists, SQLite throws; we ignore that specific case.
    await TrySqliteAlterAsync("ALTER TABLE \"Books\" ADD COLUMN \"Author\" TEXT NOT NULL DEFAULT '';");
    await TrySqliteAlterAsync("ALTER TABLE \"Books\" ADD COLUMN \"Description\" TEXT NOT NULL DEFAULT '';");

    async Task TrySqliteAlterAsync(string sql)
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync(sql);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 &&
                                        ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
        {
            // Column already exists.
        }
    }
}

static async Task BackfillMissingBookMetadataAsync(ApplicationDbContext db)
{
    // Only update missing fields; never duplicate or overwrite existing data.
    var knownByTitle = new Dictionary<string, (string Author, string Description)>(StringComparer.OrdinalIgnoreCase)
    {
        ["The Great Gatsby"] = ("F. Scott Fitzgerald",
            "A dazzling portrait of ambition and heartbreak in Jazz Age New York, told through the enigmatic Jay Gatsby. A classic story of longing, wealth, and the illusions we chase."),
        ["1984"] = ("George Orwell",
            "A chilling dystopian novel about surveillance, propaganda, and the struggle for truth. Winston Smith risks everything by questioning a regime that controls reality itself."),
        ["To Kill a Mockingbird"] = ("Harper Lee",
            "Seen through a child's eyes, a small Southern town confronts racism and injustice in a landmark court case. A humane, enduring novel about conscience and compassion."),
        ["Brave New World"] = ("Aldous Huxley",
            "A sleek, unsettling dystopia where comfort and conditioning replace freedom and individuality. A sharp look at what people trade for stability."),
        ["The Hobbit"] = ("J.R.R. Tolkien",
            "Bilbo Baggins is swept into an adventure filled with dwarves, dragons, and hidden courage. A timeless journey from homebody to hero."),
        ["Pride and Prejudice"] = ("Jane Austen",
            "A witty romance about first impressions, family expectations, and personal growth. Elizabeth Bennet and Mr. Darcy learn to see beyond pride and prejudice."),
        ["The Catcher in the Rye"] = ("J.D. Salinger",
            "A restless teenage voice wanders New York City, wrestling with grief, belonging, and adulthood. A candid portrait of alienation and longing."),
        ["The Handmaid's Tale"] = ("Margaret Atwood",
            "In a theocratic regime, one woman fights to hold onto identity and memory. A powerful warning about control, language, and resistance."),
        ["The Book Thief"] = ("Markus Zusak",
            "In Nazi Germany, a girl finds refuge in stolen books and the people she loves. A moving story about words, loss, and small acts of defiance."),
        ["The Alchemist"] = ("Paulo Coelho",
            "A shepherd boy follows a dream across deserts and cities in search of treasure and purpose. A short fable about risk, faith, and listening to your life."),
        ["Clean Code"] = ("Robert C. Martin",
            "Practical guidance on writing readable, maintainable software, with concrete examples and clear principles. A staple for developers who want codebases that age well."),
        ["The Pragmatic Programmer"] = ("Andrew Hunt and David Thomas",
            "A collection of actionable ideas for building better software and working effectively as a developer. Covers habits, design thinking, and pragmatic decision-making."),
        ["Refactoring"] = ("Martin Fowler",
            "A hands-on guide to improving existing code without changing behavior, using disciplined, incremental steps. Focuses on reducing complexity and increasing clarity."),
        ["Design Patterns: Elements of Reusable Object-Oriented Software"] = ("Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
            "Classic patterns for solving common design problems in object-oriented systems. Emphasizes shared vocabulary and proven structure over clever tricks."),
        ["Introduction to Algorithms"] = ("Thomas H. Cormen, Charles E. Leiserson, Ronald L. Rivest, Clifford Stein",
            "A comprehensive reference for fundamental algorithms and data structures, balancing theory with practical reasoning. Widely used in university courses and interviews."),
        ["The Mythical Man-Month"] = ("Frederick P. Brooks Jr.",
            "Essays on software engineering management, including why adding people to a late project can make it later. Still relevant for planning, estimation, and communication."),
        ["Sapiens: A Brief History of Humankind"] = ("Yuval Noah Harari",
            "A wide-ranging narrative of how Homo sapiens came to dominate the planet, from early foragers to modern economies. Raises big questions about culture, power, and meaning."),
        ["A Brief History of Time"] = ("Stephen Hawking",
            "An accessible tour of modern cosmology, from black holes to the origin of the universe. Hawking explains complex ideas with clarity and curiosity."),
        ["Cosmos"] = ("Carl Sagan",
            "A sweeping introduction to the universe and our place in it, blending science with wonder. Sagan connects discoveries to culture, history, and human curiosity."),
        ["The Gene: An Intimate History"] = ("Siddhartha Mukherjee",
            "A narrative history of genetics that weaves scientific breakthroughs with personal and ethical questions. Explains how genes shape traits and how society shapes science."),
        ["The Immortal Life of Henrietta Lacks"] = ("Rebecca Skloot",
            "The story behind the HeLa cells that transformed medicine and the family whose consent was never asked. A compelling look at ethics, race, and scientific progress."),
        ["Silent Spring"] = ("Rachel Carson",
            "A landmark work on the ecological effects of pesticides and the costs of industrial convenience. Helped spark modern environmental awareness and regulation."),
        ["The Selfish Gene"] = ("Richard Dawkins",
            "A seminal explanation of evolution from the gene's-eye view, introducing ideas like memes and kin selection. A thought-provoking look at how natural selection shapes behavior."),
        ["Guns, Germs, and Steel"] = ("Jared Diamond",
            "An ambitious attempt to explain why some societies advanced faster than others, emphasizing geography and environment. A popular, debated work that connects history with ecology."),
        ["The Diary of a Young Girl"] = ("Anne Frank",
            "A young girl's diary captures daily life, fear, and hope while hiding during World War II. A profound, personal record of history seen from inside a small room."),
        ["Team of Rivals"] = ("Doris Kearns Goodwin",
            "A deeply researched account of Abraham Lincoln and the cabinet he built from political opponents. Highlights leadership, strategy, and the costs of civil war."),
        ["The Wright Brothers"] = ("David McCullough",
            "A vivid biography of the inventors who solved powered flight through persistence and careful experimentation. A story of ingenuity, setbacks, and family support."),
        ["The Silk Roads"] = ("Peter Frankopan",
            "A re-centering of world history around trade routes linking East and West. Shows how commerce, conquest, and ideas reshaped societies across centuries."),
        ["The Code Book"] = ("Simon Singh",
            "A lively history of cryptography, from ancient ciphers to modern encryption and public-key systems. Blends real stories with the ideas that keep information secure."),

        // Backward-compatible entries for older placeholder seed titles (if they already exist in the DB).
        ["The Silent Library"] = ("A. Winters",
            "A quiet town hides a loud secret in its old library, and one reader is determined to uncover it."),
        ["Orbit & Beyond"] = ("Dr. L. Nguyen",
            "A friendly introduction to space science and modern astronomy, with clear explanations for curious readers."),
        ["Blueprints of Code"] = ("M. Patel",
            "Practical patterns for building reliable software systems, focusing on readability, testing, and design tradeoffs."),
        ["Empires of the Ancient World"] = ("S. Carter",
            "A concise tour of major civilizations and how they shaped history through culture, conflict, and innovation."),
        ["Data Structures Made Simple"] = ("K. Romero",
            "Clear explanations and examples of core data structures, aimed at building strong fundamentals for developers."),
        ["The Biology of Everyday Life"] = ("Prof. J. Stein",
            "How biology explains the world around us, from sleep and food to stress and adaptation."),
        ["Letters From Yesterday"] = ("E. Hart",
            "A story told through letters, memory, and unexpected reunions that reshape the present."),
        ["Turning Points: Modern History"] = ("R. Ahmed",
            "Key events of the last two centuries that changed the world, presented with context and clear narrative."),
    };

    var toFix = await db.Books
        .Include(b => b.Category)
        .Where(b => string.IsNullOrWhiteSpace(b.Author) || string.IsNullOrWhiteSpace(b.Description))
        .ToListAsync();

    if (toFix.Count == 0)
        return;

    foreach (var book in toFix)
    {
        var title = (book.Title ?? string.Empty).Trim();
        if (knownByTitle.TryGetValue(title, out var info))
        {
            if (string.IsNullOrWhiteSpace(book.Author))
                book.Author = info.Author;
            if (string.IsNullOrWhiteSpace(book.Description))
                book.Description = info.Description;
            continue;
        }

        if (string.IsNullOrWhiteSpace(book.Author))
            book.Author = "Unknown Author";

        if (string.IsNullOrWhiteSpace(book.Description))
        {
            var categoryName = book.Category?.Name?.Trim();
            book.Description = string.IsNullOrWhiteSpace(categoryName)
                ? "A book available in our catalog, with details to be updated."
                : $"A {categoryName.ToLowerInvariant()} book available in our catalog, with details to be updated.";
        }
    }

    await db.SaveChangesAsync();
}

static async Task SeedRolesAndAdminAsync(IServiceProvider sp)
{
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    const string adminEmail = "admin@bookstore.com";
    const string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            // If user creation fails, don't crash app startup.
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
