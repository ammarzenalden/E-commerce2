using E2.configure;
using E2.Firebase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddServices();
builder.AddSwagger();
builder.AddEntityFramework();
builder.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
