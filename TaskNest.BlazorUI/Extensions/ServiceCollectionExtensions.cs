namespace TaskNest.BlazorUI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaskNestServices(this IServiceCollection services)
        {
            // Configure HttpClient for API calls
            services.AddHttpClient("TaskNestApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7179/api/");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
            // Register application services
            services.AddScoped<ApiService>();
            services.AddScoped<AuthService>();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            // Add Radzen services
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();

            // Add SignalR if needed
            services.AddSingleton<IHubConnectionBuilder, HubConnectionBuilder>();

            return services;
        }
    }
}

//namespace TaskNest.BlazorUI.Extensions
//{
//    public static class ServiceCollectionExtensions
//    {
//        public static IServiceCollection AddTaskNestServices(this IServiceCollection services)
//        {
//            services.AddHttpClient();
//            services.AddScoped<ApiService>();
//            services.AddScoped<AuthService>();
//            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

//            return services;
//        }
//    }
//}
