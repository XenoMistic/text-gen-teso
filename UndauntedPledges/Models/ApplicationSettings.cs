using UndauntedPledges.Enums;

namespace UndauntedPledges.Models;

public class ApplicationSettings
{
    public ICollection<Role> Roles { get; set; } = [];

    public string Template { get; set; } = string.Empty;
}