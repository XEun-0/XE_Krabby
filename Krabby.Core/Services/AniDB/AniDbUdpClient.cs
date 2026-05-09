//using System;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//public class AniDbUdpClient : IDisposable
//{
//    private readonly UdpClient _udp;
//    private IPEndPoint _endpoint;

//    public AniDbUdpClient()
//    {
//        // 🔥 FIX: bind to stable port
//        _udp = new UdpClient(9001);

//        // optional but helpful
//        _udp.Client.ReceiveTimeout = 10000;
//        _udp.Client.SendTimeout = 10000;
//    }

//    public async Task InitializeAsync()
//    {
//        Console.WriteLine("Resolving api.anidb.net...");

//        var addresses = await Dns.GetHostAddressesAsync("api.anidb.net");

//        foreach (var addr in addresses)
//        {
//            Console.WriteLine("Resolved IP: " + addr);
//        }

//        var ip = addresses.First(a => a.AddressFamily == AddressFamily.InterNetwork);

//        _endpoint = new IPEndPoint(ip, 9000);

//        Console.WriteLine("Using _endpoint: " + _endpoint);
//    }

//    public async Task<string> SendAsync(string message)
//    {
//        if (_endpoint == null)
//            throw new InvalidOperationException("Client not initialized");

//        Console.WriteLine("---- UDP SEND START ----");
//        Console.WriteLine("Message: " + message);
//        Console.WriteLine("Local Port: 9001");
//        Console.WriteLine("Remote: " + _endpoint);

//        var bytes = Encoding.UTF8.GetBytes(message);

//        await _udp.SendAsync(bytes, bytes.Length, _endpoint);

//        Console.WriteLine("Packet sent, waiting for response...");

//        var receiveTask = _udp.ReceiveAsync();
//        var timeoutTask = Task.Delay(10000);

//        var completed = await Task.WhenAny(receiveTask, timeoutTask);

//        if (completed == timeoutTask)
//        {
//            Console.WriteLine("❌ TIMEOUT waiting for UDP response");
//            throw new TimeoutException("AniDB UDP timeout");
//        }

//        var response = receiveTask.Result;

//        var text = Encoding.UTF8.GetString(response.Buffer);

//        Console.WriteLine("Response received:");
//        Console.WriteLine(text);
//        Console.WriteLine("---- UDP SEND END ----");

//        return text;
//    }

//    public void Dispose()
//    {
//        _udp.Dispose();
//    }
//}