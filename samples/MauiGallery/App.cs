using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SliverWidgets.MauiGallery;

public sealed class App : Application
{
    public App()
    {
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new MainPage())
        {
            BarBackgroundColor = Color.FromArgb("#111827"),
            BarTextColor = Colors.White
        });
    }
}
