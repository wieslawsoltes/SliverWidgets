using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using SliverWidgets.Core;

namespace AvaloniaGallery;

public sealed class SliverScenarioStackPanel : Panel, ILogicalScrollable
{
    private const double HeaderMinExtent = 42d;
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverScenarioStackPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverScenarioStackPanel, double>(nameof(CacheExtent), 280d);

    public static readonly StyledProperty<bool> ShowVisibilitySliverProperty =
        AvaloniaProperty.Register<SliverScenarioStackPanel, bool>(nameof(ShowVisibilitySliver), true);

    public static readonly StyledProperty<bool> MaintainVisibilitySliverSizeProperty =
        AvaloniaProperty.Register<SliverScenarioStackPanel, bool>(nameof(MaintainVisibilitySliverSize));

    static SliverScenarioStackPanel()
    {
        AffectsMeasure<SliverScenarioStackPanel>(
            ScrollOffsetProperty,
            CacheExtentProperty,
            ShowVisibilitySliverProperty,
            MaintainVisibilitySliverSizeProperty);
        AffectsArrange<SliverScenarioStackPanel>(
            ScrollOffsetProperty,
            CacheExtentProperty,
            ShowVisibilitySliverProperty,
            MaintainVisibilitySliverSizeProperty);
    }

    public SliverScenarioStackPanel()
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

    public bool ShowVisibilitySliver
    {
        get => GetValue(ShowVisibilitySliverProperty);
        set => SetValue(ShowVisibilitySliverProperty, value);
    }

