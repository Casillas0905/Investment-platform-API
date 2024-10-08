﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json.Linq;

namespace HttpRequest;

public class Login
{
    
    private static readonly HttpClient client = new HttpClient();
    
    public async Task<string> GetSessionToken(string username, string password)
    {
        try
        {
            // Authenticate
            var loginData = new { login = username, password, remember_me = true };
            var loginResponse = await client.PostAsJsonAsync("https://api.cert.tastyworks.com/sessions", loginData);
            if (!loginResponse.IsSuccessStatusCode)
            {
                var errorMessage = await loginResponse.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed: {errorMessage}");
            }

            // Extract session token
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            JObject parsedResponse = JObject.Parse(loginContent);

            // Check if the necessary properties exist
            if (parsedResponse["data"]?["session-token"] == null)
            {
                throw new Exception("Session token not found in the response.");
            }

            var sessionToken = parsedResponse["data"]["session-token"].ToString();
            return sessionToken;
        }

        catch (Exception ex)
        {
            // Log or handle the exception as needed
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    public string DestroySession(string sessionToken)
    {
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", sessionToken);
        Console.WriteLine(sessionToken);
        var destroyResponse = client.DeleteAsync("https://api.cert.tastyworks.com/sessions");
        int responseCode = (int)destroyResponse.Result.StatusCode;
        if (responseCode == 204)
        {
            return "Session closed successfully";
        }
        return "Error closing the session";
    }

    public async Task<string> GetDetails(string sessionToken, string accountNumber)
    {
        // Set Authorization header directly with the session token
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", sessionToken);

        // Retrieve account details
        var accountsResponse = await client.GetAsync($"https://api.cert.tastyworks.com/customers/me/accounts/{accountNumber}");

        if (!accountsResponse.IsSuccessStatusCode)
        {
            var errorMessage = await accountsResponse.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve account details: {errorMessage}");
        }

        var accountDetails = await accountsResponse.Content.ReadAsStringAsync();
        return accountDetails;
    }
}