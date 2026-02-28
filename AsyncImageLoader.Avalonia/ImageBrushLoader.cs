using System;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AsyncImageLoader;

public static class ImageBrushLoader {
    private static readonly ParametrizedLogger? Logger;
    public static IAsyncImageLoader AsyncImageLoader { get; set; } = new RamCachedWebImageLoader();

    static ImageBrushLoader() {
        SourceProperty.Changed.AddClassHandler<ImageBrush>(OnSourceChanged);
        Logger = Avalonia.Logging.Logger.TryGet(LogEventLevel.Error, ImageLoader.AsyncImageLoaderLogArea);
    }

    private static async void OnSourceChanged(ImageBrush imageBrush, AvaloniaPropertyChangedEventArgs args) {
        var (oldValue, newValue) = args.GetOldAndNewValue<string?>();
        if (oldValue == newValue)
            return;

        SetIsLoading(imageBrush, true);

        Bitmap? bitmap = null;
        try {
            if (!string.IsNullOrWhiteSpace(newValue)) {
                bitmap = await AsyncImageLoader.ProvideImageAsync(newValue!);
            } else {
                var fallback = GetFallbackImage(imageBrush);
                if (fallback != null) {
                    bitmap = fallback!;
                    imageBrush.Source = bitmap;
                    SetIsLoading(imageBrush, false);
                    return;
                }
            }
        }
        catch (Exception e) {
            Logger?.Log("ImageBrushLoader", "ImageBrushLoader image resolution failed: {0}", e);
        }

        if (GetSource(imageBrush) != newValue) return;
        imageBrush.Source = bitmap;

        SetIsLoading(imageBrush, false);
    }

    public static readonly AttachedProperty<string?> SourceProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, string?>("Source", typeof(ImageLoader));

    public static readonly AttachedProperty<Bitmap?> FallbackImageProperty = 
        AvaloniaProperty.RegisterAttached<ImageBrush, Bitmap?>("FallbackImage", typeof(Bitmap));

    public static Bitmap? GetFallbackImage(ImageBrush element) {
        return element.GetValue(FallbackImageProperty);
    }

    public static void SetFallbackImage(ImageBrush element, Bitmap? value) {
        element.SetValue(FallbackImageProperty, value);
    }

    public static string? GetSource(ImageBrush element) {
        return element.GetValue(SourceProperty);
    }

    public static void SetSource(ImageBrush element, string? value) {
        element.SetValue(SourceProperty, value);
    }

    public static readonly AttachedProperty<bool> IsLoadingProperty =
        AvaloniaProperty.RegisterAttached<ImageBrush, bool>("IsLoading", typeof(ImageLoader));

    public static bool GetIsLoading(ImageBrush element) {
        return element.GetValue(IsLoadingProperty);
    }

    private static void SetIsLoading(ImageBrush element, bool value) {
        element.SetValue(IsLoadingProperty, value);
    }
}