using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using BlazoreTestPortal.Client;
using BlazoreTestPortal.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── Azure Active Directory SSO (MSAL) ─────────────────────────────────────────
// Replace YOUR-TENANT-ID and YOUR-CLIENT-ID in appsettings.json before running.
//
// Azure Portal setup:
//   1. App registrations → New registration (single-tenant)
//   2. Redirect URI → Single-page application:
//      https://localhost:<port>/authentication/login-callback
//   3. Expose an API on your backend app registration → add a scope
//      e.g. api://YOUR-API-CLIENT-ID/access_as_user
//   4. Add that scope to DefaultAccessTokenScopes below (and in appsettings.json)
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.LoginMode = "redirect";

    // ── Uncomment to call a protected API ─────────────────────────────────────
    // options.ProviderOptions.DefaultAccessTokenScopes
    //     .Add("api://YOUR-API-CLIENT-ID/access_as_user");
    // options.ProviderOptions.AdditionalScopesToConsent
    //     .Add("api://YOUR-API-CLIENT-ID/access_as_user");
});

// ── Auth service (claims + token helpers) ─────────────────────────────────────
builder.Services.AddScoped<AuthService>();

// ── Default HttpClient (unauthenticated — for public endpoints) ───────────────
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ── Authenticated HttpClient for your backend API ─────────────────────────────
// Uses AuthorizationMessageHandler to auto-attach the Bearer token.
//
// Usage in any component or service:
//   @inject AuthService AuthSvc
//   var token  = await AuthSvc.GetAccessTokenAsync();
//   var client = new HttpClient();
//   client.DefaultRequestHeaders.Authorization =
//       new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
//   var result = await client.GetFromJsonAsync<MyDto>("https://your-api/endpoint");
//
// OR inject the pre-configured client registered below:
//   @inject ApiClient Api
var apiBaseUrl = builder.Configuration["ApiSettings:BlazoreApiBaseUrl"]
                 ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(
            authorizedUrls: [apiBaseUrl],
            scopes: ["api://YOUR-API-CLIENT-ID/access_as_user"]);

    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});

await builder.Build().RunAsync();
