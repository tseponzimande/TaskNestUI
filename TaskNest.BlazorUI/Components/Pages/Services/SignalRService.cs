namespace TaskNest.BlazorUI.Components.Pages.Services
{
    public class SignalRService(NavigationManager navigationManager, AuthenticationStateProvider authStateProvider) : IAsyncDisposable
    {
        private readonly NavigationManager _navigationManager = navigationManager;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
        private HubConnection? _hubConnection;

        public event Action<TaskItemDto>? OnTaskCreated;
        public event Action<TaskItemDto>? OnTaskUpdated;
        public event Action<Guid>? OnTaskDeleted;
        public event Action<BoardColumnDto>? OnColumnCreated;
        public event Action<BoardColumnDto>? OnColumnUpdated;
        public event Action<Guid>? OnColumnDeleted;

        public async Task InitializeAsync()
        {
            if (_hubConnection != null)
            {
                return;
            }

            // Get the JWT token
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var token = authState.User.FindFirst("Token")?.Value;

            // Create the connection
            var builder = new HubConnectionBuilder();

            if (!string.IsNullOrEmpty(token))
            {
                _hubConnection = builder
                    .WithUrl(_navigationManager.ToAbsoluteUri("/taskNestHub"), options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    })
                    .WithAutomaticReconnect()
                    .Build();
            }
            else
            {
                _hubConnection = builder
                    .WithUrl(_navigationManager.ToAbsoluteUri("/taskNestHub"))
                    .WithAutomaticReconnect()
                    .Build();
            }

            // Register handlers
            _hubConnection.On<TaskItemDto>("TaskCreated", task =>
            {
                OnTaskCreated?.Invoke(task);
            });

            _hubConnection.On<TaskItemDto>("TaskUpdated", task =>
            {
                OnTaskUpdated?.Invoke(task);
            });

            _hubConnection.On<Guid>("TaskDeleted", taskId =>
            {
                OnTaskDeleted?.Invoke(taskId);
            });

            _hubConnection.On<BoardColumnDto>("ColumnCreated", column =>
            {
                OnColumnCreated?.Invoke(column);
            });

            _hubConnection.On<BoardColumnDto>("ColumnUpdated", column =>
            {
                OnColumnUpdated?.Invoke(column);
            });

            _hubConnection.On<Guid>("ColumnDeleted", columnId =>
            {
                OnColumnDeleted?.Invoke(columnId);
            });

            // Start the connection
            await _hubConnection.StartAsync();
        }

        public async Task JoinBoardGroupAsync(Guid boardId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinBoard", boardId.ToString());
            }
        }

        public async Task LeaveBoardGroupAsync(Guid boardId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveBoard", boardId.ToString());
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
    }
}


//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.SignalR.Client;

//namespace TaskNest.BlazorUI.Components.Pages.Services
//{
//    public class SignalRService : IAsyncDisposable
//    {
//        private readonly NavigationManager _navigationManager;
//        private readonly AuthenticationStateProvider _authStateProvider;
//        private HubConnection? _hubConnection;

//        public event Action<TaskItemDto>? OnTaskCreated;
//        public event Action<TaskItemDto>? OnTaskUpdated;
//        public event Action<Guid>? OnTaskDeleted;
//        public event Action<BoardColumnDto>? OnColumnCreated;
//        public event Action<BoardColumnDto>? OnColumnUpdated;
//        public event Action<Guid>? OnColumnDeleted;

//        public SignalRService(NavigationManager navigationManager, AuthenticationStateProvider authStateProvider)
//        {
//            _navigationManager = navigationManager;
//            _authStateProvider = authStateProvider;
//        }

//        public async Task InitializeAsync()
//        {
//            if (_hubConnection != null)
//            {
//                return;
//            }

//            // Get the JWT token
//            var authState = await _authStateProvider.GetAuthenticationStateAsync();
//            var token = authState.User.FindFirst("Token")?.Value;

//            // Create the connection
//            _hubConnection = new HubConnectionBuilder()
//                .WithUrl(_navigationManager.ToAbsoluteUri("/taskNestHub"), options =>
//                {
//                    if (!string.IsNullOrEmpty(token))
//                    {
//                        options.AccessTokenProvider = () => Task.FromResult(token);
//                    }
//                })
//                .WithAutomaticReconnect()
//                .Build();

//            // Register handlers
//            _hubConnection.On<TaskItemDto>("TaskCreated", task =>
//            {
//                OnTaskCreated?.Invoke(task);
//            });

//            _hubConnection.On<TaskItemDto>("TaskUpdated", task =>
//            {
//                OnTaskUpdated?.Invoke(task);
//            });

//            _hubConnection.On<Guid>("TaskDeleted", taskId =>
//            {
//                OnTaskDeleted?.Invoke(taskId);
//            });

//            _hubConnection.On<BoardColumnDto>("ColumnCreated", column =>
//            {
//                OnColumnCreated?.Invoke(column);
//            });

//            _hubConnection.On<BoardColumnDto>("ColumnUpdated", column =>
//            {
//                OnColumnUpdated?.Invoke(column);
//            });

//            _hubConnection.On<Guid>("ColumnDeleted", columnId =>
//            {
//                OnColumnDeleted?.Invoke(columnId);
//            });

//            // Start the connection
//            await _hubConnection.StartAsync();
//        }

//        public async Task JoinBoardGroupAsync(Guid boardId)
//        {
//            if (_hubConnection?.State == HubConnectionState.Connected)
//            {
//                await _hubConnection.InvokeAsync("JoinBoard", boardId.ToString());
//            }
//        }

//        public async Task LeaveBoardGroupAsync(Guid boardId)
//        {
//            if (_hubConnection?.State == HubConnectionState.Connected)
//            {
//                await _hubConnection.InvokeAsync("LeaveBoard", boardId.ToString());
//            }
//        }

//        public async ValueTask DisposeAsync()
//        {
//            if (_hubConnection != null)
//            {
//                await _hubConnection.DisposeAsync();
//            }
//        }

//        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
//    }
//}

