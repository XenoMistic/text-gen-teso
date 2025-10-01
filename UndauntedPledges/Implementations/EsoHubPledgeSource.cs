using AngleSharp.Html.Parser;
using System.Diagnostics;
using System.Net.Http;
using AngleSharp.Dom;
using Microsoft.Extensions.Options;
using UndauntedPledges.Enums;
using UndauntedPledges.Interfaces;
using UndauntedPledges.Models;

namespace UndauntedPledges.Implementations;

public class EsoHubPledgeSource : IPledgeSource
{
    private const string Url = "https://eso-hub.com/ru/daily-undaunted-pledges";

    private readonly HttpClient _client;

    private readonly Dictionary<string, string> _pledgeMapper = [];

    public EsoHubPledgeSource(IOptions<PledgesMap> options)
    {
        _client = new HttpClient();

        foreach (var pledge in options.Value.Items)
        {
            _pledgeMapper[pledge.Name] = pledge.DisplayName;
        }
    }

    public PledgeSource Source => PledgeSource.EsoHub;

    public async Task<ICollection<DailyPledge>> GetDailyPledgesAsync(DateTime from, DateTime to)
    {
        var page = await GetPageAsync()
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(page))
        {
            Trace.WriteLine("Не удалось получить данные из eso-hub");

            return [];
        }

        var parsedPledges = await ParsePageAsync(page)
            .ConfigureAwait(false);

        foreach (var pledge in parsedPledges)
        {
            if (_pledgeMapper.TryGetValue(pledge.UrgarlagPledge, out var tUrgarlagPledge))
            {
                pledge.UrgarlagPledge = tUrgarlagPledge;
            }

            if (_pledgeMapper.TryGetValue(pledge.GlirionPledge, out var tGlirionPledge))
            {
                pledge.GlirionPledge = tGlirionPledge;
            }

            if (_pledgeMapper.TryGetValue(pledge.MajPledge, out var tMajPledge))
            {
                pledge.MajPledge = tMajPledge;
            }
        }

        return parsedPledges;
    }

    private async Task<ICollection<DailyPledge>> ParsePageAsync(string page)
    {
        var parser = new HtmlParser();

        var document = await parser.ParseDocumentAsync(page)
            .ConfigureAwait(false);

        var result = new List<DailyPledge>();

        var current = document.QuerySelector("section#current");
        if (current is not null)
        {
            var cells = current.QuerySelectorAll("a");

            var cellNumber = 0;
            var pledge = new DailyPledge
            {
                DateTime = DateTimeOffset.UtcNow.AddHours(-3).DateTime
            };

            foreach (var cell in cells)
            {
                var value = cell.QuerySelector("div > div")?.InnerHtml ?? string.Empty;
                switch (cellNumber)
                {
                    case 0:
                        pledge.MajPledge = value;
                        break;

                    case 1:
                        pledge.GlirionPledge = value;
                        break;

                    case 2:
                        pledge.UrgarlagPledge = value;
                        break;
                }

                cellNumber += 1;
            }

            result.Add(pledge);
        }

        var upcoming = document.QuerySelector("section#upcoming");
        if (upcoming is not null)
        {
            var rows = upcoming.QuerySelectorAll("tr");

            var rowNumber = 1;
            foreach (var row in rows)
            {
                var cells = row.QuerySelectorAll("td");

                var pledge = ParseRow(cells, rowNumber);

                rowNumber += 1;
                result.Add(pledge);
            }
        }

        return result;
    }

    private static DailyPledge ParseRow(IHtmlCollection<IElement> cells, int rowNumber)
    {
        var cellNumber = 0;
        var pledge = new DailyPledge();
        foreach (var cell in cells)
        {
            var child = cell.Children.FirstOrDefault();

            var value = child is not null
                ? child.InnerHtml.Trim()
                : cell.InnerHtml.Trim();

            switch (cellNumber)
            {
                case 0:
                    pledge.DateTime = DateTimeOffset.UtcNow.AddHours(-3).DateTime.AddDays(rowNumber);
                    break;

                case 1:
                    pledge.MajPledge = value;
                    break;

                case 2:
                    pledge.GlirionPledge = value;
                    break;

                case 3:
                    pledge.UrgarlagPledge = value;
                    break;
            }

            cellNumber += 1;
        }

        return pledge;
    }

    private async Task<string> GetPageAsync()
    {
        var response = await _client.GetAsync(Url)
            .ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);
        }

        Trace.WriteLine($"Error. Http code: {response.StatusCode}");

        return string.Empty;
    }
}