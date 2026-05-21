using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using SliverWidgets.Core;

namespace AvaloniaGallery;

public sealed class MixedSliverPreviewPanel : Panel, ILogicalScrollable
{
    private const int HeaderChildIndex = 0;
    private const int FixedChildStart = 1;
    private const int FixedChildCount = 5;
    private const int GridChildStart = FixedChildStart + FixedChildCount;
    private const int GridChildCount = 12;
    private const int FillChildIndex = GridChildStart + GridChildCount;
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<MixedSliverPreviewPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<MixedSliverPreviewPanel, double>(nameof(CacheExtent), 280d);

    static MixedSliverPreviewPanel()
    {
        AffectsMeasure<MixedSliverPreviewPanel>(ScrollOffsetProperty, CacheExtentProperty);
        AffectsArrange<MixedSliverPreviewPanel>(ScrollOffsetProperty, CacheExtentProperty);
    }

    public MixedSliverPreviewPanel()
    {
        ClipToBounds = true;
    }

    public event EventHandler? ScrollInvalidated;

    public double ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    public double CacheExtent
    {
        get => GetValue(CacheExtentProperty);
        set => SetValue(CacheExtentProperty, value);
    }

    public bool CanHorizontallyScroll
    {
        get => _canHorizontallyScroll;
        set
        {
            if (_canHorizontallyScroll == value)
            {
                return;
            }

            _canHorizontallyScroll = value;
            InvalidateMeasure();
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    public bool CanVerticallyScroll
    {
        get => _canVerticallyScroll;
        set
        {
            if (_canVerticallyScroll == value)
            {
                return;
            }

            _canVerticallyScroll = value;
            InvalidateMeasure();
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    public bool IsLogicalScrollEnabled => true;

    public Size ScrollSize => new(16d, 16d);

    public Size PageScrollSize => _viewport;

    public Size Extent => _extent;

    public Vector Offset
    {
        get => new(0d, ScrollOffset);
        set => SetMainOffset(value.Y);
    }

    public Size Viewport => _viewport;

    protected override Size MeasureOverride(Size availableSize)
    {
        var viewport = CreateViewport(availableSize);
        var result = LayoutSlivers(viewport);
        var measured = new bool[Children.Count];
        UpdateScrollInfo(new Size(viewport.CrossAxisExtent, result.ScrollExtent), new Size(viewport.CrossAxisExtent, viewport.MainAxisExtent));

        foreach (var slot in result.Slots)
        {
            var childIndex = GetChildIndex(slot);
            if (childIndex < 0 || childIndex >= Children.Count)
            {
                continue;
            }

            Children[childIndex].Measure(new Size(slot.CrossAxisExtent, slot.MainAxisExtent));
            measured[childIndex] = true;
        }

        for (var i = 0; i < Children.Count; i++)
        {
            if (!measured[i])
            {
                Children[i].Measure(default);
            }
        }

        return new Size(viewport.CrossAxisExtent, viewport.MainAxisExtent);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var viewport = new SliverViewport(finalSize.Height, finalSize.Width);
        var result = LayoutSlivers(viewport);
        var arranged = new bool[Children.Count];
        var pinnedObstructionExtent = GetPinnedObstructionExtent(result.Slots);
        UpdateScrollInfo(new Size(viewport.CrossAxisExtent, result.ScrollExtent), new Size(viewport.CrossAxisExtent, viewport.MainAxisExtent));

        foreach (var slot in result.Slots)
        {
            var childIndex = GetChildIndex(slot);
            if (childIndex < 0 || childIndex >= Children.Count)
            {
                continue;
            }

            var child = Children[childIndex];
            child.SetValue(ZIndexProperty, slot.IsPinned ? 10 : 0);
            if (slot.IsCacheOnly ||
                !TryGetVisibleClip(
                    slot,
                    pinnedObstructionExtent,
                    viewport.MainAxisExtent,
                    out var visibleClip))
            {
                HideArrangedChild(child);
            }
            else
            {
                child.Opacity = 1d;
                child.IsHitTestVisible = true;
                child.Arrange(new Rect(slot.CrossAxisOffset, slot.MainAxisOffset, slot.CrossAxisExtent, slot.MainAxisExtent));
                ApplyVisibleClip(child, slot, visibleClip);
            }

            arranged[childIndex] = true;
        }

        for (var i = 0; i < Children.Count; i++)
        {
            if (!arranged[i])
            {
                HideArrangedChild(Children[i]);
            }
        }

        return finalSize;
    }

    public bool BringIntoView(Control target, Rect targetRect)
    {
        var index = Children.IndexOf(target);
        if (index < 0)
        {
            return false;
        }

        var result = LayoutSlivers(new SliverViewport(Math.Max(1d, _viewport.Height), Math.Max(1d, _viewport.Width)));
        var slot = result.Slots.FirstOrDefault(candidate => GetChildIndex(candidate) == index);
        if (slot == default)
        {
            return false;
        }

        return BringRangeIntoView(ScrollOffset + slot.MainAxisOffset, ScrollOffset + slot.MainAxisOffset + slot.MainAxisExtent);
    }

    public Control? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        if (Children.Count == 0)
        {
            return null;
        }

        var currentIndex = from is null ? -1 : Children.IndexOf(from);
        var targetIndex = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Children.Count - 1,
            NavigationDirection.Next or NavigationDirection.Down or NavigationDirection.Right => currentIndex + 1,
            NavigationDirection.Previous or NavigationDirection.Up or NavigationDirection.Left => currentIndex - 1,
            _ => currentIndex
        };

        if (targetIndex < 0 || targetIndex >= Children.Count)
        {
            return null;
        }

        var target = Children[targetIndex];
        BringIntoView(target, target.Bounds);
        return target;
    }

    public void RaiseScrollInvalidated(EventArgs e)
    {
        ScrollInvalidated?.Invoke(this, e);
    }

    private SliverViewportLayoutResult LayoutSlivers(SliverViewport viewport)
    {
        var slivers = new ISliverLayout[]
        {
            new SliverAdvancedPersistentHeaderLayout(new SliverAdvancedPersistentHeaderOptions(56d, 152d, Pinned: true)),
            new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(FixedChildCount, 62d, 8d)),
            new SliverPaddingLayout(
                new SliverEdgeInsets(14d, 14d),
                new SliverGridLayout(SliverGridLayoutOptions.FixedCrossAxisCount(GridChildCount, 3, 10d, 10d, mainAxisExtent: 96d))),
            new SliverFillRemainingLayout(new SliverFillRemainingOptions(180d, HasScrollBody: false))
        };

        return new SliverViewportLayoutEngine().Layout(
            slivers,
            viewport,
            Math.Max(0d, ScrollOffset),
            Math.Max(0d, CacheExtent));
    }

    private static SliverViewport CreateViewport(Size size)
    {
        return new SliverViewport(
            double.IsFinite(size.Height) ? Math.Max(0d, size.Height) : 520d,
            double.IsFinite(size.Width) ? Math.Max(0d, size.Width) : 760d);
    }

    private static int GetChildIndex(SliverViewportSlot slot)
    {
        return slot.SliverIndex switch
        {
            0 => HeaderChildIndex,
            1 => FixedChildStart + slot.ItemIndex,
            2 => GridChildStart + slot.ItemIndex,
            3 => FillChildIndex,
            _ => -1
        };
    }

    private bool BringRangeIntoView(double start, double end)
    {
        var current = ScrollOffset;
        var target = current;

        if (start < current)
        {
            target = start;
        }
        else if (end > current + _viewport.Height)
        {
            target = end - _viewport.Height;
        }

        return SetMainOffset(target);
    }

    private bool SetMainOffset(double value)
    {
        var clamped = ClampScrollOffset(value, _extent.Height, _viewport.Height);
        if (AreClose(clamped, ScrollOffset))
        {
            return false;
        }

        SetCurrentValue(ScrollOffsetProperty, clamped);
        InvalidateMeasure();
        RaiseScrollInvalidated(EventArgs.Empty);
        return true;
    }

    private void UpdateScrollInfo(Size extent, Size viewport)
    {
        var changed = !AreClose(_extent, extent) ||
                      !AreClose(_viewport, viewport);
        _extent = extent;
        _viewport = viewport;

        var clamped = ClampScrollOffset(ScrollOffset, extent.Height, viewport.Height);
        if (!AreClose(clamped, ScrollOffset))
        {
            SetCurrentValue(ScrollOffsetProperty, clamped);
            changed = true;
        }

        if (changed)
        {
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    private static double GetPinnedObstructionExtent(IReadOnlyList<SliverViewportSlot> slots)
    {
        var obstructionExtent = 0d;
        foreach (var slot in slots)
        {
            if (!slot.IsPinned || slot.IsCacheOnly)
            {
                continue;
            }

            if (slot.MainAxisOffset - obstructionExtent > SliverMath.Epsilon)
            {
                continue;
            }

            obstructionExtent = Math.Max(obstructionExtent, slot.MainAxisOffset + slot.MainAxisExtent);
        }

        return obstructionExtent;
    }

    private static bool TryGetVisibleClip(
        SliverViewportSlot slot,
        double pinnedObstructionExtent,
        double viewportMainAxisExtent,
        out Rect visibleClip)
    {
        var clipStartBoundary = slot.IsPinned ? 0d : pinnedObstructionExtent;
        var clipStart = Math.Max(0d, clipStartBoundary - slot.MainAxisOffset);
        var clipEnd = Math.Min(slot.MainAxisExtent, viewportMainAxisExtent - slot.MainAxisOffset);
        if (clipEnd - clipStart <= SliverMath.Epsilon)
        {
            visibleClip = default;
            return false;
        }

        visibleClip = new Rect(0d, clipStart, slot.CrossAxisExtent, clipEnd - clipStart);
        return true;
    }

    private static void ApplyVisibleClip(Control child, SliverViewportSlot slot, Rect visibleClip)
    {
        if (visibleClip.Y <= SliverMath.Epsilon &&
            slot.MainAxisExtent - visibleClip.Bottom <= SliverMath.Epsilon)
        {
            child.Clip = null;
            return;
        }

        child.Clip = new RectangleGeometry(visibleClip);
    }

    private static void HideArrangedChild(Control child)
    {
        child.Opacity = 0d;
        child.IsHitTestVisible = false;
        child.Clip = null;
        child.Arrange(default);
    }

    private static double ClampScrollOffset(double offset, double extent, double viewport)
    {
        return SliverMath.Clamp(offset, 0d, Math.Max(0d, extent - viewport));
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) <= SliverMath.Epsilon;
    }

    private static bool AreClose(Size left, Size right)
    {
        return AreClose(left.Width, right.Width) && AreClose(left.Height, right.Height);
    }
}
