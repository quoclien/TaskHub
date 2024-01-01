using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDBConnection");
var mongoClient = new MongoClient(mongoConnectionString);
var databaseName = builder.Configuration.GetSection("DatabaseName").Value;
if (databaseName != null)
{
    var mongoDatabase = mongoClient.GetDatabase(databaseName);
    builder.Services.AddSingleton(mongoDatabase);
    builder.Services.AddDbContext<TaskDbContext>(options => options.UseMongoDB(mongoClient, databaseName));
}
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers(); // Map the API controllers
app.MapRazorPages();

app.Run();