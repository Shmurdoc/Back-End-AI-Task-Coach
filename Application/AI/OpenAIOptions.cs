namespace Application.AI
{
    public class OpenAIOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string Model { get; set; } = "gpt-3.5-turbo";
    }
}
