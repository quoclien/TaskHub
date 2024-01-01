using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase("TaskHub");
builder.Services.AddSingleton(mongoDatabase);
builder.Services.AddDbContext<TaskDbContext>(options => options.UseMongoDB(mongoClient, "TaskHub")); // Add the TaskDbContext as a service
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Register your controllers

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // Map the API controllers
app.MapRazorPages();

app.Run();