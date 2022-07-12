using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PlatformService.Models;
using System;
using System.Linq;

namespace PlatformService.Data
{
    public static class PredDb
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SeedData(AppDbContext context)
        {
            if (!context.Platform.Any())
            {
                Console.WriteLine($"Seeding data...");
                context.Platform.AddRange(
                    new Platform() { Name="Dot Net", Publisher="Microsoft",Cost="Free"},
                    new Platform() { Name="Insomnia", Publisher="Insomnia",Cost="Free"},
                    new Platform() { Name="Kubernetes", Publisher="Cloud Native Computing Fundation",Cost="Free"}
                    );
                context.SaveChanges();

            }
            else
            {
                Console.WriteLine($"We already have date");
            }
        }
    }
}
