
// .NET Feature Flags with Azure: Everything You Need to Know - Mohamad Dbouk
// https://www.youtube.com/watch?v=z6LNrMtJa6Y



// install-package Microsoft.Azure.AppConfiguration.AspNetCore 
// install-package Microsoft.FeatureManagement.AspNetCore


using AppConfigFeature_Filter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.FeatureManagement;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);

//--appConfiguration
var connectionString = builder.Configuration.GetConnectionString("AppConfig");
//builder.Configuration.AddAzureAppConfiguration(connectionString);  //appsettings.json
builder.Configuration.AddAzureAppConfiguration(options =>            //azure appConfiguration
{
    options.Connect(connectionString)
        .Select("Sentinel");            //no label, AzureConn, AzureLabel, Common 
        //https://localhost:7025/Sentinel --> you will see the value

    options.ConfigureRefresh(refresh =>
    {
        //if 'Sentinel' is changed then all cache will be updated regarless label
        refresh.Register("Sentinel", refreshAll: true);             //case sensitive      

        refresh.SetCacheExpiration(TimeSpan.FromSeconds(10));     //default: 30 seconds
        //info: Microsoft.Extensions.Configuration.AzureAppConfiguration.Refresh[0]
        //Setting updated. Key: 'Sentinel'
        //Configuration reloaded.
        //--> The "Sentinel" change is not automatically detected but reloaded when it is referenced
        //--> and 2nd time referenced, it gived the updated value
    });

    options.UseFeatureFlags(refresh =>
    {
        refresh.CacheExpirationInterval = TimeSpan.FromSeconds(5);  //default: 30 seconds
    });

});
builder.Services.AddAzureAppConfiguration();    //--appConfiguration
builder.Services.AddFeatureManagement();        //--FeatureManagement

var app = builder.Build();

app.UseAzureAppConfiguration();                 //--appConfiguration






//--display to the web browswer
app.MapGet("/", () => "Hello World!");  

//--appConfiguration
app.MapGet("/Sentinel", (IConfiguration configuration) =>
    new { Sentinel = configuration["Sentinel"] });
//https://localhost:7025/Sentinel --> you will see the value

//--FeatureManagement
app.MapGet("/Dev", async (IFeatureManager featureManager) =>
{
    var enabled = await featureManager.IsEnabledAsync(FeatureFlags.Dev);
    if (enabled)
    {
        return Results.Ok(new { Dev = "Enabled" });
    }
    return Results.NotFound(new { Dev = "Disabled" });
})
.WithName("Dev");
//https://localhost:7025/Dev --> you will see the value

app.MapGet("/Uat", async (IFeatureManager featureManager) =>
{
    var enabled = await featureManager.IsEnabledAsync(FeatureFlags.Uat);
    if (enabled)
    {
        return Results.Ok(new { Uat = "Enabled" });
    }
    return Results.NotFound(new { Uat = "Disabled" });
})
.WithName("Uat");
//https://localhost:7025/Uat --> you will see the value

//--FeatureManagement:Endpoint
//https://localhost:7025/debug --> "text": "Hello from DEBUG"
app.MapGet("/DEBUG", () => new { Text = "Hello from DEBUG" })
    .AddEndpointFilter<FeatureFlagEndoint>()
    .WithName("DEBUG");
//https://localhost:7025/Debug --> you will see the value if the key is enabled, if disabled then you will get 404 error



app.Run();
