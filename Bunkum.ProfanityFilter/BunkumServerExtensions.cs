using Bunkum.Core;

namespace Bunkum.ProfanityFilter;

/// <summary>
/// Extension class that facilitates injection of this library's services.
/// </summary>
[Obsolete(DeprecationWarning.Reason)]
public static class BunkumServerExtensions
{
    /// <summary>
    /// Adds the profanity service to the server. Can be customized to whitelist/blacklist more words.
    /// </summary>
    /// <param name="server">The server to inject to.</param>
    /// <param name="allowList">A list of words to allow.</param>
    /// <param name="extraDenyList">A list of words to deny.</param>
    [Obsolete(DeprecationWarning.Reason)]
    public static void AddProfanityService(this BunkumServer server, string[]? allowList = null, string[]? extraDenyList = null)
    {
        allowList ??= Array.Empty<string>();
        extraDenyList ??= Array.Empty<string>();
        
        server.AddService<ProfanityService>(allowList, extraDenyList);
    }
}