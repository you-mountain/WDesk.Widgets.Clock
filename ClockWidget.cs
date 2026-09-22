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
        Version = "1.0.0",
        DefaultWidth = 240,
        DefaultHeight = 180,
        HasSettings = true    // ★ حالا true
    };

    public override IEnumerable<WStyle> GetStyles() => new List<WStyle>
    {
        new()
        {
            Id = "style1",
            Name = "Default",
            Icon = "\uE823",
            PreviewEmoji = "🕐"
        }
    };

    public override IStyleBuilder? GetStyleBuilder(string styleId) => styleId switch
    {
        "style1" => new Style1(),
        _ => new Style1()
    };

    // ═══════════════════════════════════════════
    //  ★ CreateSettingsView
    // ═══════════════════════════════════════════
    public override FrameworkElement CreateSettingsView(
        PlacedWidget instance,
        Action<Dictionary<string, string>> onSave)
    {
        var root = new StackPanel();

        // ═══ ۱. فرمت ساعت (12/24) ═══
        var formatRow = CreateRow(
            "Time Format",
            "Choose between 12-hour and 24-hour");

        var formatCombo = new ComboBox
        {
            Width = 140,
            FontSize = 12
        };

        var is24 = GetSetting(instance, "clock_format_24", "true") == "true";
        formatCombo.Items.Add(new ComboBoxItem
        {
            Content = "24-hour (14:30)",
            Tag = "true",
            FontSize = 12
        });
        formatCombo.Items.Add(new ComboBoxItem
        {
            Content = "12-hour (2:30 PM)",
            Tag = "false",
            FontSize = 12
        });
        formatCombo.SelectedIndex = is24 ? 0 : 1;

        ((StackPanel)formatRow.Children[1]).Children.Add(formatCombo);

        root.Children.Add(formatRow);

        // ═══ ۲. نمایش ثانیه ═══
        var secondsRow = CreateToggleRow(
            "Show Seconds",
            "Display seconds in the clock");

        var secondsToggle = (CheckBox)secondsRow.Tag;
        secondsToggle.IsChecked =
            GetSetting(instance, "clock_show_seconds", "true") == "true";

        root.Children.Add(secondsRow);

        // ═══ ۳. نمایش تاریخ ═══
        var dateRow = CreateToggleRow(
            "Show Date",
            "Display the current date");

        var dateToggle = (CheckBox)dateRow.Tag;
        dateToggle.IsChecked =
            GetSetting(instance, "clock_show_date", "true") == "true";

        root.Children.Add(dateRow);

        // ═══ ۴. فرمت تاریخ ═══
        var dateFormatRow = CreateRow(
            "Date Format",
            "Choose date display format");

        var dateFormatCombo = new ComboBox
        {
            Width = 140,
            FontSize = 12
        };

        var currentDateFmt = GetSetting(instance, "clock_date_format", "long");
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Long (Monday, Sep 22)", Tag = "long", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Short (Mon, Sep 22)", Tag = "short", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Numeric (2026/09/22)", Tag = "numeric", FontSize = 11 });
        dateFormatCombo.Items.Add(new ComboBoxItem { Content = "Persian (۱۴۰۵/۰۶/۳۱)", Tag = "persian", FontSize = 11 });

        for (int i = 0; i < dateFormatCombo.Items.Count; i++)
        {
            if (dateFormatCombo.Items[i] is ComboBoxItem ci &&
                ci.Tag is string tag && tag == currentDateFmt)
            {
                dateFormatCombo.SelectedIndex = i;
                break;
            }
        }

        ((StackPanel)dateFormatRow.Children[1]).Children.Add(dateFormatCombo);

        root.Children.Add(dateFormatRow);

        // ═══ ۵. منطقه زمانی ═══
        var tzRow = CreateRow(
            "Time Zone",
            "Which time zone to display");

        var tzCombo = new ComboBox
        {
            Width = 200,
            FontSize = 12
        };

        var currentTz = GetSetting(instance, "clock_timezone", "Local");

        tzCombo.Items.Add(new ComboBoxItem { Content = "Local (System)", Tag = "Local", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "UTC", Tag = "UTC", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Tehran (IRST)", Tag = "Iran Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "London (GMT)", Tag = "GMT Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "New York (EST)", Tag = "Eastern Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Los Angeles (PST)", Tag = "Pacific Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Tokyo (JST)", Tag = "Tokyo Standard Time", FontSize = 11 });
        tzCombo.Items.Add(new ComboBoxItem { Content = "Dubai (GST)", Tag = "Arabian Standard Time", FontSize = 11 });

        for (int i = 0; i < tzCombo.Items.Count; i++)
        {
            if (tzCombo.Items[i] is ComboBoxItem ci &&
                ci.Tag is string tag && tag == currentTz)
            {
                tzCombo.SelectedIndex = i;
                break;
            }
        }

        ((StackPanel)tzRow.Children[1]).Children.Add(tzCombo);

        root.Children.Add(tzRow);

        // ═══ ۶. Save Button ═══
        var saveBtn = new Button
        {
            Content = "💾  Save Settings",
            Height = 36,
            Margin = new Thickness(0, 12, 0, 0),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        };
        saveBtn.SetResourceReference(Button.StyleProperty, "BtnSmallPrimary");

        saveBtn.Click += (s, e) =>
        {
            var settings = new Dictionary<string, string>
            {
                ["clock_format_24"] = ((ComboBoxItem)formatCombo.SelectedItem).Tag.ToString() ?? "true",
                ["clock_show_seconds"] = (secondsToggle.IsChecked == true).ToString().ToLower(),
                ["clock_show_date"] = (dateToggle.IsChecked == true).ToString().ToLower(),
                ["clock_date_format"] = ((ComboBoxItem)dateFormatCombo.SelectedItem).Tag.ToString() ?? "long",
                ["clock_timezone"] = ((ComboBoxItem)tzCombo.SelectedItem).Tag.ToString() ?? "Local"
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

    private static StackPanel CreateRow(string label, string description)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 12)
        };

        // ── ستون چپ: label + description ──
        var leftStack = new StackPanel
        {
            Width = 180,
            VerticalAlignment = VerticalAlignment.Center
        };

        leftStack.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        });

        leftStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 10,
            Opacity = 0.6,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0)
        });

        row.Children.Add(leftStack);

        // ── ستون راست: content (خالی، بعداً پر می‌شه) ──
        var rightStack = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        row.Children.Add(rightStack);

        return row;
    }

    private static StackPanel CreateToggleRow(string label, string description)
    {
        var row = CreateRow(label, description);

        // ── Toggle ──
        var toggle = new CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        ((StackPanel)row.Children[1]).Children.Add(toggle);

        // ★ Tag رو ست کن تا بتونیم بعداً بهش دسترسی داشته باشیم
        row.Tag = toggle;

        return row;
    }
}