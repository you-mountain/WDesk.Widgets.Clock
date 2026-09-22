using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Clock.Style;

public class Style2 : IStyleBuilder
{
    public string StyleId => "style2";

    private DispatcherTimer? _timer;

    public FrameworkElement Build(PlacedWidget instance)
    {
        // ═══ تنظیمات ═══
        var is24Hour = GetSetting(instance, "clock_format_24", "true") == "true";
        var showSeconds = GetSetting(instance, "clock_show_seconds", "true") == "true";
        var showDate = GetSetting(instance, "clock_show_date", "true") == "true";
        var dateFormat = GetSetting(instance, "clock_date_format", "long");
        var timezone = GetSetting(instance, "clock_timezone", "Local");
        var showAmPm = GetSetting(instance, "clock_show_ampm", "true") == "true";

        // ═══ ROOT ═══
        var root = new Grid();

        var bg = new Border
        {
            CornerRadius = new CornerRadius(16)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══ CONTENT ═══
        var content = new Grid
        {
            Margin = new Thickness(20, 16, 20, 16)
        };
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.Children.Add(content);

        // ── Top Bar: LIVE + TZ ──
        var topBar = new Grid();
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var liveDot = new System.Windows.Shapes.Ellipse
        {
            Width = 6,
            Height = 6,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        };
        liveDot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "WidgetAccent");
        Grid.SetColumn(liveDot, 0);
        topBar.Children.Add(liveDot);

        var liveText = new TextBlock
        {
            Text = "LIVE",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        liveText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(liveText, 1);
        topBar.Children.Add(liveText);

        var tzText = new TextBlock
        {
            Text = GetTZShortName(timezone),
            FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        tzText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetColumn(tzText, 2);
        topBar.Children.Add(tzText);

        Grid.SetRow(topBar, 0);
        content.Children.Add(topBar);

        // ── Center: Big Time ──
        var centerStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var bigTime = new TextBlock
        {
            Text = "--:--",
            FontSize = 48,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New"),
            HorizontalAlignment = HorizontalAlignment.Center,
            LineHeight = 54
        };
        bigTime.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");
        centerStack.Children.Add(bigTime);

        var secondsText = new TextBlock
        {
            Text = "--",
            FontSize = 14,
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.6,
            Margin = new Thickness(0, 2, 0, 0),
            Visibility = showSeconds ? Visibility.Visible : Visibility.Collapsed
        };
        secondsText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetAccent");
        centerStack.Children.Add(secondsText);

        Grid.SetRow(centerStack, 1);
        content.Children.Add(centerStack);

        // ── Bottom: Date ──
        var dateText = new TextBlock
        {
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Visibility = showDate ? Visibility.Visible : Visibility.Collapsed
        };
        dateText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        Grid.SetRow(dateText, 2);
        content.Children.Add(dateText);

        // ═══ UPDATE UI ═══
        Action updateUI = () =>
        {
            var now = GetTimeInZone(timezone);

            string timeStr;
            string secStr = "";

            if (is24Hour)
            {
                timeStr = now.ToString("HH:mm");
                secStr = now.ToString("ss");
            }
            else
            {
                timeStr = showAmPm ? now.ToString("h:mm") : now.ToString("h:mm");
                secStr = showAmPm
                    ? now.ToString("ss") + " " + now.ToString("tt")
                    : now.ToString("ss");
            }

            bigTime.Text = timeStr;

            if (showSeconds)
                secondsText.Text = secStr;

            if (showDate)
                dateText.Text = FormatDate(now, dateFormat).ToUpperInvariant();
        };

        // ═══ TIMER ═══
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => updateUI();
        _timer.Start();

        updateUI();

        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(updateUI),
            System.Windows.Threading.DispatcherPriority.Loaded);

        root.Unloaded += (_, _) =>
        {
            try
            {
                _timer?.Stop();
                _timer = null;
            }
            catch { }
        };

        return root;
    }

    private static string GetTZShortName(string tz)
    {
        return tz switch
        {
            "Local" => "LOCAL",
            "UTC" => "UTC",
            "Iran Standard Time" => "IRST",
            "GMT Standard Time" => "GMT",
            "Central Europe Standard Time" => "CET",
            "Eastern Standard Time" => "EST",
            "Pacific Standard Time" => "PST",
            "Tokyo Standard Time" => "JST",
            "Arabian Standard Time" => "GST",
            "Turkey Standard Time" => "TRT",
            "Russian Standard Time" => "MSK",
            _ => "LOCAL"
        };
    }

    private static string GetSetting(PlacedWidget instance, string key, string defaultVal)
    {
        return instance.Settings.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val)
            ? val
            : defaultVal;
    }

    private static DateTime GetTimeInZone(string timezoneId)
    {
        if (string.IsNullOrEmpty(timezoneId) || timezoneId == "Local")
            return DateTime.Now;

        if (timezoneId == "UTC")
            return DateTime.UtcNow;

        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        }
        catch
        {
            return DateTime.Now;
        }
    }

    private static string FormatDate(DateTime dt, string format)
    {
        return format switch
        {
            "long" => dt.ToString("dddd, MMMM d"),
            "short" => dt.ToString("ddd, MMM d"),
            "numeric" => dt.ToString("yyyy/MM/dd"),
            "persian" => GetPersianDate(dt),
            "hijri" => GetHijriDate(dt),
            _ => dt.ToString("dddd, MMMM d")
        };
    }

    private static string GetPersianDate(DateTime dt)
    {
        try
        {
            var pc = new System.Globalization.PersianCalendar();
            return $"{pc.GetYear(dt)}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00}";
        }
        catch { return dt.ToString("yyyy/MM/dd"); }
    }

    private static string GetHijriDate(DateTime dt)
    {
        try
        {
            var hc = new System.Globalization.HijriCalendar();
            return $"{hc.GetYear(dt)}/{hc.GetMonth(dt):00}/{hc.GetDayOfMonth(dt):00}";
        }
        catch { return dt.ToString("yyyy/MM/dd"); }
    }
}