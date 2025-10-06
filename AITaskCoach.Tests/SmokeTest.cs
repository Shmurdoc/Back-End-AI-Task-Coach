using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Application.DTOs.TaskDtos;
using Application.DTOs.AuthDtos;

namespace AITaskCoach.Tests
{
    public class SmokeTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public SmokeTest(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterLoginCreateTaskFlow()
        {
            var email = $"smoketest+{Guid.NewGuid()}@example.com";
            var password = "Sm0keTest!";
            var regBody = new { email, password, name = "Smoke Tester" };
            var regResp = await _client.PostAsync("/api/auth/register", new StringContent(JsonSerializer.Serialize(regBody), Encoding.UTF8, "application/json"));
            regResp.EnsureSuccessStatusCode();
            var regJson = await regResp.Content.ReadAsStringAsync();
            var regDoc = JsonDocument.Parse(regJson);
            var userId = regDoc.RootElement.GetProperty("user").GetProperty("id").GetString();
            var token = regDoc.RootElement.GetProperty("token").GetString();

            var loginBody = new { email, password };
            var loginResp = await _client.PostAsync("/api/auth/login", new StringContent(JsonSerializer.Serialize(loginBody), Encoding.UTF8, "application/json"));
            loginResp.EnsureSuccessStatusCode();
            var loginJson = await loginResp.Content.ReadAsStringAsync();
            var loginDoc = JsonDocument.Parse(loginJson);
            var loginToken = loginDoc.RootElement.GetProperty("token").GetString();
            Assert.False(string.IsNullOrEmpty(loginToken));

            var taskBody = new CreateTaskDto(
                Guid.Parse(userId!),
                "Smoke Test Task",
                "Created by integration test",
                Domain.Enums.TaskItemStatus.Todo,
                Domain.Enums.TaskPriority.Medium,
                1.0,
                null,
                new[] { "smoke" },
                null,
                3,
                30,
                null
            );
            var taskJson = JsonSerializer.Serialize(taskBody);
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/tasks")
            {
                Content = new StringContent(taskJson, Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginToken);
            var createResp = await _client.SendAsync(req);
            Assert.Equal(System.Net.HttpStatusCode.Created, createResp.StatusCode);
            var createJson = await createResp.Content.ReadAsStringAsync();
            Assert.Contains("success", createJson);
        }
    }
}
