using AudioScriptExtractor.Pages;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "\\AudioExtractor\\AudioExtractor.log"), rollingInterval: RollingInterval.Infinite)
    //.Filter.ByIncludingOnly(Matching.FromSource("AudioScriptExtractor.Pages"))
    .Filter.ByExcluding(Matching.FromSource("Microsoft"))
    .CreateLogger();    

// Add services to the container.
builder.Services.AddRazorPages();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.Configure<AudioScriptExtractor.Models.AudioExtractorSettings>(builder.Configuration.GetSection("AudioExtractor"));

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

app.MapRazorPages();

app.Run();
