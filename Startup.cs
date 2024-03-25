//Startup.cs
using DotNetEnv;
using minimalwebapi.Authentication;
using minimalwebapi.Classes;

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
            services.AddSingleton(sp =>
            {
                return new DbConnection("mongodb+srv://lpreis:mgdhcz8dt@talentsync.iwpm9dp.mongodb.net/?retryWrites=true&w=majority&appName=talentSync", "talentSync");
            });
            services.ConfigureCors();
            services.AddSingleton<JwtTokenService>();
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

            // Use logger here
            var jwtIssuer = Configuration["Jwt:Issuer"]; //o problema e aqui
            string message = $"Jwt:Issuer from appsettings: {jwtIssuer}";
            logger.LogInformation(message: "loguei ");
            Console.WriteLine(jwtIssuer);
            Console.WriteLine(message);
            Console.WriteLine("tem alguem ai ?");
        }
    }
}
