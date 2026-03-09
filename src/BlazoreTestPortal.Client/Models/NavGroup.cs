// Models/NavGroup.cs
namespace BlazoreTestPortal.Client.Models;

/// <summary>
/// A labelled section in the sidebar containing one or more NavItems.
/// </summary>
public class NavGroup
{
    /// <summary>Section heading displayed above the items, e.g. "OVERVIEW".</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Ordered list of nav items inside this group.</summary>
    public List<NavItem> Items { get; set; } = [];
}
