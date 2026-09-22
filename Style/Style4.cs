using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Clock.Style;

public class Style4 : IStyleBuilder
{
    public string StyleId => "style4";

    private DispatcherTimer? _timer;

    public FrameworkElement Build(PlacedWidget instance)
    {
        var is24Hour = GetSetting(instance, "clock_format_24", "true") == "true";
        var showSeconds = GetSetting(instance, "clock_show_seconds", "true") == "true";
        var showDate = GetSetting(instance, "clock_show_date", "true") == "true";
        var dateFormat = GetSetting(instance, "clock_date_format", "long");
        var timezone = GetSetting(instance, "clock_timezone", "Local");
        var showAmPm = GetSetting(instance, "clock_show_ampm", "true") == "true";

        // ═══ ROOT ═══
        var root = new Grid();

        // ── Background (تیره‌تر) ═══
        var bg = new Border
        {
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromRgb(0x0A, 0x0A, 0x12))
        };
        root.Children.Add(bg);

        // ═══ CONTENT ═══
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        // ── Title ──
        var title = new TextBlock
        {
            Text = "◆ NEON CLOCK ◆",
            FontSize = 9,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = (Brush)Application.Current.FindResource("WidgetAccent")
        };
        title.Effect = new DropShadowEffect
        {
            Color = GetAccentColor(instance),
            BlurRadius = 12,
            ShadowDepth = 0,
            Opacity = 0.8
        };
        stack.Children.Add(title);

        // ── Time ──
        var timeText = new TextBlock
        {
            FontSize = 42,
            FontWeight = FontWeights.Light,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        timeText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetAccent");
        timeText.Effect = new DropShadowEffect
        {
            Color = GetAccentColor(instance),
            BlurRadius = 20,
            ShadowDepth = 0,
            Opacity = 1.0
        };
        stack.Children.Add(timeText);

        // ── Date ──
        var dateText = new TextBlock
        {
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Visibility = showDate ? Visibility.Visible : Visibility.Collapsed,
            Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF))
        };
        stack.Children.Add(dateText);

        // ── Line ──
        var line = new Border
        {
            Height = 1,
            Width = 80,
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = (Brush)Application.Current.FindResource("WidgetAccent")
        };
        line.Effect = new DropShadowEffect
        {
            Color = GetAccentColor(instance),
            BlurRadius = 8,
            ShadowDepth = 0,
            Opacity = 0.8
        };
        stack.Children.Add(line);

        root.Children.Add(stack);

        // ═══ UPDATE UI ═══
        Action updateUI = () =>
        {
            var now = GetTimeInZone(timezone);

            string timeStr;
            if (is24Hour)
            {
                timeStr = showSeconds
                    ? now.ToString("HH:mm:ss")
                    : now.ToString("HH:mm");
            }
            else
            {
                if (showSeconds && showAmPm)
                    timeStr = now.ToString("h:mm:ss tt");
                else if (showSeconds)
                    timeStr = now.ToString("h:mm:ss");
                else if (showAmPm)
                    timeStr = now.ToString("h:mm tt");
                else
                    timeStr = now.ToString("h:mm");
            }

            timeText.Text = timeStr;

            if (showDate)
                dateText.Text = FormatDate(now, dateFormat);
        };

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

    private static Color GetAccentColor(PlacedWidget instance)
{
    try
    {
        // ★ اگه ویجت accent custom داره
        if (instance.Settings.TryGetValue("accentColor", out var hex) &&
            !string.IsNullOrEmpty(hex))
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        // ★ از Resource بخون (WDesk accent رو توی Application.Current.Resources ست می‌کنه)
        try
        {
            var app = Application.Current;
            if (app != null)
            {
                var accentBrush = app.TryFindResource("WidgetAccent") as SolidColorBrush
                    ?? app.TryFindResource("AccentBrush") as SolidColorBrush;

                if (accentBrush != null)
                    return accentBrush.Color;
            }
        }
        catch { }

        // ★ Fallback
        return Color.FromRgb(0x3B, 0x82, 0xF6);
    }
    catch
    {
        return Color.FromRgb(0x3B, 0x82, 0xF6);
    }
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
        catch { return DateTime.Now; }
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