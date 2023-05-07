using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Bunkum.CustomHttpListener.Extensions;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace Bunkum.CustomHttpListener;

/// <summary>
/// A custom HTTP socket listener used by Bunkum's HTTP server.
/// This probably isn't what you're looking for. 
/// </summary>
public abstract class BunkumHttpListener : IDisposable
{
    protected readonly Uri _listenEndpoint;
    protected readonly LoggerContainer<HttpLogContext> _logger;

    private const int HeaderLineLimit = 1024; // 1KB per header
    private const int RequestLineLimit = 256; // 256 bytes
    
    public BunkumHttpListener(Uri listenEndpoint)
    {
        this._listenEndpoint = listenEndpoint;
        this._logger = new LoggerContainer<HttpLogContext>();
        this._logger.RegisterLogger(new ConsoleLogger());
        
        this._logger.LogInfo(HttpLogContext.Startup, "Internal server is listening at URL " + listenEndpoint);
        this._logger.LogInfo(HttpLogContext.Startup, "The above URL is probably not the URL you should use to patch. " +
                                                     "See https://littlebigrefresh.github.io/Docs/patch-url for more information.");
    }

    public abstract void StartListening();

    public async Task WaitForConnectionAsync(Func<ListenerContext, Task> action)
    {
        while (true)
        {
            ListenerContext? request = null;
            try
            {
                request = await this.WaitForConnectionAsyncInternal();
            }
            catch (Exception e)
            {
                this._logger.LogError(HttpLogContext.Request, "Failed to handle a connection: " + e);
                if (!(request?.SocketClosed).GetValueOrDefault(true)) await request!.SendResponse(HttpStatusCode.BadRequest);
                continue;
            }
            
            if (request == null) continue;
            
            await action.Invoke(request);
            if(!request.SocketClosed) await request.SendResponse(HttpStatusCode.NotFound);
            return;
        }
    }

    protected abstract Task<ListenerContext?> WaitForConnectionAsyncInternal();

    protected static IEnumerable<(string, string)> ReadCookies(string header)
    {
        if (string.IsNullOrEmpty(header)) yield break;

        string[] pairs = header.Split(';');
        foreach (string pair in pairs)
        {
            int index = pair.IndexOf('=');
            if (index < 0) continue; // Pair is split by =, if we cant find it then this is obviously bad data

            string key = pair.Substring(0, index).TrimStart();
            string value = pair.Substring(index + 1).TrimEnd();

            yield return (key, value);
        }
    }

    protected static string[] ReadRequestLine(Stream stream)
    {
        byte[] requestLineBytes = new byte[RequestLineLimit];
        // Probably breaks spec to just look for \n instead of \r\n but who cares
        stream.ReadIntoBufferUntilChar('\n', requestLineBytes);
        
        return Encoding.ASCII.GetString(requestLineBytes).Split(' ');
    }

    protected static IEnumerable<(string, string)> ReadHeaders(Stream stream)
    {
        while (true)
        {
            byte[] headerLineBytes = new byte[HeaderLineLimit];
            int count = stream.ReadIntoBufferUntilChar('\n', headerLineBytes);

            string headerLine = Encoding.UTF8.GetString(headerLineBytes, 0, count);
            int index = headerLine.IndexOf(": ", StringComparison.Ordinal);
            if(index == -1) break; // no more headers

            string key = headerLine.Substring(0, index);
            string value = headerLine.Substring(index + 2).TrimEnd('\r');

            yield return (key, value);
        }
    }

    public virtual void Dispose() {}
}