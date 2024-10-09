using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SurveyBasket.Persistence.EntitiesConfigurations;
using System.Reflection;

namespace SurveyBasket.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : 
    IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Poll> Polls { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
