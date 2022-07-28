using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Heartbeat;
using Hangfire.HttpJob;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region ��̨���� hangfire

// ���ʹ��iis��Ҫ���������첽io
builder.Services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
// ���httpjob������
builder.Services.AddHttpJob();
//builder.Host.UseSerilog((context, logger) =>
//{
//    logger.ReadFrom.Configuration(context.Configuration);
//    logger.Enrich.FromLogContext();
//});

#region ������־ log4net

builder.Logging.AddLog4Net(Path.Combine(AppContext.BaseDirectory, "log4net.config"));
ILoggerRepository repository = LogManager.CreateRepository("ServerSampleRepository");
XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

#endregion ������־ log4net

builder.Services.AddHangfire(config =>
{
    // �Զ�����ʽ���ű����룬��������ΪǶ��ʽ��Դ
    DashboardRoutes.AddStylesheet(typeof(Program).GetTypeInfo().Assembly, "ServerSample.Content.job.css");
    DashboardRoutes.AddJavaScript(typeof(Program).GetTypeInfo().Assembly, "ServerSample.Content.job.js");

    config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));
    config.UseDarkModeSupportForDashboard();
    var Redis = ConnectionMultiplexer.Connect(builder.Configuration["RedisServer"].ToString());
    config.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions()
    {
        Db = 10,
        FetchTimeout = TimeSpan.FromMinutes(5),
        Prefix = "{IMFHangfire}:",
        //���������ʱʱ��
        InvisibilityTimeout = TimeSpan.FromHours(1),
        //������ڼ��Ƶ��
        ExpiryCheckInterval = TimeSpan.FromHours(1),
        DeletedListSize = 10000,
        SucceededListSize = 10000
    })
    .UseHangfireHttpJob(new HangfireHttpJobOptions()
    {
        UseEmail = true,// ʹ������
        AutomaticDelete = 2,// ������ҵִ�к��ù��ڣ���λ�죬 Ĭ��3��
        // ��������
        AttemptsCountArray = new List<int>() { 5, 10, 20 },//����ʱ���������鳤�������Դ���
        AddHttpJobButtonName = "��Ӽƻ�����",
        AddRecurringJobHttpJobButtonName = "��Ӷ�ʱ����",
        EditRecurringJobButtonName = "�༭��ʱ����",
        PauseJobButtonName = "��ͣ��ʼ",
        //DashboardTitle = "��̨����",
        DashboardName = "��̨�������",
        DashboardFooter = "��̨�������",
    })
    .UseConsole(new ConsoleOptions()
    {
        BackgroundColor = "#000000"
    })
    .UseDashboardMetrics(new DashboardMetric[] { DashboardMetrics.AwaitingCount, DashboardMetrics.ProcessingCount, DashboardMetrics.RecurringJobCount, DashboardMetrics.RetriesCount, DashboardMetrics.FailedCount, DashboardMetrics.SucceededCount })
                            ;
});

var listqueue = new[] { "default", "apis", "localjobs" };// ���У��������Ĭ��default
builder.Services.AddHangfireServer(op =>
{
    op.ServerTimeout = TimeSpan.FromMinutes(4);
    op.SchedulePollingInterval = TimeSpan.FromSeconds(1);// �뼶������Ҫ���ö̵㣬һ�������������Ĭ��ʱ�䣬Ĭ��15��
    op.ShutdownTimeout = TimeSpan.FromMinutes(30);// ��ʱʱ��
    op.Queues = listqueue.ToArray();// ����
    op.WorkerCount = Math.Max(Environment.ProcessorCount, 40);// �����߳�������ǰ���������̣߳�Ĭ��20
    op.StopTimeout = TimeSpan.FromSeconds(20);
});

#endregion ��̨���� hangfire

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
var supportedCultures = new[]
           {
                new CultureInfo("zh-CN")
            };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("zh-CN"),
    // Formatting numbers, dates, etc.
    SupportedCultures = supportedCultures,
    // UI strings that we have localized.
    SupportedUICultures = supportedCultures
});
// ��¼�������
app.UseHangfireDashboard("/job", new Hangfire.DashboardOptions
{
    AppPath = "#",// ����ʱ��ת�ĵ�ַ
    DisplayStorageConnectionString = false,// �Ƿ���ʾ���ݿ�������Ϣ
    IsReadOnlyFunc = Context =>
    {
        var isreadonly = false;
        return isreadonly;
    },
    //AsyncAuthorization=
    Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,// �Ƿ�����ssl��֤����https
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login ="admin",// ��¼�˺�
                            PasswordClear = "admin"// ��¼����
                        }
                    }
                })
    }
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();