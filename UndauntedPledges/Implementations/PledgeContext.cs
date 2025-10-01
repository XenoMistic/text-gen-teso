using UndauntedPledges.Enums;
using UndauntedPledges.Interfaces;
using UndauntedPledges.Models;

namespace UndauntedPledges.Implementations;

public class PledgeContext : IPledgeContext
{
    private readonly IDictionary<PledgeSource, IPledgeSource> _sources;

    public PledgeContext(IEnumerable<IPledgeSource> sources)
    {
        _sources = sources.ToDictionary(x => x.Source);
    }

    public async Task<ICollection<DailyPledge>> GetDailyPledgeAsync(PledgeSource source, DateTime from, DateTime to)
    {
        if (!_sources.TryGetValue(source, out var impl))
        {
            throw new InvalidOperationException($"Не найдена реализация для источника {source}");
        }

        return await impl.GetDailyPledgesAsync(from, to)
            .ConfigureAwait(false);
    }
}