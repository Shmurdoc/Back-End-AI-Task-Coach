using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.DTOs.TaskDtos;
using Application.DTOs.GoalDtos;
using Microsoft.Extensions.Options;

namespace Application.AI
{
    public class OpenAIPredictionService : IAIPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAIOptions _options;

        public OpenAIPredictionService(HttpClient httpClient, IOptions<OpenAIOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<TaskPredictionResult> PredictTaskAsync(TaskDto task, Guid userId)
        {
            var prompt = $"Analyze this task for risk and suggest a due date and nudge: {JsonSerializer.Serialize(task)}";
            var response = await CallOpenAIAsync(prompt);
            // TODO: Parse response for real values
            return new TaskPredictionResult(false, DateTime.UtcNow.AddDays(1), response);
        }

        public async Task<IEnumerable<string>> GetSuggestionsAsync(Guid userId)
        {
            var prompt = $"Suggest 3 productivity nudges for user {userId}.";
            var response = await CallOpenAIAsync(prompt);
            // TODO: Parse response for real suggestions
            return new[] { response };
        }

        public async Task<GoalPredictionResult> PredictGoalAsync(GoalDto goal, Guid userId)
        {
            var prompt = $"Analyze this goal for risk and suggest decompositions: {JsonSerializer.Serialize(goal)}";
            var response = await CallOpenAIAsync(prompt);
            // TODO: Parse response for real values
            return new GoalPredictionResult(false, new[] { response });
        }

        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[] { new { role = "user", content = prompt } }
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var response = await _httpClient.PostAsync(_options.Endpoint, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            // Extract the AI's reply (simplified)
            using var doc = JsonDocument.Parse(json);
            var result = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            return result ?? "No response.";
        }
    }
}
