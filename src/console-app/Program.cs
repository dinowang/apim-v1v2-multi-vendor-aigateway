using System.ClientModel;
using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ApimAi.ConsoleApp;

public static class Program
{
    private static readonly List<string> PromptHistory = [];
    private static int _historyIndex = -1;

    // Current settings
    private static string[] _endpointKeys = [];
    private static string[] _backendKeys = [];
    private static int[] _csThresholds = [];
    private static int _currentEndpointIndex;
    private static int _currentBackendIndex;
    private static int _currentCsIndex;

    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var endpoints = new Dictionary<string, EndpointConfig>
        {
            ["APIM v1"] = config.GetSection("Endpoints:v1").Get<EndpointConfig>()!,
            ["APIM v2"] = config.GetSection("Endpoints:v2").Get<EndpointConfig>()!,
        };

        var backends = new Dictionary<string, BackendConfig>
        {
            ["OpenAI"] = config.GetSection("Backends:OpenAI").Get<BackendConfig>()!,
            ["Claude"] = config.GetSection("Backends:Claude").Get<BackendConfig>()!,
            ["Gemini"] = config.GetSection("Backends:Gemini").Get<BackendConfig>()!,
        };

        _endpointKeys = ["APIM v1", "APIM v2"];
        _backendKeys = ["OpenAI", "Claude", "Gemini"];
        _csThresholds = [7, 0, 1, 2, 4, 5, 6];
        _currentEndpointIndex = 0;
        _currentBackendIndex = 0;
        _currentCsIndex = 0;