    public bool MaintainVisibilitySliverSize
    {
        get => GetValue(MaintainVisibilitySliverSizeProperty);
        set => SetValue(MaintainVisibilitySliverSizeProperty, value);
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
        var extent = new Size(viewport.CrossAxisExtent, result.ScrollExtent);
        var measuredViewport = new Size(viewport.CrossAxisExtent, viewport.MainAxisExtent);
        var stickyHeader = GetActiveStickyHeader();

        if (!CanVerticallyScroll)
        {
            result = LayoutSlivers(new SliverViewport(Math.Max(viewport.MainAxisExtent, result.ScrollExtent), viewport.CrossAxisExtent));
        }

        var measured = new bool[Children.Count];
        UpdateScrollInfo(extent, measuredViewport);

        foreach (var slot in result.Slots)
        {
            var childIndex = slot.SliverIndex;
            if (childIndex < 0 || childIndex >= Children.Count)
            {
                continue;
            }

            Children[childIndex].Measure(new Size(slot.CrossAxisExtent, slot.MainAxisExtent));
            measured[childIndex] = true;
        }

        if (stickyHeader.IsVisible && stickyHeader.ChildIndex < Children.Count)
        {
            Children[stickyHeader.ChildIndex].Measure(new Size(viewport.CrossAxisExtent, stickyHeader.MainAxisExtent));
            measured[stickyHeader.ChildIndex] = true;
        }

        for (var i = 0; i < Children.Count; i++)
        {
            if (!measured[i])
            {
                Children[i].Measure(default);
            }
        }

        return CanVerticallyScroll ? measuredViewport : extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var viewport = new SliverViewport(finalSize.Height, finalSize.Width);
        var result = LayoutSlivers(viewport);
        var arranged = new bool[Children.Count];
        var stickyHeader = GetActiveStickyHeader();
        var pinnedObstructionExtent = stickyHeader.IsVisible
            ? Math.Max(0d, stickyHeader.MainAxisOffset + stickyHeader.MainAxisExtent)
            : GetPinnedObstructionExtent(result.Slots);
        UpdateScrollInfo(new Size(viewport.CrossAxisExtent, result.ScrollExtent), new Size(viewport.CrossAxisExtent, viewport.MainAxisExtent));

        foreach (var slot in result.Slots)
        {
            var childIndex = slot.SliverIndex;
            if (childIndex < 0 || childIndex >= Children.Count)
            {
                continue;
            }

            if (stickyHeader.IsVisible && childIndex == stickyHeader.ChildIndex)
            {
                continue;
            }

            var child = Children[childIndex];
            child.SetValue(ZIndexProperty, slot.IsPinned ? 20 : 0);
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

        if (stickyHeader.IsVisible && stickyHeader.ChildIndex < Children.Count)
        {
            var child = Children[stickyHeader.ChildIndex];
            child.SetValue(ZIndexProperty, 20);
            child.Opacity = 1d;
            child.IsHitTestVisible = true;
            child.Clip = null;
            child.Arrange(new Rect(0d, stickyHeader.MainAxisOffset, viewport.CrossAxisExtent, stickyHeader.MainAxisExtent));
            arranged[stickyHeader.ChildIndex] = true;
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
        var slot = result.Slots.FirstOrDefault(candidate => candidate.SliverIndex == index);
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
        var slivers = Children
            .Select(child => CreateLayout(child.DataContext as AvaloniaSliverBlock))
            .ToArray();

        return new SliverViewportLayoutEngine().Layout(
            slivers,
            viewport,
            Math.Max(0d, ScrollOffset),
            Math.Max(0d, CacheExtent));
    }

    private ISliverLayout CreateLayout(AvaloniaSliverBlock? block)
    {
        if (block is null)
        {
            return new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(48d));
        }

        var extent = Math.Max(0d, block.Extent);
        return block.Kind switch
        {
            AvaloniaSliverBlockKind.Header => new SliverToBoxAdapterLayout(
                new SliverToBoxAdapterOptions(Math.Max(HeaderMinExtent, extent))),
            AvaloniaSliverBlockKind.PaddedBox => new SliverPaddingLayout(
                new SliverEdgeInsets(18d, 18d, 18d, 18d),
                new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(extent))),
            AvaloniaSliverBlockKind.FillRemaining => new SliverFillRemainingLayout(
                new SliverFillRemainingOptions(extent, HasScrollBody: false)),
            AvaloniaSliverBlockKind.Visibility => new SliverVisibilityLayout(
                ShowVisibilitySliver,
                new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(extent)),
                maintainSize: MaintainVisibilitySliverSize),
            _ => new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(extent))
        };
    }

    private static SliverViewport CreateViewport(Size size)
    {
        return new SliverViewport(
            double.IsFinite(size.Height) ? Math.Max(0d, size.Height) : 520d,
            double.IsFinite(size.Width) ? Math.Max(0d, size.Width) : 760d);
    }

    private StickyHeaderState GetActiveStickyHeader()
    {
        var scrollOffset = Math.Max(0d, ScrollOffset);
        var cursor = 0d;
        var activeChildIndex = -1;
        var activeStart = 0d;
        var activeMaxExtent = 0d;
        double? nextHeaderStart = null;

        for (var childIndex = 0; childIndex < Children.Count; childIndex++)
        {
            var block = Children[childIndex].DataContext as AvaloniaSliverBlock;
            var extent = GetScrollExtent(block);

            if (block?.Kind == AvaloniaSliverBlockKind.Header)
            {
                if (cursor <= scrollOffset + SliverMath.Epsilon)
                {
                    activeChildIndex = childIndex;
                    activeStart = cursor;
                    activeMaxExtent = Math.Max(HeaderMinExtent, block.Extent);
                    nextHeaderStart = null;
                }
                else if (activeChildIndex >= 0)
                {
                    nextHeaderStart = cursor;
                    break;
                }
            }

            cursor += extent;
        }

        if (activeChildIndex < 0)
        {
            return default;
        }

        var shrinkOffset = SliverMath.Clamp(scrollOffset - activeStart, 0d, activeMaxExtent - HeaderMinExtent);
        var currentExtent = SliverMath.Clamp(activeMaxExtent - shrinkOffset, HeaderMinExtent, activeMaxExtent);
        var mainAxisOffset = 0d;
        if (nextHeaderStart.HasValue)
        {
            mainAxisOffset = Math.Min(0d, nextHeaderStart.Value - scrollOffset - currentExtent);
        }

        if (mainAxisOffset + currentExtent <= SliverMath.Epsilon)
        {
            return default;
        }

        return new StickyHeaderState(activeChildIndex, mainAxisOffset, currentExtent);
    }

    private static double GetScrollExtent(AvaloniaSliverBlock? block)
    {
        if (block is null)
        {
            return 48d;
        }

        var extent = Math.Max(0d, block.Extent);
        return block.Kind switch
        {
            AvaloniaSliverBlockKind.Header => Math.Max(HeaderMinExtent, extent),
            AvaloniaSliverBlockKind.PaddedBox => extent + 36d,
            _ => extent
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
            if (slot.IsPinned && !slot.IsCacheOnly)
            {
                obstructionExtent = Math.Max(obstructionExtent, slot.MainAxisOffset + slot.MainAxisExtent);
            }
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

    private readonly record struct StickyHeaderState(int ChildIndex, double MainAxisOffset, double MainAxisExtent)
    {
        public bool IsVisible => ChildIndex >= 0 && MainAxisExtent > SliverMath.Epsilon;
    }
}
