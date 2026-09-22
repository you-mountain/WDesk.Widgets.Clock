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

    // ... (background و بقیه)

    // ═══ تنظیمات ═══
    var is24Hour = GetSetting(instance, "clock_format_24", "true") == "true";
    var showSeconds = GetSetting(instance, "clock_show_seconds", "true") == "true";
    var showDate = GetSetting(instance, "clock_show_date", "true") == "true";
    var dateFormat = GetSetting(instance, "clock_date_format", "long");
    var timezone = GetSetting(instance, "clock_timezone", "Local");

    // ... (بقیه UI)

    // ═══ Time Text ═══
    var timeText = new TextBlock
    {
        FontSize = 32,
        FontWeight = FontWeights.Light,
        HorizontalAlignment = HorizontalAlignment.Center
    };
    timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextPrimary");

    // ═══ Date Text ═══
    var dateText = new TextBlock
    {
        FontSize = 11,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 4, 0, 0),
        Visibility = showDate ? Visibility.Visible : Visibility.Collapsed
    };
    dateText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");

    // ═══ Timer ═══
    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
    timer.Tick += (_, _) =>
    {
        var now = GetTimeInZone(timezone);

        // ── Time ──
        string timeStr;
        if (is24Hour)
            timeStr = showSeconds ? now.ToString("HH:mm:ss") : now.ToString("HH:mm");
        else
            timeStr = showSeconds ? now.ToString("h:mm:ss tt") : now.ToString("h:mm tt");

        timeText.Text = timeStr;

        // ── Date ──
        if (showDate)
        {
            dateText.Text = dateFormat switch
            {
                "long" => now.ToString("dddd, MMMM d"),
                "short" => now.ToString("ddd, MMM d"),
                "numeric" => now.ToString("yyyy/MM/dd"),
                "persian" => GetPersianDate(now),
                _ => now.ToString("dddd, MMMM d")
            };
        }
    };
    timer.Start();

    // آپدیت اولیه
    var now0 = GetTimeInZone(timezone);
    timeText.Text = is24Hour
        ? (showSeconds ? now0.ToString("HH:mm:ss") : now0.ToString("HH:mm"))
        : (showSeconds ? now0.ToString("h:mm:ss tt") : now0.ToString("h:mm tt"));

    if (showDate)
    {
        dateText.Text = dateFormat switch
        {
            "long" => now0.ToString("dddd, MMMM d"),
            "short" => now0.ToString("ddd, MMM d"),
            "numeric" => now0.ToString("yyyy/MM/dd"),
            "persian" => GetPersianDate(now0),
            _ => now0.ToString("dddd, MMMM d")
        };
    }

    root.Unloaded += (_, _) => timer.Stop();

    return root;
}

// ═══════════════════════════════════════════
//  Helpers
// ═══════════════════════════════════════════
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

private static string GetPersianDate(DateTime dt)
{
    try
    {
        var pc = new System.Globalization.PersianCalendar();
        var y = pc.GetYear(dt);
        var m = pc.GetMonth(dt);
        var d = pc.GetDayOfMonth(dt);
        return $"{y}/{m:00}/{d:00}";
    }
    catch
    {
        return dt.ToString("yyyy/MM/dd");
    }
}
}