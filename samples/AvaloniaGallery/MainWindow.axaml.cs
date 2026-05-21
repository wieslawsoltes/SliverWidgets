using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaGallery;

public sealed partial class MainWindow : Window
{
    private ScrollViewer? _dataGridHeaderScrollViewer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDataGridBodyScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer bodyScrollViewer)
        {
            return;
        }

        _dataGridHeaderScrollViewer ??= this.FindControl<ScrollViewer>("DataGridHeaderScrollViewer");
        if (_dataGridHeaderScrollViewer is null)
        {
            return;
        }

        var nextOffset = _dataGridHeaderScrollViewer.Offset.WithX(bodyScrollViewer.Offset.X).WithY(0d);
        if (_dataGridHeaderScrollViewer.Offset != nextOffset)
        {
            _dataGridHeaderScrollViewer.Offset = nextOffset;
        }
    }
}
