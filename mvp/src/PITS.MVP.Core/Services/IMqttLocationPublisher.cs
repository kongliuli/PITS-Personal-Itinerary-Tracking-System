namespace PITS.MVP.Core.Services;

public interface IMqttLocationPublisher
{
    /// <summary>
    /// 连接到 MQTT 服务器
    /// </summary>
    Task ConnectAsync(string host, int port, string username, string password);

    /// <summary>
    /// 发布位置更新（OwnTracks 格式）
    /// </summary>
    Task PublishLocationAsync(double latitude, double longitude, double accuracy = 0, double speed = 0, double battery = 0);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }
}
