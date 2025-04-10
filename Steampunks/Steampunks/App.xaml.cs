using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Steampunks.DataLink;
using Microsoft.Extensions.DependencyInjection;
using Steampunks.Services;
using Steampunks.ViewModels;
using Steampunks.Views;
using Steampunks.Domain.Entities;
using Steampunks.Services.TradeService;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Steampunks
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? m_window;
        private readonly IServiceCollection services;

        public static User CurrentUser { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            services = new ServiceCollection();
            ConfigureServices();

            // Initialize the database
            try
            {
                var dbConnector = new DatabaseConnector();
                if (!dbConnector.TestConnection())
                {
                    System.Diagnostics.Debug.WriteLine("Database not initialized, initializing now...");
                    DatabaseInitializer.InitializeDatabase();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Database already initialized, skipping initialization.");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                // In a real application, you might want to show this to the user
                // or handle it more gracefully
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private void ConfigureServices()
        {
            // Add navigation services
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPageService, PageService>();

            // Add database services
            services.AddSingleton<DatabaseConnector>();

            // Add application services
            services.AddTransient<UserService>();
            services.AddTransient<GameService>();
            services.AddTransient<ITradeService>();

            // Add view models
            services.AddTransient<ITradeViewModel>();

            // Configure pages
            services.Configure<PageServiceOptions>(options =>
            {
                options.Configure<ITradeViewModel, TradeView>();
            });
        }

        public static T GetService<T>()
            where T : class
        {
            if ((App.Current as App)?.services == null)
            {
                throw new InvalidOperationException("Cannot get service before app initialization");
            }

            var serviceProvider = ((App)Current).services.BuildServiceProvider();
            return serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
        }
    }
}
