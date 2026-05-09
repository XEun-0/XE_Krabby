namespace Krabby.Core.Models;

public class SessionCache
{
    public string? SessionKey { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? LastLogout { get; set; }
}
