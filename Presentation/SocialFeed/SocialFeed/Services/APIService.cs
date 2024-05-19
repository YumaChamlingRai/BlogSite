using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace Bislerium.Services;

public class APIService(HttpClient httpClient, ILocalStorageService localStorage)
{
    public async Task<bool> IsUserLoggedIn()
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken == null) return false;

        var tokenHandler = new JwtSecurityTokenHandler();
     
        var jwtToken = tokenHandler.ReadJwtToken(accessToken);

        var expiryDateTime = jwtToken.ValidTo;

        return expiryDateTime > DateTime.Now;
    }

    public async Task LogOutUser()
    {
        await localStorage.RemoveItemAsync("access_token");
    }
    
    public async Task<T?> GetAsync<T>(string endpoint, IDictionary<string, string>? parameters = null)
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var fullUrl = $"https://localhost:44340/api/{endpoint}";

        if (parameters is { Count: > 0 })
        {
            var queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            
            fullUrl += "?" + queryString;
        }

        var response = await httpClient.GetAsync(fullUrl);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }

        throw new ApplicationException($"Error: {response.StatusCode}");
    }

    public async Task PostAsync(string endpoint, StringContent stringContent)
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        var fullUrl = $"https://localhost:44340/api/{endpoint}";

        var response = await httpClient.PostAsync(fullUrl, stringContent);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new ApplicationException($"Error: {response.StatusCode}");
    }
    
    public async Task<T> PostAsync<T>(string endpoint, StringContent stringContent)
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        var fullUrl = $"https://localhost:44340/api/{endpoint}";

        var response = await httpClient.PostAsync(fullUrl, stringContent);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>();

            return result!;
        }

        throw new ApplicationException($"Error: {response.StatusCode}");
    }
    
    public async Task UpdateAsync(string endpoint, StringContent stringContent)
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        var fullUrl = $"https://localhost:44340/api/{endpoint}";
        
        var response = await httpClient.PatchAsync(fullUrl, stringContent);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new ApplicationException($"Error: {response.StatusCode}");
    }

    public async Task DeleteAsync(string endpoint)
    {
        var accessToken = await localStorage.GetItemAsync<string>("access_token");

        if (accessToken != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        var fullUrl = $"https://localhost:44340/api/{endpoint}";
        
        var response = await httpClient.DeleteAsync(fullUrl);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new ApplicationException($"Error: {response.StatusCode}");
    }
}