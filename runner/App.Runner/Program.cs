using Serilog;
using App.Runner;
Directory.CreateDirectory("logs");
var builder=Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog((services,lc)=>lc.WriteTo.Console().WriteTo.File("logs/runner-.log",rollingInterval:RollingInterval.Day,retainedFileCountLimit:14,shared:true));
builder.Services.AddHostedService<Worker>();
await builder.Build().RunAsync();
