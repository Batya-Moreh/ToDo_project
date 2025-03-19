using TodoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

// הוספת שירות DbContext עם חיבור ל-MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mySql")
    )
);
//cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("policy", policyBuilder => {
         policyBuilder.WithOrigins("http://localhost:3000")
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

//swager
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // 👈 כאן בונים את האפליקציה, ורק אחר כך אפשר להשתמש ב-app

app.UseCors("policy");
app.UseSwagger();

// קונפיגורציה של צינור הבקשות HTTP
if (app.Environment.IsDevelopment())
{
    // app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();


// הצגת כל הפריטים
app.MapGet("/items", async (ToDoDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items);
});
app.MapGet("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    return Results.Ok(item);
});

// הוספת פריט חדש
app.MapPost("/items", async (ToDoDbContext db, [FromBody] Item item) =>
{
    await db.Items.AddAsync(item);
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// מחיקת פריט לפי ID
app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, [FromBody] Item updateItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    item.Name = updateItem.Name;
    item.IsComplete = updateItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});
app.Run();
