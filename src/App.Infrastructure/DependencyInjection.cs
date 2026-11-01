using App.Domain.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Amazon.Extensions.NETCore.Setup;
using App.Infrastructure.Data;
using App.Infrastructure.Security;
using App.Infrastructure.Storage;
using App.Infrastructure.Auditing;
using App.Infrastructure.Notifications;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace App.Infrastructure;
public static class DependencyInjection
{
 public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
  services.Configure<DatabaseOptions>(config.GetSection("Database"));services.AddSingleton<DbConnectionFactory>();services.AddScoped<IDbManager,DapperDbManager>();
  services.AddDataProtection();services.AddSingleton<ISecretProtector,DataProtectionSecretProtector>();services.AddSingleton<IPasswordHasher,PasswordHasher>();
  services.AddDefaultAWSOptions(config.GetAWSOptions());services.AddAWSService<Amazon.S3.IAmazonS3>();services.Configure<S3Options>(config.GetSection("Storage:S3"));services.AddScoped<IObjectStorage,S3ObjectStorage>();services.AddScoped<IDocumentUploadService,DocumentUploadService>();services.AddScoped<IFileScanner,FailClosedFileScanner>();
  services.AddScoped<IAuditStore,DatabaseAuditStore>();services.AddScoped<IUploadRepository,UploadRepository>();
  var firebaseProjectId=config["Firebase:ProjectId"];if(!string.IsNullOrWhiteSpace(firebaseProjectId)){services.AddSingleton(_=>FirebaseApp.Create(new AppOptions{Credential=GoogleCredential.GetApplicationDefault(),ProjectId=firebaseProjectId},"AppNotifications"));services.AddSingleton(sp=>FirebaseMessaging.GetMessaging(sp.GetRequiredService<FirebaseApp>()));services.AddScoped<INotificationSender,FcmNotificationSender>();}
  return services;
 }
}




