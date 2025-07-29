using Adidas.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

public class AdidasDbContextFactory : IDesignTimeDbContextFactory<AdidasDbContext>
{
    public AdidasDbContext CreateDbContext(string[] args)
    {
        // حاول تعرف اسم مشروع الـ Startup (MVC أو API) من args
        string startupProject = args != null && args.Length > 0 ? args[0] : "";

        // لو مفيش args، حدد أسماء المشاريع الممكنة
        string[] webProjects = { "Adidas.ClientAPI", "Adidas.AdminDashboardMVC" };

        string basePath = null;

        foreach (var project in webProjects)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "..", project);
            if (Directory.Exists(path))
            {
                basePath = path;
                break;
            }
        }

        if (basePath == null)
        {
            throw new Exception("❌ Couldn't find web project folder (Adidas.ClientAPI or Adidas.MVC). Check folder names.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AdidasDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new AdidasDbContext(optionsBuilder.Options);
    }
}
