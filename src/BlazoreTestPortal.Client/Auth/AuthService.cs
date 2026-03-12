// Auth/AuthService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazoreTestPortal.Client.Auth;

/// <summary>
/// Centralises Azure AD login, logout, user claims, and access token retrieval.
///
/// INJECTION:
///   @inject AuthService AuthSvc
///   @inject IAccessTokenProvider TokenProvider   ← for raw token access
///
/// CLAIMS available after AAD login (standard AAD v2.0 token claims):
///   "name"                 → Display name         e.g. "Raja Thangarasu"
///   "preferred_username"   → UPN / email          e.g. "raja@corp.com"
///   "oid"                  → Azure AD Object ID   (immutable user identifier)
///   "sub"                  → Subject identifier
///   "tid"                  → Tenant ID
///   "roles"                → App role assignments (if configured in Azure AD)
///   "groups"               → Group memberships    (if group claims enabled)
///   "email"                → Email (if profile scope granted)
///   "given_name"           → First name
///   "family_name"          → Last name
///   "jobTitle" / "department" → If optional claims configured in Azure AD manifest
/// </summary>
public sealed class AuthService
{
    private readonly NavigationManager      _nav;
    private readonly IAccessTokenProvider   _tokenProvider;

    public AuthService(NavigationManager nav, IAccessTokenProvider tokenProvider)
    {
        _nav           = nav;
        _tokenProvider = tokenProvider;
    }

    // ── Login / Logout ────────────────────────────────────────────────────────

    public void Login(string? returnUrl = null) =>
        _nav.NavigateToLogin("authentication/login",
            new InteractiveRequestOptions
            {
                Interaction = InteractionType.SignIn,
                ReturnUrl   = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl
            });

    public Task LogoutAsync()
    {
        _nav.NavigateToLogout("authentication/logout");
        return Task.CompletedTask;
    }

    // ── Access Token ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current access token string, or null if unavailable.
    /// Use this to manually attach a Bearer token to outgoing HTTP requests.
    ///
    /// EXAMPLE:
    ///   var token = await AuthSvc.GetAccessTokenAsync();
    ///   request.Headers.Authorization = new("Bearer", token);
    ///
    /// NOTE: For API calls, prefer ApiHttpClientFactory (Program.cs) which
    /// attaches the token automatically via AddHttpMessageHandler.
    /// </summary>
    public async Task<string?> GetAccessTokenAsync(string[]? scopes = null)
    {
        var options = scopes is { Length: > 0 }
            ? new AccessTokenRequestOptions { Scopes = scopes }
            : null;

        var result = options is not null
            ? await _tokenProvider.RequestAccessToken(options)
            : await _tokenProvider.RequestAccessToken();

        return result.TryGetToken(out var token) ? token.Value : null;
    }

    /// <summary>
    /// Returns the full AccessToken object (includes Value, Expires, GrantedScopes).
    /// Returns null if the token could not be obtained.
    /// </summary>
    public async Task<AccessToken?> GetAccessTokenObjectAsync(string[]? scopes = null)
    {
        var options = scopes is { Length: > 0 }
            ? new AccessTokenRequestOptions { Scopes = scopes }
            : null;

        var result = options is not null
            ? await _tokenProvider.RequestAccessToken(options)
            : await _tokenProvider.RequestAccessToken();

        return result.TryGetToken(out var token) ? token : null;
    }

    // ── Claim helpers (all static — usable without injection) ─────────────────

    /// <summary>Returns the value of a named claim, or "" if absent.</summary>
    public static string GetClaim(ClaimsPrincipal? user, string type)
        => user?.FindFirst(type)?.Value ?? string.Empty;

    /// <summary>All values for a claim that may appear multiple times (e.g. "roles", "groups").</summary>
    public static IEnumerable<string> GetClaims(ClaimsPrincipal? user, string type)
        => user?.FindAll(type).Select(c => c.Value) ?? [];

    /// <summary>Display name → "name" claim, falls back to UPN.</summary>
    public static string GetDisplayName(ClaimsPrincipal? user)
    {
        var name = GetClaim(user, "name");
        return string.IsNullOrWhiteSpace(name) ? GetClaim(user, "preferred_username") : name;
    }

    /// <summary>Email → "email" claim, falls back to UPN.</summary>
    public static string GetEmail(ClaimsPrincipal? user)
    {
        var email = GetClaim(user, "email");
        return string.IsNullOrWhiteSpace(email) ? GetClaim(user, "preferred_username") : email;
    }

    /// <summary>Azure AD immutable Object ID — best unique user identifier for DB keys.</summary>
    public static string GetObjectId(ClaimsPrincipal? user)
        => GetClaim(user, "oid").IfEmpty(GetClaim(user, "http://schemas.microsoft.com/identity/claims/objectidentifier"));

    /// <summary>Tenant ID the user authenticated against.</summary>
    public static string GetTenantId(ClaimsPrincipal? user)
        => GetClaim(user, "tid").IfEmpty(GetClaim(user, "http://schemas.microsoft.com/identity/claims/tenantid"));

    /// <summary>App roles assigned to the user in Azure AD (requires role claims configured).</summary>
    public static IEnumerable<string> GetRoles(ClaimsPrincipal? user)
        => GetClaims(user, "roles").Concat(GetClaims(user, "role"));

    /// <summary>Returns true if the user has the given app role.</summary>
    public static bool HasRole(ClaimsPrincipal? user, string role)
        => GetRoles(user).Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>2-char initials from the display name.</summary>
    public static string GetInitials(ClaimsPrincipal? user)
    {
        var name = GetDisplayName(user);
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var src   = name.Contains('@') ? name.Split('@')[0] : name;
        var parts = src.Split([' ', '.', '_', '-'], StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{char.ToUpper(parts[0][0])}{char.ToUpper(parts[^1][0])}"
            : src[..Math.Min(2, src.Length)].ToUpper();
    }

    /// <summary>All claims as a flat list — useful for debugging.</summary>
    public static List<(string Type, string Value)> GetAllClaims(ClaimsPrincipal? user)
        => user?.Claims.Select(c => (ShortClaimType(c.Type), c.Value)).ToList() ?? [];

    // Strips the long URI prefix from claim type names for readability
    static string ShortClaimType(string type)
    {
        var last = type.LastIndexOfAny(['/', '#']);
        return last >= 0 ? type[(last + 1)..] : type;
    }
}

file static class StringExtensions
{
    public static string IfEmpty(this string s, string fallback)
        => string.IsNullOrWhiteSpace(s) ? fallback : s;
}
