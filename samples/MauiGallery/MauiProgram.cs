using Microsoft.Maui.Hosting;

namespace SliverWidgets.MauiGallery;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        return builder.Build();
    }
}
