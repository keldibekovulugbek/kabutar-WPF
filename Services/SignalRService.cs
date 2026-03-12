using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kabutar_WPF.Services
{
    public class IncomingMessage
    {
        public long SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class SignalRService
    {
        private HubConnection? _connection;
        private const string HubUrl = "http://localhost:5237/hubs/chat";

        public event Action<long>? UserConnected;
        public event Action<long>? UserDisconnected;
        public event Action<IncomingMessage>? MessageReceived;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public async Task ConnectAsync(string token)
        {
            if (_connection != null)
                await DisconnectAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl(HubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<long>("UserConnected", userId =>
            {
                UserConnected?.Invoke(userId);
            });

            _connection.On<long>("UserDisconnected", userId =>
            {
                UserDisconnected?.Invoke(userId);
            });

            _connection.On<IncomingMessage>("ReceiveMessage", msg =>
            {
                MessageReceived?.Invoke(msg);
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SignalR connection failed: " + ex.Message);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                }
                catch { }
                finally
                {
                    _connection = null;
                }
            }
        }
    }
}
