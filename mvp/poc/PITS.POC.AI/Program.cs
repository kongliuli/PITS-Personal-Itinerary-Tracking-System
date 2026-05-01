using Microsoft.SemanticKernel;

namespace PITS.POC.AI;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== PITS AI POC ===\n");

        Console.WriteLine("--- Test 1: Semantic Kernel Setup ---");
        var builder = Kernel.CreateBuilder();
        Console.WriteLine("  Kernel builder created successfully");
        var kernel = builder.Build();
        Console.WriteLine("  Kernel built successfully\n");

        Console.WriteLine("--- Test 2: Trip Log Plugin Definition ---");
        
        var tripLogPlugin = new TripLogPlugin();
        kernel.Plugins.AddFromObject(tripLogPlugin, "TripLog");
        Console.WriteLine("  TripLogPlugin registered\n");

        Console.WriteLine("--- Test 3: Intent Parsing Simulation ---");
        var testInputs = new[]
        {
            "记录今天下午3点到5点在公司开会",
            "上周去了哪些地方",
            "这个月工作了多少小时",
            "添加一个新行程：明天上午9点在客户公司拜访"
        };

        foreach (var input in testInputs)
        {
            var intent = tripLogPlugin.ParseIntent(input);
            Console.WriteLine($"  Input: {input}");
            Console.WriteLine($"  Intent Type: {intent.IntentType}");
            Console.WriteLine($"  Parsed Data: {intent.ParsedData}");
            Console.WriteLine();
        }

        Console.WriteLine("--- Test 4: Trip Query Plugin ---");
        var tripQueryPlugin = new TripQueryPlugin();
        kernel.Plugins.AddFromObject(tripQueryPlugin, "TripQuery");
        Console.WriteLine("  TripQueryPlugin registered\n");

        Console.WriteLine("--- Test 5: Query Simulation ---");
        var queryResults = tripQueryPlugin.QueryTrips("last_week", null, null);
        Console.WriteLine($"  Query Result: {queryResults}\n");

        Console.WriteLine("=== AI POC Structure Ready ===");
        Console.WriteLine("Note: Full Ollama integration requires running Ollama service.");
        Console.WriteLine("Run 'ollama pull qwen2.5:14b' to enable local LLM.\n");
    }
}

public class TripLogPlugin
{
    public IntentResult ParseIntent(string input)
    {
        var lowerInput = input.ToLower();
        
        if (lowerInput.Contains("记录") || lowerInput.Contains("添加") || lowerInput.Contains("新建"))
        {
            return new IntentResult
            {
                IntentType = IntentType.CreateTrip,
                ParsedData = ExtractTripInfo(input)
            };
        }
        
        if (lowerInput.Contains("多少") || lowerInput.Contains("统计") || lowerInput.Contains("小时"))
        {
            return new IntentResult
            {
                IntentType = IntentType.Query,
                ParsedData = ExtractQueryInfo(input)
            };
        }

        if (lowerInput.Contains("哪") || lowerInput.Contains("去了"))
        {
            return new IntentResult
            {
                IntentType = IntentType.QueryPlaces,
                ParsedData = ExtractQueryInfo(input)
            };
        }

        return new IntentResult
        {
            IntentType = IntentType.Unknown,
            ParsedData = "Unable to parse intent"
        };
    }

    private string ExtractTripInfo(string input)
    {
        var info = new System.Text.StringBuilder();
        
        if (input.Contains("今天"))
            info.AppendLine("Date: Today");
        else if (input.Contains("明天"))
            info.AppendLine("Date: Tomorrow");
        else if (input.Contains("上午") || input.Contains("下午") || input.Contains("晚上"))
            info.AppendLine("Time: Morning/Afternoon/Evening");
            
        if (input.Contains("公司"))
            info.AppendLine("Location: Office");
        else if (input.Contains("客户"))
            info.AppendLine("Location: Client Site");
            
        if (input.Contains("开会") || input.Contains("会议"))
            info.AppendLine("Activity: Meeting");
        else if (input.Contains("拜访"))
            info.AppendLine("Activity: Visit");

        return info.ToString();
    }

    private string ExtractQueryInfo(string input)
    {
        var lowerInput = input.ToLower();
        
        if (lowerInput.Contains("上周"))
            return "TimeRange: Last Week";
        else if (lowerInput.Contains("本周"))
            return "TimeRange: This Week";
        else if (lowerInput.Contains("本月"))
            return "TimeRange: This Month";
            
        return "TimeRange: Unknown";
    }
}

public class TripQueryPlugin
{
    public string QueryTrips(string timeRange, string? location = null, string? activityType = null)
    {
        return $"Query: {timeRange}, Location: {location ?? "All"}, Activity: {activityType ?? "All"}";
    }

    public string SemanticSearch(string query)
    {
        return $"Semantic search for: {query}";
    }

    public string Summarize(string period, string? groupBy = null)
    {
        return $"Summary for {period}, grouped by {groupBy ?? "default"}";
    }
}

public class IntentResult
{
    public IntentType IntentType { get; set; }
    public string ParsedData { get; set; } = "";
}

public enum IntentType
{
    Unknown,
    CreateTrip,
    Query,
    QueryPlaces,
    DeleteTrip,
    UpdateTrip
}
