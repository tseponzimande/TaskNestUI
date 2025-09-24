namespace TaskNest.BlazorUI.Components.Pages.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly NotificationService _notificationService;

        public ApiService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, NotificationService notificationService)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _notificationService = notificationService;
            _httpClient.BaseAddress = new Uri("https://localhost:7179/api/");
        }

        private async Task AddAuthorizationHeader()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var token = user.FindFirst("Token")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                await AddAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                await HandleErrorResponse(response);
                throw new Exception($"API request failed with status code {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "API Error", "Failed to retrieve data");
                throw;
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await AddAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                await HandleErrorResponse(response);
                throw new Exception($"API request failed with status code {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "API Error", "Failed to submit data");
                throw;
            }
        }

        public async Task PutAsync<T>(string endpoint, T data)
        {
            try
            {
                await AddAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response);
                    throw new Exception($"API request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "API Error", "Failed to update data");
                throw;
            }
        }

        public async Task DeleteAsync(string endpoint)
        {
            try
            {
                await AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleErrorResponse(response);
                    throw new Exception($"API request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _notificationService.Notify(NotificationSeverity.Error, "API Error", "Failed to delete data");
                throw;
            }
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            var errorContent = await response.Content.ReadAsStringAsync();

            try
            {
                var errorObj = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                if (!string.IsNullOrEmpty(errorObj?.Message))
                {
                    _notificationService.Notify(NotificationSeverity.Error, "API Error", errorObj.Message);
                    return;
                }
            }
            catch { }

            // If we can't parse the error or there's no message, show a generic error
            _notificationService.Notify(NotificationSeverity.Error, "API Error",
                $"Request failed with status code {(int)response.StatusCode}");
        }

        private class ErrorResponse
        {
            public string Message { get; set; }
            public string Detail { get; set; }
        }
    }
}