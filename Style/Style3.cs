using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WDesk.Core;

namespace WDesk.Widgets.Clock.Style;

public class Style3 : IStyleBuilder
{
    public string StyleId => "style3";

    private DispatcherTimer? _timer;

    public FrameworkElement Build(PlacedWidget instance)
    {
        var timezone = GetSetting(instance, "clock_timezone", "Local");
        var showDate = GetSetting(instance, "clock_show_date", "true") == "true";
        var dateFormat = GetSetting(instance, "clock_date_format", "long");

        // ═══ ROOT ═══
        var root = new Grid();

        var bg = new Border
        {
            CornerRadius = new CornerRadius(16)
        };
        bg.SetResourceReference(Border.BackgroundProperty, "WidgetBg");
        root.Children.Add(bg);

        // ═══ CONTENT ═══
        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16)
        };

        // ═══ ANALOG CLOCK ═══
        var canvas = new Canvas
        {
            Width = 120,
            Height = 120,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var centerX = 60.0;
        var centerY = 60.0;
        var clockRadius = 55.0;

        // ── دایره بیرونی ──
        var outerCircle = new Ellipse
        {
            Width = clockRadius * 2,
            Height = clockRadius * 2,
            StrokeThickness = 2
        };
        outerCircle.SetResourceReference(Shape.StrokeProperty, "WidgetAccent");
        Canvas.SetLeft(outerCircle, centerX - clockRadius);
        Canvas.SetTop(outerCircle, centerY - clockRadius);
        canvas.Children.Add(outerCircle);

        // ── دایره داخلی ──
        var innerCircle = new Ellipse
        {
            Width = clockRadius * 2 - 8,
            Height = clockRadius * 2 - 8,
            StrokeThickness = 0.5,
            Opacity = 0.3
        };
        innerCircle.SetResourceReference(Shape.StrokeProperty, "WidgetTextMuted");
        Canvas.SetLeft(innerCircle, centerX - clockRadius + 4);
        Canvas.SetTop(innerCircle, centerY - clockRadius + 4);
        canvas.Children.Add(innerCircle);

        // ── ۱۲ نقطه ساعت ──
        for (int i = 0; i < 12; i++)
        {
            var angle = (i * 30 - 90) * Math.PI / 180;
            var outerR = clockRadius - 4;
            var innerR = clockRadius - 8;

            var x1 = centerX + Math.Cos(angle) * innerR;
            var y1 = centerY + Math.Sin(angle) * innerR;
            var x2 = centerX + Math.Cos(angle) * outerR;
            var y2 = centerY + Math.Sin(angle) * outerR;

            var tick = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                StrokeThickness = (i % 3 == 0) ? 2 : 1
            };
            tick.SetResourceReference(Shape.StrokeProperty, "WidgetTextSecondary");
            canvas.Children.Add(tick);
        }

        // ── عقربه ساعت ──
        var hourHand = new Line
        {
            X1 = centerX,
            Y1 = centerY,
            X2 = centerX,
            Y2 = centerY - 25,
            StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        hourHand.SetResourceReference(Shape.StrokeProperty, "WidgetTextPrimary");
        canvas.Children.Add(hourHand);

        // ── عقربه دقیقه ──
        var minuteHand = new Line
        {
            X1 = centerX,
            Y1 = centerY,
            X2 = centerX,
            Y2 = centerY - 38,
            StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        minuteHand.SetResourceReference(Shape.StrokeProperty, "WidgetTextPrimary");
        canvas.Children.Add(minuteHand);

        // ── عقربه ثانیه ──
        var secondHand = new Line
        {
            X1 = centerX,
            Y1 = centerY,
            X2 = centerX,
            Y2 = centerY - 45,
            StrokeThickness = 1,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        secondHand.SetResourceReference(Shape.StrokeProperty, "WidgetAccent");
        canvas.Children.Add(secondHand);

        // ── نقطه مرکز ──
        var centerDot = new Ellipse
        {
            Width = 8,
            Height = 8
        };
        centerDot.SetResourceReference(Shape.FillProperty, "WidgetAccent");
        Canvas.SetLeft(centerDot, centerX - 4);
        Canvas.SetTop(centerDot, centerY - 4);
        canvas.Children.Add(centerDot);

        stack.Children.Add(canvas);

        // ═══ DIGITAL TIME (کوچک، زیر) ═══
        var digitalTime = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            FontFamily = new FontFamily("Cascadia Mono, Consolas"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0)
        };
        digitalTime.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextSecondary");
        stack.Children.Add(digitalTime);

        // ═══ DATE ═══
        var dateText = new TextBlock
        {
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
            Visibility = showDate ? Visibility.Visible : Visibility.Collapsed
        };
        dateText.SetResourceReference(TextBlock.ForegroundProperty, "WidgetTextMuted");
        stack.Children.Add(dateText);

        root.Children.Add(stack);

        // ═══ UPDATE UI ═══
        Action updateUI = () =>
        {
            var now = GetTimeInZone(timezone);

            // ── عقربه‌ها ──
            var hourAngle = ((now.Hour % 12) * 30 + now.Minute * 0.5) - 90;
            var minuteAngle = (now.Minute * 6 + now.Second * 0.1) - 90;
            var secondAngle = (now.Second * 6) - 90;

            UpdateHand(hourHand, centerX, centerY, hourAngle, 25);
            UpdateHand(minuteHand, centerX, centerY, minuteAngle, 38);
            UpdateHand(secondHand, centerX, centerY, secondAngle, 45);

            // ── Digital ──
            digitalTime.Text = now.ToString("HH:mm:ss");

            // ── Date ──
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

    private static void UpdateHand(Line hand, double cx, double cy, double angleDeg, double length)
    {
        var angle = angleDeg * Math.PI / 180;
        hand.X2 = cx + Math.Cos(angle) * length;
        hand.Y2 = cy + Math.Sin(angle) * length;
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