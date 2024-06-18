using LearningPortal.DataBaseTables;
using LearningPortal.Middlewares;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LearningPortal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true,
                    };
                });

            builder.Services.AddLogging();

            string? connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlite(connection));

            builder.Services.AddScoped<ApplicationContext>();

            builder.Services.AddScoped<ExercisesReadService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RecommendService>();
            builder.Services.AddScoped<VariantCreateService>();
            builder.Services.AddScoped<ResultsService>();
            builder.Services.AddScoped<RandomizeService>();
            builder.Services.AddScoped<GetVariantService>();
            builder.Services.AddScoped<VariantCountService>();
            builder.Services.AddScoped<StatisticService>();

            var app = builder.Build();

            app.UseStaticFiles();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.UseHandleUser();

            app.MapGet("/", async (HttpContext context, ApplicationContext db, ExercisesReadService ers, RandomizeService rs) =>
            {
                //db.Recreate(ers, rs);
                string htmlContent = await File.ReadAllTextAsync("wwwroot/html/index.html");
                Console.WriteLine($"Main page loaded.");

                return Results.Content(htmlContent, "text/html");
            });

            app.Run();
        }
    }
}