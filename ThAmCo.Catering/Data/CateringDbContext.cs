using Microsoft.EntityFrameworkCore;
using ThAmCo.Catering.Data;

public class CateringDbContext : DbContext
{
    public DbSet<FoodItem> FoodItems { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuFoodItem> MenuFoodItems { get; set; }
    public DbSet<FoodBooking> FoodBookings { get; set; }

    public string DbPath { get; }

    public CateringDbContext()
    {
        var folder = Environment.SpecialFolder.ApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "TheAmCo.Catering.db");

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data for FoodItems
        modelBuilder.Entity<FoodItem>().HasData(
            new FoodItem { FoodItemId = 1, Description = "Bacon", UnitPrice = 1.50m },
            new FoodItem { FoodItemId = 2, Description = "Sausage", UnitPrice = 1.20m },
            new FoodItem { FoodItemId = 3, Description = "Salad", UnitPrice = 2.00m },
            new FoodItem { FoodItemId = 4, Description = "Chicken Nuggets", UnitPrice = 2.50m },
            new FoodItem { FoodItemId = 5, Description = "Chips", UnitPrice = 1.00m },
            new FoodItem { FoodItemId = 6, Description = "Beans", UnitPrice = 0.90m },
            new FoodItem { FoodItemId = 7, Description = "Vegetables", UnitPrice = 1.80m }
        );

        // Seed data for Menus
        modelBuilder.Entity<Menu>().HasData(
            new Menu { MenuId = 1, MenuName = "Breakfast" },
            new Menu { MenuId = 2, MenuName = "Lunch" }
        );

        // Seed data for MenuFoodItems
        modelBuilder.Entity<MenuFoodItem>().HasData(
            // Breakfast Menu
            new MenuFoodItem { MenuId = 1, FoodItemId = 2 }, // Sausage
            new MenuFoodItem { MenuId = 1, FoodItemId = 6 }, // Beans
            new MenuFoodItem { MenuId = 1, FoodItemId = 1 }, // Bacon
                                                             // Lunch Menu
            new MenuFoodItem { MenuId = 2, FoodItemId = 4 }, // Chicken Nuggets
            new MenuFoodItem { MenuId = 2, FoodItemId = 7 }, // Vegetables
            new MenuFoodItem { MenuId = 2, FoodItemId = 6 }  // Beans
        );

        modelBuilder.Entity<MenuFoodItem>()
     .HasKey(mfi => new { mfi.MenuId, mfi.FoodItemId }); // Composite key

        modelBuilder.Entity<MenuFoodItem>()
            .HasOne(mfi => mfi.Menu)
            .WithMany(menu => menu.MenuFoodItems)
            .HasForeignKey(mfi => mfi.MenuId);

        modelBuilder.Entity<MenuFoodItem>()
            .HasOne(mfi => mfi.FoodItem)
            .WithMany(fi => fi.MenuFoodItems)
            .HasForeignKey(mfi => mfi.FoodItemId);
    }
}
