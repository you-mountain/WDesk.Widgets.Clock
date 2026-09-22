using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        Version = "1.0.2",
        DefaultWidth = 240,
        DefaultHeight = 180,
        HasSettings = true
    };

    public override IEnumerable<WStyle> GetStyles() => new List<WStyle>
    {
        new()
        {
            Id = "style1",
            Name = "Card",
            Icon = "\uE8F1",
            PreviewEmoji = "🕐"
        },
        new()
        {
            Id = "style2",
            Name = "Digital Minimal",
            Icon = "\uE7C4",
            PreviewEmoji = "✨"
        },
        new()
        {
            Id = "style3",
            Name = "Analog",
            Icon = "\uE823",
            PreviewEmoji = "⏰"
        },
        new()
        {
            Id = "style4",
            Name = "Neon Glow",
            Icon = "\uE945",
            PreviewEmoji = "💫"
        }
    };

    public override IStyleBuilder? GetStyleBuilder(string styleId) => styleId switch
    {
        "style2" => new Style2(),
        "style3" => new Style3(),
        "style4" => new Style4(),
        _ => new Style1()
    };

    // ═══════════════════════════════════════════
    //  CreateSettingsView
    // ═══════════════════════════════════════════
    public override FrameworkElement CreateSettingsView(
        PlacedWidget instance,
        Action<Dictionary<string, string>> onSave)
    {
        var root = new StackPanel();

        // ═══ ۱. فرمت ساعت ═══
        var formatCombo = CreateComboBox();
        formatCombo.Items.Add(new ComboBoxItem { Content = "24-hour  (14:30)", Tag = "true", FontSize = 12 });
        formatCombo.Items.Add(new ComboBoxItem { Content = "12-hour  (2:30 PM)", Tag = "false", FontSize = 12 });

        var is24 = GetSetting(instance, "clock_format_24", "true") == "true";
        formatCombo.SelectedIndex = is24 ? 0 : 1;

        root.Children.Add(CreateRow("Time Format",
            "Choose between 12-hour and 24-hour", formatCombo));

        // ═══ ۲. نمایش ثانیه ═══
        var secondsToggle = CreateToggle();
        secondsToggle.IsChecked =
            GetSetting(instance, "clock_show_seconds", "true") == "true";

        root.Children.Add(CreateToggleRow("Show Seconds",
            "Display seconds in the clock", secondsToggle));

        // ═══ ۳. نمایش تاریخ ═══
        var dateToggle = CreateToggle();
        dateToggle.IsChecked =
            GetSetting(instance, "clock_show_date", "true") == "true";

        root.Children.Add(CreateToggleRow("Show Date",
            "Display the current date", dateToggle));

        // ═══ ۴. فرمت تاریخ ═══
        var dateFormatCombo = CreateComboBox();
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Long  (Monday, September 22)", Tag = "long", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Short  (Mon, Sep 22)", Tag = "short", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Numeric  (2026/09/22)", Tag = "numeric", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Persian  (۱۴۰۵/۰۶/۳۱)", Tag = "persian", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Hijri  (۱۴۴۸/۰۳/۲۰)", Tag = "hijri", FontSize = 11 });

        var currentDateFmt = GetSetting(instance, "clock_date_format", "long");
        SelectComboItem(dateFormatCombo, currentDateFmt);

        root.Children.Add(CreateRow("Date Format",
            "Choose how to display the date", dateFormatCombo));

        // ═══ ۵. منطقه زمانی ═══
        var tzCombo = CreateComboBox();
        tzCombo.Items.Add(new ComboBoxItem { Content = "Local (System Time)", Tag = "Local", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "UTC", Tag = "UTC", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Tehran  (IRST)", Tag = "Iran Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "London  (GMT)", Tag = "GMT Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Paris  (CET)", Tag = "Central Europe Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "New York  (EST)", Tag = "Eastern Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Los Angeles  (PST)", Tag = "Pacific Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Tokyo  (JST)", Tag = "Tokyo Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Dubai  (GST)", Tag = "Arabian Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Istanbul  (TRT)", Tag = "Turkey Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Moscow  (MSK)", Tag = "Russian Standard Time", FontSize = 11 });

        var currentTz = GetSetting(instance, "clock_timezone", "Local");
        SelectComboItem(tzCombo, currentTz);

        root.Children.Add(CreateRow("Time Zone",
            "Which time zone to display", tzCombo));

        // ═══ ۶. نمایش AM/PM ═══
        var ampmToggle = CreateToggle();
        ampmToggle.IsChecked =
            GetSetting(instance, "clock_show_ampm", "true") == "true";

        root.Children.Add(CreateToggleRow("Show AM/PM",
            "For 12-hour format only", ampmToggle));

        // ═══ ۷. اندازه فونت ═══
        var fontSizeCombo = CreateComboBox();
        fontSizeCombo.Items.Add(new ComboBoxItem { Content = "Small  (24px)", Tag = "24", FontSize = 11 });
        fontSizeCombo.Items.Add(new ComboBoxItem { Content = "Medium  (32px)", Tag = "32", FontSize = 11 });
        fontSizeCombo.Items.Add(new ComboBoxItem { Content = "Large  (42px)", Tag = "42", FontSize = 11 });
        fontSizeCombo.Items.Add(new ComboBoxItem { Content = "Huge  (56px)", Tag = "56", FontSize = 11 });

        var currentSize = GetSetting(instance, "clock_font_size", "32");
        SelectComboItem(fontSizeCombo, currentSize);

        root.Children.Add(CreateRow("Font Size",
            "Clock text size", fontSizeCombo));

        // ═══ ۸. Save ═══
        var saveBtn = new Button
        {
            Content = "💾  Save Settings",
            Height = 38,
            Margin = new Thickness(0, 16, 0, 0),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        saveBtn.SetResourceReference(Button.StyleProperty, "BtnSmallPrimary");

        saveBtn.Click += (s, e) =>
        {
            var settings = new Dictionary<string, string>
            {
                ["clock_format_24"] = GetComboTag(formatCombo, "true"),
                ["clock_show_seconds"] = BoolToStr(secondsToggle.IsChecked == true),
                ["clock_show_date"] = BoolToStr(dateToggle.IsChecked == true),
                ["clock_date_format"] = GetComboTag(dateFormatCombo, "long"),
                ["clock_timezone"] = GetComboTag(tzCombo, "Local"),
                ["clock_show_ampm"] = BoolToStr(ampmToggle.IsChecked == true),
                ["clock_font_size"] = GetComboTag(fontSizeCombo, "32")
            };

            onSave(settings);
        };

        root.Children.Add(saveBtn);

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

    private static string BoolToStr(bool v) => v ? "true" : "false";

    private static string GetComboTag(ComboBox combo, string defaultVal)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            return tag;
        return defaultVal;
    }

    private static void SelectComboItem(ComboBox combo, string tag)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is ComboBoxItem ci && ci.Tag is string t && t == tag)
            {
                combo.SelectedIndex = i;
                return;
            }
        }
        combo.SelectedIndex = 0;
    }

    private static ComboBox CreateComboBox()
    {
        return new ComboBox
        {
            Width = 240,
            Height = 34,
            FontSize = 12,
            VerticalContentAlignment = VerticalAlignment.Center
        };
    }

    private static CheckBox CreateToggle()
    {
        return new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 4, 0, 0)
        };
    }

    private static StackPanel CreateRow(string label, string description, FrameworkElement content)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 14)
        };

        var leftStack = new StackPanel
        {
            Width = 130,
            VerticalAlignment = VerticalAlignment.Center
        };

        leftStack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        leftStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 10,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 3, 0, 0)
        });

        row.Children.Add(leftStack);

        var rightStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };
        rightStack.Children.Add(content);
        row.Children.Add(rightStack);

        return row;
    }

    private static StackPanel CreateToggleRow(string label, string description, CheckBox toggle)
    {
        return CreateRow(label, description, toggle);
    }
}