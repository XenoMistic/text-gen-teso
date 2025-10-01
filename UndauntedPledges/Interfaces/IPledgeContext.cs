using UndauntedPledges.Enums;
using UndauntedPledges.Models;

namespace UndauntedPledges.Interfaces;

public interface IPledgeContext
{
    Task<ICollection<DailyPledge>> GetDailyPledgeAsync(PledgeSource source, DateTime from, DateTime to);
}