        StatusBar.Init();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            StatusBar.Cleanup();
            Environment.Exit(0);
        };
        DrawStatusBar();

        Console.WriteLine("=== AI App Console ===");
        // Console.WriteLine("[F1] v1 [F2] v2 [F3] Backend [F4] CS [↑↓] History [Ctrl+C] Exit");
        Console.WriteLine();

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful assistant.")
        };

        while (true)
        {
            var ep = endpoints[_endpointKeys[_currentEndpointIndex]];
            var bk = backends[_backendKeys[_currentBackendIndex]];

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You> ");
            Console.ResetColor();

            var prompt = ReadLineWithHistory(out var action);
            if (prompt is null) break;

            switch (action)
            {
                case InputAction.SwitchV1:
                    _currentEndpointIndex = 0;
                    DrawStatusBar();
                    // Erase current "You> " and reposition for next iteration
                    Console.Write("\r\e[K");
                    continue;
                case InputAction.SwitchV2:
                    _currentEndpointIndex = 1;
                    DrawStatusBar();
                    Console.Write("\r\e[K");
                    continue;
                case InputAction.ToggleBackend:
                    _currentBackendIndex = (_currentBackendIndex + 1) % _backendKeys.Length;
                    DrawStatusBar();
                    Console.Write("\r\e[K");
                    continue;
                case InputAction.ToggleCs:
                    _currentCsIndex = (_currentCsIndex + 1) % _csThresholds.Length;
                    DrawStatusBar();
                    Console.Write("\r\e[K");
                    continue;
            }

            if (string.IsNullOrWhiteSpace(prompt)) continue;

            // Re-read current settings (may have changed)
            ep = endpoints[_endpointKeys[_currentEndpointIndex]];
            bk = backends[_backendKeys[_currentBackendIndex]];

            messages.Add(new UserChatMessage(prompt));

            var allHeaders = new Dictionary<string, string>(bk.Headers)
            {
                ["X-CS-Threshold"] = _csThresholds[_currentCsIndex].ToString()
            };
            var policy = new CustomHeadersPolicy(allHeaders, bk.QueryStrings);
            var options = new AzureOpenAIClientOptions();
            options.AddPolicy(policy, PipelinePosition.PerCall);

            var client = new AzureOpenAIClient(
                new Uri(ep.Url),
                new ApiKeyCredential(ep.ApiKey),
                options);
            var chatClient = client.GetChatClient(bk.DeploymentName);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("AI> ");
            Console.ResetColor();

            try
            {
                AsyncCollectionResult<StreamingChatCompletionUpdate> stream =
                    chatClient.CompleteChatStreamingAsync(messages);

                var fullResponse = string.Empty;
                var chunkCount = 0;
                var totalChunkBytes = 0L;
                ChatTokenUsage? tokenUsage = null;

                await foreach (var update in stream)
                {
                    foreach (var part in update.ContentUpdate)
                    {
                        Console.Write(part.Text);
                        fullResponse += part.Text;
                        totalChunkBytes += System.Text.Encoding.UTF8.GetByteCount(part.Text);
                    }
                    chunkCount++;

                    if (update.Usage is not null)
                    {
                        tokenUsage = update.Usage;
                    }
                }

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                var stats = new List<string>();
                if (tokenUsage is not null)
                {
                    stats.Add($"Tokens: {tokenUsage.OutputTokenCount} output / {tokenUsage.TotalTokenCount} total");
                }
                stats.Add($"Chunks: {chunkCount}");
                if (chunkCount > 0)
                {
                    stats.Add($"Avg chunk size: {totalChunkBytes / chunkCount} bytes");
                }
                Console.WriteLine($"  [{string.Join(" | ", stats)}]");
                Console.ResetColor();
                Console.WriteLine();

                messages.Add(new AssistantChatMessage(fullResponse));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine();
                messages.RemoveAt(messages.Count - 1);
            }
        }

        StatusBar.Cleanup();
    }

    private static void DrawStatusBar()
    {
        var epLabel = _endpointKeys[_currentEndpointIndex];
        var bkLabel = _backendKeys[_currentBackendIndex];
        var csLabel = _csThresholds[_currentCsIndex].ToString();

        StatusBar.Draw($" [F1/F2] {epLabel}  │  [F3] Backend: {bkLabel}  │  [F4] Content Safety Level (0-7): {csLabel}  │  [↑↓] Recall History  │  [Ctrl+C] Exit ");
    }

    private static string? ReadLineWithHistory(out InputAction action)
    {
        var buffer = new List<char>();
        var pos = 0;
        _historyIndex = PromptHistory.Count;
        action = InputAction.None;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            // F1 / Ctrl+1 → APIM v1
            if ((key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.D1)
                || key.Key == ConsoleKey.F1)
            {
                action = InputAction.SwitchV1;
                return string.Empty;
            }
            // F2 / Ctrl+2 → APIM v2
            if ((key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.D2)
                || key.Key == ConsoleKey.F2)
            {
                action = InputAction.SwitchV2;
                return string.Empty;
            }
            // F3 → toggle backend
            if (key.Key == ConsoleKey.F3)
            {
                action = InputAction.ToggleBackend;
                return string.Empty;
            }
            // F4 → cycle Content Safety threshold
            if (key.Key == ConsoleKey.F4)
            {
                action = InputAction.ToggleCs;
                return string.Empty;
            }

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    var result = new string(buffer.ToArray());
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        PromptHistory.Add(result);
                    }
                    return result;

                case ConsoleKey.Backspace:
                    if (pos > 0)
                    {
                        buffer.RemoveAt(pos - 1);
                        pos--;
                        RedrawLine(buffer, pos);
                    }
                    break;

                case ConsoleKey.Delete:
                    if (pos < buffer.Count)
                    {
                        buffer.RemoveAt(pos);
                        RedrawLine(buffer, pos);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (pos > 0)
                    {
                        pos--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (pos < buffer.Count)
                    {
                        pos++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        ReplaceBuffer(buffer, PromptHistory[_historyIndex], out pos);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (_historyIndex < PromptHistory.Count - 1)
                    {
                        _historyIndex++;
                        ReplaceBuffer(buffer, PromptHistory[_historyIndex], out pos);
                    }
                    else if (_historyIndex == PromptHistory.Count - 1)
                    {
                        _historyIndex = PromptHistory.Count;
                        ReplaceBuffer(buffer, string.Empty, out pos);
                    }
                    break;

                case ConsoleKey.Home:
                    pos = 0;
                    RedrawLine(buffer, pos);
                    break;

                case ConsoleKey.End:
                    pos = buffer.Count;
                    RedrawLine(buffer, pos);
                    break;

                default:
                    if (key.KeyChar >= 32)
                    {
                        buffer.Insert(pos, key.KeyChar);
                        pos++;
                        RedrawLine(buffer, pos);
                    }
                    break;
            }
        }
    }

    private static void ReplaceBuffer(List<char> buffer, string text, out int pos)
    {
        buffer.Clear();
        buffer.AddRange(text);
        pos = buffer.Count;
        RedrawLine(buffer, pos);
    }

    private static void RedrawLine(List<char> buffer, int pos)
    {
        var promptPrefix = "You> ";
        Console.SetCursorPosition(promptPrefix.Length, Console.CursorTop);
        var text = new string(buffer.ToArray());
        Console.Write(text + " ");
        Console.SetCursorPosition(promptPrefix.Length + pos, Console.CursorTop);
    }
}

