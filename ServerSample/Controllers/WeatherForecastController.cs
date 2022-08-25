using Hangfire;
using Hangfire.Console;
using Hangfire.HttpJob.InterFace;
using Hangfire.HttpJob.Server;
using Hangfire.Server;
using Microsoft.AspNetCore.Mvc;

namespace ServerSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IHttpJobService _httpJobService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpJobService httpJobService)
        {
            _logger = logger;
            _httpJobService = httpJobService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task Get([FromQuery] string name)
        {
            //_httpJobService.EnqueueBackGroundHttpJob(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "http://www.baidu.com",
            //    LockTimeOut = 60,
            //    Method = "get",
            //    Data = "",
            //    QueueName = "apis"
            //}
            //).GetAwaiter().GetResult();

            //// ����������
            //var jobs = new List<HttpJobItem>();
            //jobs.Add(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "http://www.baidu.com",
            //    LockTimeOut = 60,
            //    Method = "get",
            //    Data = ""
            //});
            //jobs.Add(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "https://imff.net.cn",
            //    LockTimeOut = 60,
            //    Method = "post",
            //    Data = ""
            //});
            //_httpJobService.AddContinueRecurringJobs(jobs);

            //_httpJobService.AddOrUpdateRecurringJob(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "http://www.baidu.com",
            //    LockTimeOut = 60,
            //    Method = "get",
            //    Data = "",
            //    QueueName = "apis",
            //    JobName = "����",
            //    Corn = "0/15 * * * *"
            //});

            //_httpJobService.DeleteRecurringJob("����");

            // �ӳ�����
            //await _httpJobService.AddScheduleJob(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "http://www.baidu.com",
            //    LockTimeOut = 60,
            //    Method = "get",
            //    Data = "",
            //    QueueName = "apis"
            //}, timeSpan: TimeSpan.FromMinutes(30));

            //await _httpJobService.AddScheduleJob(new HttpJobItem()
            //{
            //    IsRetry = false,
            //    Url = "http://www.baidu.com",
            //    LockTimeOut = 60,
            //    Method = "get",
            //    Data = "",
            //    QueueName = "apis"
            //}, dateTimeOffset: DateTime.Parse($"2022-07-29 13:59:50"));

            // �Լ��ķ�httpjob����
            throw new Exception();
            //BackgroundJob.Enqueue<WeatherForecastController>(a => a.MyTestMethodAsync(default));
        }

        [NonAction]
        public async Task MyTestMethodAsync(PerformContext context = null)
        {
            // ui����̨��ӡҪ��ʾ����־
            context.SetTextColor(ConsoleTextColor.Red);
            context.WriteLine($"��ִ����!!!!!!!!!!!!!!!!!!!");
            await Task.CompletedTask;
        }
    }
}