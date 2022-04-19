using Microsoft.EntityFrameworkCore;
using UnitedNationsTelegram.Services.Models;
using UnitedNationsTelegram.Services.Services;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy => { policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin(); });
});


builder.Services.AddDbContext<UNContext>(op => op.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PollService>();
builder.Services.AddScoped<SanctionService>();
builder.Services.AddScoped<MembersService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();