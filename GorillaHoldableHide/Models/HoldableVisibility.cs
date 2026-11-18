using System.ComponentModel;

namespace GorillaHoldableHide.Models;

internal enum HoldableVisibility
{
    [Description("Shown to every point of view")]
    Visible,
    [Description("Hidden to only the player's point of view")]
    FPHidden,
    [Description("Shown to only the player's point of view")]
    TPHidden,
    [Description("Hidden to every point of view")]
    Hidden
}
