
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Serilog;

namespace SurveyBasket;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDependencies(builder.Configuration);

        builder.Host.UseSerilog((contex, configuration) =>
            configuration.ReadFrom.Configuration(contex.Configuration)
        );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();

        app.UseHangfireDashboard("/jobs", new DashboardOptions
        {
            Authorization =
            [
                new HangfireCustomBasicAuthenticationFilter{
                    User = app.Configuration.GetValue<string>("HangfireSettings:Username"),
                    Pass = app.Configuration.GetValue<string>("HangfireSettings:Password")
                }
            ],
            DashboardTitle = "Survey Basket Dashboard"
        });

        // cors must write before authorization
        app.UseCors();

        app.UseAuthorization();

        app.MapControllers();

        app.UseExceptionHandler();

        app.Run();
    }
}