public enum InputAction
{
    None,
    SwitchV1,
    SwitchV2,
    ToggleBackend,
    ToggleCs
}

/// <summary>
/// Fixed bottom status bar using ANSI escape sequences.
/// Sets a scroll region for the main content area and reserves the last line.
/// </summary>
public static class StatusBar
{
    private static int _height;

    public static void Init()
    {
        // Switch to alternate screen buffer (preserves original terminal content)
        Console.Write("\e[?1049h");

        _height = Console.WindowHeight;

        // Set scroll region to rows 1..(height-1), reserving last row for status bar
        Console.Write($"\e[1;{_height - 1}r");

        // Move cursor to top-left of scroll region
        Console.SetCursorPosition(0, 0);

        // Draw initial empty status bar
        Draw(string.Empty);
    }

    public static void Draw(string text)
    {
        _height = Console.WindowHeight;
        var width = Console.WindowWidth;

        // Save cursor position
        Console.Write("\e[s");

        // Move to last row (outside scroll region)
        Console.Write($"\e[{_height};1H");

        // Set status bar colors: white text on dark blue background
        Console.Write("\e[97;44m");

        // Pad text to fill entire row
        var padded = text.Length >= width
            ? text[..width]
            : text + new string(' ', width - text.Length);
        Console.Write(padded);

        // Reset colors
        Console.Write("\e[0m");

        // Restore cursor position
        Console.Write("\e[u");
    }

    public static void Cleanup()
    {
        // Reset scroll region to full terminal
        Console.Write("\e[r");
        Console.ResetColor();

        // Switch back to main screen buffer (restores original terminal content)
        Console.Write("\e[?1049l");
    }
}

public sealed class EndpointConfig
{
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class BackendConfig
{
    public string DeploymentName { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = [];
    public Dictionary<string, string> QueryStrings { get; set; } = [];
}

/// <summary>
/// Pipeline policy that injects custom HTTP headers and query string parameters.
/// </summary>
public sealed class CustomHeadersPolicy(
    Dictionary<string, string> headers,
    Dictionary<string, string> queryStrings) : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ApplyCustomizations(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override async ValueTask ProcessAsync(
        PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        ApplyCustomizations(message);
        await ProcessNextAsync(message, pipeline, currentIndex);
    }

    private void ApplyCustomizations(PipelineMessage message)
    {
        foreach (var (key, value) in headers)
        {
            message.Request.Headers.Set(key, value);
        }

        if (queryStrings.Count > 0 && message.Request.Uri is not null)
        {
            var uriBuilder = new UriBuilder(message.Request.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            foreach (var (key, value) in queryStrings)
            {
                query[key] = value;
            }
            uriBuilder.Query = query.ToString();
            message.Request.Uri = uriBuilder.Uri;
        }
    }
}
