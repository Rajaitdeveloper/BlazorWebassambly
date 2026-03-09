// Models/NavItem.cs
namespace BlazoreTestPortal.Client.Models;

/// <summary>
/// Represents a single sidebar navigation item.
/// May be a leaf link or a parent with children (group).
/// </summary>
public class NavItem
{
    /// <summary>Display label shown next to the icon.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Font Awesome icon classes, e.g. "fa-solid fa-house fa-fw".</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Route href. Null for parent items that only expand a subnav.</summary>
    public string? Href { get; set; }

    /// <summary>Use NavLinkMatch.All for exact-match routes (e.g. "/").</summary>
    public bool ExactMatch { get; set; } = false;

    /// <summary>Optional pill badge variant: "live" | "count".</summary>
    public NavBadge? Badge { get; set; }

    /// <summary>Optional status dot variant: "ok" | "warn". Used in subnav children.</summary>
    public string? StatusDot { get; set; }

    /// <summary>Child items. When non-empty this item renders as an expandable group parent.</summary>
    public List<NavItem> Children { get; set; } = [];
}

/// <summary>
/// Optional badge shown on a nav item.
/// </summary>
public class NavBadge
{
    /// <summary>"live" renders the teal LIVE chip. "count" renders the accent number badge.</summary>
    public string Type { get; set; } = "count";

    /// <summary>Text/number displayed inside the badge.</summary>
    public string Text { get; set; } = string.Empty;
}
