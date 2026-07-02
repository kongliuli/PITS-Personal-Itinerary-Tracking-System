using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class MqttLocationPublisher : IMqttLocationPublisher, IAsyncDisposable
{
    private IMqttClient? _client;
    private string _topic = "owntracks/pits/device";
    private string _deviceId = "pits-mvp";

    public bool IsConnected => _client?.IsConnected ?? false;

    public async Task ConnectAsync(string host, int port, string username, string password)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithCredentials(username, password)
            .WithClientId($"pits-mvp-{Environment.MachineName}")
            .WithCleanSession()
            .Build();

        await _client.ConnectAsync(options);
    }

    public async Task PublishLocationAsync(double latitude, double longitude, double accuracy = 0, double speed = 0, double battery = 0)
    {
        if (_client == null || !_client.IsConnected) return;

        // OwnTracks JSON 格式
        var payload = new
        {
            _type = "location",
            lat = latitude,
            lon = longitude,
            acc = accuracy,
            vel = speed,
            batt = battery,
            tid = _deviceId,
            tst = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        var json = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(_topic)
            .WithPayload(json)
            .Build();

        await _client.PublishAsync(message);
    }

    public async Task DisconnectAsync()
    {
        if (_client?.IsConnected == true)
        {
            await _client.DisconnectAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _client?.Dispose();
    }
}
