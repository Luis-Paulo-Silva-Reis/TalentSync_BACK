using minimalwebapi.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DbConnection>(sp =>
{
    return new DbConnection("mongodb+srv://lpreis:mgdhcz8dt@talentsync.iwpm9dp.mongodb.net/?retryWrites=true&w=majority&appName=talentSync", "talentSync");
});
builder.Services.ConfigureCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers(); 

app.Run();
