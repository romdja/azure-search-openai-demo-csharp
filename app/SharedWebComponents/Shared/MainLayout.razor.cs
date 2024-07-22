// Copyright (c) Microsoft. All rights reserved.

using MudBlazor.Utilities;

namespace SharedWebComponents.Shared;

public sealed partial class MainLayout
{

    private static MudTheme GetGreenchoiceTheme()
    {
        MudTheme _theme = new MudTheme { Palette = 
    { 
        Primary = Colors.Green.Default, 

        Secondary = new MudColor("#8CC441"),

        SecondaryLighten = "#e2f0cf",
        
        Tertiary = new MudColor("#ffc84a"),
    

        AppbarBackground = new MudColor("#8CC441")}
        };

        return _theme;
    }

    private static MudTheme GetCleanTheme()
    {
        MudTheme _theme = new MudTheme { Palette = 
    { 
        Primary = new MudColor("#F2642E"), 

        Secondary = new MudColor("#8CC441"),

       // SecondaryLighten = "#e2f0cf",
        
        Tertiary = new MudColor("#FFFFFF"),
    

        AppbarBackground = new MudColor("#FFFFFF")}
        };

        return _theme;
    }

     private static MudTheme GetPinkTheme()
    {
        MudTheme _theme = new MudTheme { Palette = 
    { 
        Primary = new MudColor("#FF3F81"), 

        Secondary = new MudColor("#8CC441"),

       // SecondaryLighten = "#e2f0cf",
        
        Tertiary = new MudColor("#FFFFFF"),

        
    

        AppbarBackground = new MudColor("#FFFFFF")}
        };

        return _theme;
    }


    private readonly MudTheme _theme = GetPinkTheme();

    private bool _drawerOpen = true;
    private bool _settingsOpen = false;
    private SettingsPanel? _settingsPanel;

    private bool _isDarkTheme
    {
        get => LocalStorage.GetItem<bool>(StorageKeys.PrefersDarkTheme);
        set => LocalStorage.SetItem<bool>(StorageKeys.PrefersDarkTheme, value);
    }

    private bool _isReversed
    {
        get => LocalStorage.GetItem<bool?>(StorageKeys.PrefersReversedConversationSorting) ?? false;
        set => LocalStorage.SetItem<bool>(StorageKeys.PrefersReversedConversationSorting, value);
    }

    private bool _isRightToLeft =>
        Thread.CurrentThread.CurrentUICulture is { TextInfo.IsRightToLeft: true };

    [Inject] public required NavigationManager Nav { get; set; }
    [Inject] public required ILocalStorageService LocalStorage { get; set; }
    [Inject] public required IDialogService Dialog { get; set; }

    private bool SettingsDisabled => new Uri(Nav.Uri).Segments.LastOrDefault() switch
    {
        "ask" or "chat" => false,
        _ => true
    };

    private bool SortDisabled => new Uri(Nav.Uri).Segments.LastOrDefault() switch
    {
        "voicechat" or "chat" => false,
        _ => true
    };

    private void OnMenuClicked() => _drawerOpen = !_drawerOpen;

    private void OnThemeChanged() => _isDarkTheme = !_isDarkTheme;

    private void OnIsReversedChanged() => _isReversed = !_isReversed;
}
