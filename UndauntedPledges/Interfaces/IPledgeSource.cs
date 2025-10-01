using UndauntedPledges.Enums;
using UndauntedPledges.Models;

namespace UndauntedPledges.Interfaces;

public interface IPledgeSource
{
    PledgeSource Source { get; }

    Task<ICollection<DailyPledge>> GetDailyPledgesAsync(DateTime from, DateTime to);
}