using System.Text.Json;
using System.Text.Json.Serialization;
using EventSourcingTests.Events;

namespace Core.Services;

public class EventStoreProjectionService
{
    private readonly HttpClient _httpClient;

    public EventStoreProjectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProjectionState?> GetProjectionStateAsync(string projectionName)
    {
        var url = $"http://localhost:2113/projection/{projectionName}/state";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get projection state: {response.ReasonPhrase}");
        }
        var jsonResponse = await response.Content.ReadAsStringAsync();
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var projectionState = JsonSerializer.Deserialize<ProjectionState>(jsonResponse, options);
            
            return projectionState;
        }
        catch (JsonException jsonEx)
        {
            Console.WriteLine("JSON Deserialization Error: " + jsonEx.Message);
            throw;
        }
    }
}