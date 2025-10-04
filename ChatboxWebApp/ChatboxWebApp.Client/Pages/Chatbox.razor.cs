using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatboxWebApp.Client.Pages
{
    public partial class Chatbox : ComponentBase
    {
        //[Inject] protected HttpClient HttpClient { get; set; } = default;
        //[Inject] protected HttpClient Http { get; set; } = default;
        
        protected HttpClient HttpClient { get; set; }
        protected ElementReference messagesContainer;
        protected string conversationId = "";
        protected int questionCount = 0;
        protected int maxQuestions = 10;
        protected string inputMode = "single";
        protected string currentQuestion = "";
        protected List<string> multipleQuestions = new List<string> { "" };
        protected List<ChatMessage> messages = new List<ChatMessage>();
        protected bool isLoading = false;
        protected string errorMessage = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadConversationInfo();
        }

        protected async Task LoadConversationInfo()
        {
            try
            {
                HttpClient = new HttpClient();
                HttpClient.BaseAddress = new Uri("http://localhost:5077/");
                var response = await HttpClient.GetFromJsonAsync<ConversationInfoResponse>("/api/chat/conversation-info");
                if (response != null)
                {
                    conversationId = response.ConversationId ?? "";
                    questionCount = response.QuestionCount;
                    maxQuestions = response.MaxQuestions;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading conversation info: {ex.Message}";
            }
        }

        protected async Task AskSingleQuestion()
        {
            if (string.IsNullOrWhiteSpace(currentQuestion))
                return;

            isLoading = true;
            errorMessage = "";

            try
            {
                var userQuestion = currentQuestion.Trim();
                messages.Add(new ChatMessage { Type = "user", Content = userQuestion, Timestamp = DateTime.Now });
                currentQuestion = "";

                var request = new { Question = userQuestion };
                var response = await HttpClient.PostAsJsonAsync("/api/chat/ask", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatGptResponse>();
                    if (result?.Answers != null && result.Answers.Count > 0)
                    {
                        var answer = result.Answers[0];
                        messages.Add(new ChatMessage
                        {
                            Type = "assistant",
                            Content = answer.Answer ?? "",
                            Citations = answer.Citations,
                            Timestamp = DateTime.Now
                        });
                        await LoadConversationInfo();
                    }
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    errorMessage = error?.Error ?? "An error occurred.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task AskMultipleQuestions()
        {
            var validQuestions = multipleQuestions.Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => q.Trim()).ToList();

            if (validQuestions.Count == 0)
                return;

            isLoading = true;
            errorMessage = "";

            try
            {
                foreach (var question in validQuestions)
                {
                    messages.Add(new ChatMessage { Type = "user", Content = question, Timestamp = DateTime.Now });
                }
                multipleQuestions = new List<string> { "" };

                var request = new { Questions = validQuestions };
                var response = await HttpClient.PostAsJsonAsync("/api/chat/ask-multiple", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatGptResponse>();
                    if (result?.Answers != null)
                    {
                        foreach (var answer in result.Answers)
                        {
                            messages.Add(new ChatMessage
                            {
                                Type = "assistant",
                                Content = answer.Answer ?? "",
                                Citations = answer.Citations,
                                Timestamp = DateTime.Now
                            });
                        }
                        await LoadConversationInfo();
                    }
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    errorMessage = error?.Error ?? "An error occurred.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                isLoading = false;
            }
        }

        protected async Task NewConversation()
        {
            try
            {
                var response = await HttpClient.PostAsync("/api/chat/new-conversation", null);
                if (response.IsSuccessStatusCode)
                {
                    messages.Clear();
                    await LoadConversationInfo();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
            }
        }

        protected async Task ResetConversation()
        {
            try
            {
                var response = await HttpClient.PostAsync("/api/chat/reset", null);
                if (response.IsSuccessStatusCode)
                {
                    messages.Clear();
                    await LoadConversationInfo();
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error: {ex.Message}";
            }
        }

        protected void AddQuestion()
        {
            multipleQuestions.Add("");
        }

        protected void RemoveQuestion(int index)
        {
            if (multipleQuestions.Count > 1)
            {
                multipleQuestions.RemoveAt(index);
            }
        }

        public class ChatMessage
        {
            public string Type { get; set; } = "";
            public string Content { get; set; } = "";
            public List<DocumentCitation>? Citations { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class ChatGptResponse
        {
            public string? ConversationId { get; set; }
            public List<ChatGptAnswer>? Answers { get; set; }
        }

        public class ChatGptAnswer
        {
            public string? Question { get; set; }
            public string? Answer { get; set; }
            public List<DocumentCitation>? Citations { get; set; }
        }

        public class DocumentCitation
        {
            public string? Title { get; set; }
            public string? FilePath { get; set; }
            public float Score { get; set; }
            public string? Content { get; set; }
        }

        public class ConversationInfoResponse
        {
            public string? ConversationId { get; set; }
            public int QuestionCount { get; set; }
            public int MaxQuestions { get; set; }
            public string? Message { get; set; }
        }

        public class ErrorResponse
        {
            public string? Error { get; set; }
        }
    }
}
