using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Clock.Style;

public class Style1 : IStyleBuilder
{
    public string StyleId => "style1";

    public FrameworkElement Build(PlacedWidget instance)
    {
        var root = new Grid();

        // ═══════════════════════════════════════════
        //  Background
        // ═══════════════════════════════════════════
        var bg = new Border
        {
            CornerRadius = new CornerRadius(20)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══════════════════════════════════════════
        //  Content
        // ═══════════════════════════════════════════
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16)
        };

        var title = new TextBlock
        {
            Text = "CLOCK",
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(title);

        var timeText = new TextBlock
        {
            Text = "00:00:00",
            FontSize = 32,
            FontWeight = FontWeights.Light,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };
        timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        stack.Children.Add(timeText);

        var dateText = new TextBlock
        {
            Text = "",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };
        dateText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        stack.Children.Add(dateText);

        root.Children.Add(stack);

        // ═══════════════════════════════════════════
        //  Timer
        // ═══════════════════════════════════════════
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            var now = DateTime.Now;
            timeText.Text = now.ToString("HH:mm:ss");
            dateText.Text = now.ToString("dddd, MMMM d");
        };
        timer.Start();

        // آپدیت اولیه
        var now0 = DateTime.Now;
        timeText.Text = now0.ToString("HH:mm:ss");
        dateText.Text = now0.ToString("dddd, MMMM d");

        root.Unloaded += (_, _) => timer.Stop();

        return root;
    }
}