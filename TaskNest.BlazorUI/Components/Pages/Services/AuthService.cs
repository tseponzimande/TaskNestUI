namespace TaskNest.BlazorUI.Components.Pages.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _authStateProvider = authStateProvider;
            _httpClient.BaseAddress = new Uri("https://localhost:7179/api/"); // Adjust to your API base URL
        }

        public async Task<bool> LoginAsync(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Account/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (!string.IsNullOrEmpty(result?.Token))
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", result.Token);
                    await ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Account/register", registerDto);
            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await ((CustomAuthenticationStateProvider)_authStateProvider).NotifyUserLogout();
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
    }
}

