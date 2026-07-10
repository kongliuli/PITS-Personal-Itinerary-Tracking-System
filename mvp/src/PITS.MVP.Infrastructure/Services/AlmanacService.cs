using PITS.MVP.Core.Services;

namespace PITS.MVP.Infrastructure.Services;

public class AlmanacService : IAlmanacService
{
    public Task<AlmanacDay> GetAsync(DateTime date)
    {
        return Task.FromResult(new AlmanacDay
        {
            Date = date.Date,
            Summary = "v1 黄历 API 未配置",
            IsConfigured = false
        });
    }
}
