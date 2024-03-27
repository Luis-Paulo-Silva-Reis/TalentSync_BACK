using DotNetEnv;
using minimalwebapi.Authentication;
using minimalwebapi.Classes;
using validators.userValidator;
using MongoDB.Driver;
using minimalwebapi.models.PersonModel;

namespace minimalwebapi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Env.Load();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSingleton<IMongoClient>(sp =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DB_StringConnection") ?? Configuration["MongoDB:ConnectionString"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("MongoDB connection string is not configured.");
                }
                return new MongoClient(connectionString);
            });

            // Register MongoDB database
            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var dbName = Environment.GetEnvironmentVariable("DB_Collection") ?? Configuration["MongoDB:DatabaseName"];
                if (string.IsNullOrEmpty(dbName))
                {
                    throw new InvalidOperationException("MongoDB database name is not configured.");
                }
                return client.GetDatabase(dbName);
            });

            // Register IMongoCollection for PersonModel
            services.AddScoped(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                return database.GetCollection<PersonModel>("user");
            });

            services.AddScoped<ValidationService>();
            services.AddSingleton<JwtTokenService>();
            services.ConfigureCors();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var jwtIssuer = Configuration["Jwt:Issuer"];
            logger.LogInformation($"Jwt:Issuer from appsettings: {jwtIssuer}");
        }
    }
}
