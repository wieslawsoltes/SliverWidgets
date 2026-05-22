using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace SliverWidgets.Avalonia;

/// <summary>
/// ItemsControl wrapper that exposes an ILogicalScrollable items panel to an outer ScrollViewer.
/// </summary>
public class SliverItemsControl : ItemsControl, ILogicalScrollable
{
    private ILogicalScrollable? _logicalScrollable;
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Vector _offset;
    private Size _viewport;

    protected override Type StyleKeyOverride => typeof(ItemsControl);

    public event EventHandler? ScrollInvalidated;

    public bool CanHorizontallyScroll
    {
        get => _logicalScrollable?.CanHorizontallyScroll ?? _canHorizontallyScroll;
        set
        {
            _canHorizontallyScroll = value;
            if (_logicalScrollable is not null)
            {
                _logicalScrollable.CanHorizontallyScroll = value;
            }
        }
    }

    public bool CanVerticallyScroll
    {
        get => _logicalScrollable?.CanVerticallyScroll ?? _canVerticallyScroll;
        set
        {
            _canVerticallyScroll = value;
            if (_logicalScrollable is not null)
            {
                _logicalScrollable.CanVerticallyScroll = value;
            }
        }
    }

    public bool IsLogicalScrollEnabled => _logicalScrollable?.IsLogicalScrollEnabled ?? true;

    public Size ScrollSize => _logicalScrollable?.ScrollSize ?? new Size(16d, 16d);

    public Size PageScrollSize => _logicalScrollable?.PageScrollSize ?? _viewport;

    public Size Extent => _logicalScrollable?.Extent ?? _extent;

    public Vector Offset
    {
        get => _logicalScrollable?.Offset ?? _offset;
        set
        {
            _offset = value;
            if (_logicalScrollable is not null)
            {
                _logicalScrollable.Offset = value;
                SyncFromScrollable(_logicalScrollable);
            }
        }
    }

    public Size Viewport => _logicalScrollable?.Viewport ?? _viewport;

    public bool BringIntoView(Control target, Rect targetRect)
    {
        UpdateLogicalScrollable();
        return _logicalScrollable?.BringIntoView(target, targetRect) ?? false;
    }

    public Control? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        UpdateLogicalScrollable();
        return _logicalScrollable?.GetControlInDirection(direction, from);
    }

    public void RaiseScrollInvalidated(EventArgs e)
    {
        ScrollInvalidated?.Invoke(this, e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateLogicalScrollable();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LayoutUpdated += OnLayoutUpdated;
        UpdateLogicalScrollable();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        LayoutUpdated -= OnLayoutUpdated;
        SetLogicalScrollable(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateLogicalScrollable();
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateLogicalScrollable();
        return base.ArrangeOverride(finalSize);
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        UpdateLogicalScrollable();
    }

    private void UpdateLogicalScrollable()
    {
        SetLogicalScrollable(Presenter?.Panel as ILogicalScrollable);
    }

    private void SetLogicalScrollable(ILogicalScrollable? logicalScrollable)
    {
        if (ReferenceEquals(_logicalScrollable, logicalScrollable))
        {
            return;
        }

        if (_logicalScrollable is not null)
        {
            _logicalScrollable.ScrollInvalidated -= OnLogicalScrollInvalidated;
        }

        _logicalScrollable = logicalScrollable;

        if (_logicalScrollable is not null)
        {
            _logicalScrollable.ScrollInvalidated += OnLogicalScrollInvalidated;
            _logicalScrollable.CanHorizontallyScroll = _canHorizontallyScroll;
            _logicalScrollable.CanVerticallyScroll = _canVerticallyScroll;
            _logicalScrollable.Offset = _offset;
            SyncFromScrollable(_logicalScrollable);
        }

        RaiseScrollInvalidated(EventArgs.Empty);
    }

    private void OnLogicalScrollInvalidated(object? sender, EventArgs e)
    {
        if (sender is ILogicalScrollable logicalScrollable)
        {
            SyncFromScrollable(logicalScrollable);
        }

        RaiseScrollInvalidated(e);
    }

    private void SyncFromScrollable(ILogicalScrollable logicalScrollable)
    {
        _canHorizontallyScroll = logicalScrollable.CanHorizontallyScroll;
        _canVerticallyScroll = logicalScrollable.CanVerticallyScroll;
        _extent = logicalScrollable.Extent;
        _offset = logicalScrollable.Offset;
        _viewport = logicalScrollable.Viewport;
    }
}
