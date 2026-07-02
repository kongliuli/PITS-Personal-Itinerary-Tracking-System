using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PITS.MVP.Core.Entities;
using PITS.MVP.Core.Services;
using System.Collections.ObjectModel;

namespace PITS.MVP.App.ViewModels;

public partial class AIChatViewModel : BaseViewModel
{
    private readonly ITripService _tripService;
    private readonly ITripPlanService _planService;

    [ObservableProperty] private string _inputText = "";
    
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public AIChatViewModel(ITripService tripService, ITripPlanService planService)
    {
        _tripService = tripService;
        _planService = planService;
        Title = "AI 助手";
        Messages.Add(new ChatMessage 
        { 
            Content = "你好！我是 PITS AI 助手。你可以告诉我：\n" +
                     "• \"记录今天下午3点到5点在公司开会\"\n" +
                     "• \"上周去了哪些地方\"\n" +
                     "• \"本月工作了多少小时\"",
            IsUser = false 
        });
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;

        var userMessage = InputText;
        Messages.Add(new ChatMessage { Content = userMessage, IsUser = true });
        InputText = "";

        await ExecuteAsync(async () =>
        {
            var response = await ProcessUserInputAsync(userMessage);
            if (!string.IsNullOrWhiteSpace(response))
                Messages.Add(new ChatMessage { Content = response, IsUser = false });
        });
    }

    private async Task<string> ProcessUserInputAsync(string input)
    {
        var lowerInput = input.ToLower();

        if (lowerInput.Contains("计划") || lowerInput.Contains("安排") || lowerInput.Contains("添加"))
        {
            var plan = ParsePlan(input);
            Messages.Add(new ChatMessage
            {
                Content = "我解析到一个计划，请确认后保存。",
                IsUser = false,
                HasTripCard = true,
                TripCard = new TripCard
                {
                    Title = plan.Title,
                    Time = $"{plan.StartsAt:g}",
                    Location = plan.LocationName,
                    ConfirmCommand = new Command(async () =>
                    {
                        await _planService.AddAsync(plan);
                        Messages.Add(new ChatMessage { Content = $"已保存计划：{plan.Title}", IsUser = false });
                    })
                }
            });
            return "";
        }

        if (lowerInput.Contains("上周") || lowerInput.Contains("本周") || lowerInput.Contains("本月"))
        {
            var now = DateTime.Now;
            var (start, end) = lowerInput.Contains("上周") 
                ? (now.AddDays(-7 - (int)now.DayOfWeek), now.AddDays(-(int)now.DayOfWeek))
                : lowerInput.Contains("本周")
                    ? (now.AddDays(-(int)now.DayOfWeek), now)
                    : (new DateTime(now.Year, now.Month, 1), now);

            var trips = await _tripService.GetByDateRangeAsync(start, end);
            var workTrips = trips.Where(t => t.ActivityType == ActivityType.Work).ToList();
            var totalHours = trips.Where(t => t.EndedAt != null)
                .Sum(t => (t.EndedAt!.Value - t.StartedAt).TotalHours);

            return $"{(lowerInput.Contains("上周") ? "上周" : lowerInput.Contains("本周") ? "本周" : "本月")}统计：\n" +
                   $"• 共 {trips.Count()} 条行程记录\n" +
                   $"• 总计约 {totalHours:F1} 小时\n" +
                   $"• 工作行程 {workTrips.Count} 次";
        }

        if (lowerInput.Contains("在哪") || lowerInput.Contains("去了"))
        {
            var trips = await _tripService.GetAllAsync();
            var recentTrips = trips.Take(5).ToList();
            
            if (recentTrips.Any())
            {
                return "最近的行程：\n" + string.Join("\n", recentTrips.Select(t => 
                    $"• {t.StartedAt:MM-dd HH:mm} - {t.Address ?? t.Description ?? "未知地点"}"));
            }
            return "暂无行程记录。";
        }

        return "我理解了你的输入。目前 AI 功能正在完善中，你可以尝试：\n" +
               "• 查询上周/本周/本月的行程统计\n" +
               "• 询问最近的行程记录\n" +
               "• 使用记录页面添加新行程";
    }

    private static TripPlan ParsePlan(string input)
    {
        var now = DateTime.Now;
        var startsAt = input.Contains("明天") ? now.Date.AddDays(1).AddHours(9) : now.Date.AddHours(now.Hour + 1);
        if (input.Contains("下午")) startsAt = startsAt.Date.AddHours(15);
        if (input.Contains("晚上")) startsAt = startsAt.Date.AddHours(19);

        var activity = input.Contains("通勤") ? ActivityType.Commute :
            input.Contains("出差") || input.Contains("旅行") ? ActivityType.Travel :
            input.Contains("学习") ? ActivityType.Study :
            input.Contains("运动") || input.Contains("健身") ? ActivityType.Health :
            input.Contains("会议") || input.Contains("客户") || input.Contains("公司") ? ActivityType.Work :
            ActivityType.Other;

        return new TripPlan
        {
            Title = input,
            StartsAt = startsAt,
            EndsAt = startsAt.AddHours(1),
            LocationName = ExtractLocation(input),
            ActivityType = activity,
            Source = DataSource.AiParse,
            Status = PlanStatus.Planned
        };
    }

    private static string? ExtractLocation(string input)
    {
        var markers = new[] { "在", "去", "到" };
        foreach (var marker in markers)
        {
            var index = input.IndexOf(marker, StringComparison.Ordinal);
            if (index >= 0 && index < input.Length - 1)
                return input[(index + marker.Length)..].Split(' ', '，', ',', '。').FirstOrDefault();
        }
        return null;
    }
}

public partial class ChatMessage : ObservableObject
{
    public string Content { get; set; } = "";
    public bool IsUser { get; set; }
    public bool HasTripCard { get; set; }
    public TripCard? TripCard { get; set; }
}

public record TripCard
{
    public string Title { get; set; } = "";
    public string Time { get; set; } = "";
    public string? Location { get; set; }
    public Command? ConfirmCommand { get; set; }
}
