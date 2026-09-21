using System.Windows;
using WDesk.Core;
using WDesk.Widgets.Clock.Style;

namespace WDesk.Widgets.Clock;

public class ClockWidget : WidgetBase
{
    public override WidgetMetadata Metadata { get; } = new()
    {
        Id = "clock",
        NameKey = "widget.clock.name",
        DescriptionKey = "widget.clock.desc",
        Category = WidgetCategory.Time,
        Icon = "\uE823",
        Author = "WDesk Team",
        Version = "1.0.0",
        DefaultWidth = 240,
        DefaultHeight = 180,
        HasSettings = false
    };

    public override FrameworkElement CreateView(PlacedWidget instance)
    {
        return new Style1().Build(instance);
    }
}
