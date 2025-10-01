using UndauntedPledges.Enums;
using UndauntedPledges.Interfaces;
using UndauntedPledges.Models;

namespace UndauntedPledges.Implementations;

public class ApplicationPledgeSource : IPledgeSource
{
    public PledgeSource Source => PledgeSource.Application;

    public async Task<ICollection<DailyPledge>> GetDailyPledgesAsync(DateTime from, DateTime to)
    {
        return [];
    }
}