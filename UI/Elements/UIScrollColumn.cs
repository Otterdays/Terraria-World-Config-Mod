using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WorldConfigMod.UI.Elements;

// Column container for UIList + UIScrollbar pair.
public class UIScrollColumn : UIPanel
{
    public UIList List { get; set; }
    public UIScrollbar Scrollbar { get; set; }
}
