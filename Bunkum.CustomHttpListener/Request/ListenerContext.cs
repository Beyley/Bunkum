using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Bunkum.CustomHttpListener.Parsing;

namespace Bunkum.CustomHttpListener.Request;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ListenerContext
{
    private readonly Socket _socket;

    public Method Method { get; internal set; }
    public Uri Uri { get; internal set; } = null!;

    public readonly Dictionary<string, string> RequestHeaders = new();
    public readonly Dictionary<string, string> ResponseHeaders = new();

    public ListenerContext(Socket socket)
    {
        this._socket = socket;
    }

    internal async Task SendResponse(HttpStatusCode code)
    {
        List<string> response = new() { $"HTTP/1.1 {(int)code} {code.ToString()}" };
        foreach ((string? key, string? value) in this.ResponseHeaders)
        {
            Debug.Assert(key != null);
            Debug.Assert(value != null);
            
            response.Add($"{key}: {value}");
        }
        response.Add("\r\n");

        await this.SendBufferSafe(string.Join("\r\n", response));
        this.CloseSocket();
    }

    internal void CloseSocket()
    {
        this._socket.Shutdown(SocketShutdown.Both);
        this._socket.Disconnect(false);
        this._socket.Close();
        this._socket.Dispose();
    }

    private Task SendBufferSafe(string str) => this.SendBufferSafe(Encoding.UTF8.GetBytes(str));
    private async Task SendBufferSafe(byte[] buffer)
    {
        try
        {
            await this._socket.SendAsync(buffer);
        }
        catch
        {
            // ignored, log warning in the future?
        }
    }
}