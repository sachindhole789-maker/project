using Amazon.Extensions.NETCore.Setup;
using App.Domain.IInterfaces;
using App.Domain.Manager;
using App.Domain.Service;
using App.Infrastructure.Manager.DbManager;
using App.Infrastructure.Repositories.AuditRepository;
using App.Infrastructure.Repositories.AuthRepository;
using App.Infrastructure.Repositories.StudentRepository;
using App.Infrastructure.Repositories.UploadRepository;
using App.Infrastructure.Service.Notifications;
using App.Infrastructure.Service.OtpService;
using App.Infrastructure.Service.Security;
using App.Infrastructure.Service.Storage;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace App.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DatabaseOptions>(config.GetSection("Database"));
        services.AddSingleton<DbConnectionFactory>();
        services.AddScoped<IDbManager, DapperDbManager>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IAuditStore, AuditRepository>();
        services.AddScoped<IUploadRepository, UploadRepository>();

        services.AddDataProtection();
        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
        services.AddMemoryCache();
        services.AddScoped<IOtpManager, OtpManager>();

        services.AddDefaultAWSOptions(config.GetAWSOptions());
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.Configure<S3Options>(config.GetSection("Storage:S3"));
        services.AddScoped<IObjectStorage, S3ObjectStorage>();
        services.AddScoped<IDocumentUploadService, DocumentUploadService>();
        services.AddScoped<IFileScanner, FailClosedFileScanner>();

        var firebaseProjectId = config["Firebase:ProjectId"];
        if (!string.IsNullOrWhiteSpace(firebaseProjectId))
        {
            services.AddSingleton(_ => FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.GetApplicationDefault(), ProjectId = firebaseProjectId }, "AppNotifications"));
            services.AddSingleton(sp => FirebaseMessaging.GetMessaging(sp.GetRequiredService<FirebaseApp>()));
            services.AddScoped<INotificationSender, FcmNotificationSender>();
        }
        return services;
    }
}
