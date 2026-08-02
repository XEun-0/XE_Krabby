using System.Net;
using System.Net.Sockets;
using System.Text;
using Krabby.Core.Common;

namespace Krabby.Core.Services.AniDB;

/// <summary>
/// 
/// </summary>
public class AniDbTransport
{
    /// <summary>
    /// 
    /// </summary>
    private readonly UdpClient _udp;

    /// <summary>
    /// 
    /// </summary>
    private readonly AniDbRateLimiter _rateLimiter;

    /// <summary>
    /// Constructor for AniDbTransport class
    /// </summary>
    /// <param name="rateLimiter"></param>
    public AniDbTransport(AniDbRateLimiter rateLimiter)
    {
        _udp = new UdpClient(9001);

        _rateLimiter = rateLimiter;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IPEndPoint> ResolveEndpoint()
    {
        var addresses = await Dns.GetHostAddressesAsync("api.anidb.net");
        return new IPEndPoint(addresses[0], 9000);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<string> SendAsync(IPEndPoint endpoint, string message)
    {
        Console.WriteLine("\n---- SEND ----");
        Console.WriteLine(message);
        
        var bytes = Encoding.UTF8.GetBytes(message);

        await _rateLimiter.WaitAsync();

        await _udp.SendAsync(bytes, bytes.Length, endpoint);

        var receiveTask = _udp.ReceiveAsync();
        var timeoutTask = Task.Delay(10000);

        var completed = await Task.WhenAny(receiveTask, timeoutTask);

        if (completed == timeoutTask) 
        {
            return AppCommon.TIMEOUT_ERROR;
        }
        
        var response = receiveTask.Result;

        var text = Encoding.UTF8.GetString(response.Buffer);

        Console.WriteLine("---- RESPONSE ----");
        Console.WriteLine(text);

        return text;
    }
}