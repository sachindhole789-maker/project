using App.Application;
using App.Application.Messaging;
using App.Domain.Data;
using App.Domain.Results;
using App.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using Rest.API.Hubs;
using Rest.API.Middleware;

var builder=WebApplication.CreateBuilder(args);
Directory.CreateDirectory("logs");
builder.Host.UseSerilog((ctx,lc)=>lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console().WriteTo.File("logs/api-.log",rollingInterval:RollingInterval.Day,retainedFileCountLimit:14,shared:true));
builder.Services.AddControllers();builder.Services.AddOpenApi();builder.Services.AddApplication(typeof(App.Application.DependencyInjection).Assembly);builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<IUserInfoProvider,UserInfoProvider>();
builder.Services.AddSignalR();builder.Services.AddScoped<IRealtimeDashboard,SignalRDashboard>();

builder.Services.AddCors(o=>o.AddPolicy("FlutterDev",p=>p.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()??["http://localhost:3000","http://localhost:5000","http://localhost:8080"]).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
var jwt=builder.Configuration.GetSection("Jwt");var signingKey=jwt["SigningKey"]??throw new InvalidOperationException("Jwt:SigningKey must be set with a 32+ byte secret in a secret store.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>{o.TokenValidationParameters=new(){ValidateIssuer=true,ValidIssuer=jwt["Issuer"],ValidateAudience=true,ValidAudience=jwt["Audience"],ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30)};});builder.Services.AddAuthorization();
// Fixed window is inexpensive; tune PermitLimit/Window for the expected client and gateway limits. Partition by caller identity when authenticated.
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.GlobalLimiter=PartitionedRateLimiter.Create<HttpContext,string>(context=>RateLimitPartition.GetFixedWindowLimiter(context.User.FindFirst("sub")?.Value??context.Connection.RemoteIpAddress?.ToString()??"anonymous",_=>new FixedWindowRateLimiterOptions{PermitLimit=100,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true}));});
var app=builder.Build();if(app.Environment.IsDevelopment()||app.Environment.IsEnvironment("Predevelopment"))app.MapOpenApi();
app.UseHttpsRedirection();app.UseCors("FlutterDev");app.UseAuthentication();app.UseMiddleware<AuditMiddleware>();app.UseMiddleware<RequestExceptionMiddleware>();app.UseRateLimiter();app.UseAuthorization();app.MapControllers();app.MapHub<DashboardHub>("/hubs/dashboard").RequireCors("FlutterDev");app.Run();

public sealed class UserInfoProvider(IHttpContextAccessor accessor):IUserInfoProvider { public string? UserId=>accessor.HttpContext?.User.FindFirst("sub")?.Value??accessor.HttpContext?.User.Identity?.Name; }



