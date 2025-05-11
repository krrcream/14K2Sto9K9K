using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using _14k2sTO9K9K.Services;

namespace _14k2sTO9K9K
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 初始化依赖注入容器
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // 创建主窗口并注入依赖
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 注册服务
            services.AddSingleton<IOsuFileReader, OsuFileReader>();
            services.AddSingleton<IOsuFileProcessor, OsuFileGenerator>();
            services.AddSingleton<OsuProcess>();
            services.AddSingleton<Services.Services>(); // Services 类需要 OsuProcess 的依赖

            // 注册 MainWindow
            services.AddTransient<MainWindow>();
        }
    }
}