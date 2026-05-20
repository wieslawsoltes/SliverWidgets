namespace SliverWidgets.Core;

public sealed record SliverFixedExtentListOptions(int ItemCount, double ItemExtent, double Spacing = 0d)
{
    public void Validate()
    {
        if (ItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ItemCount));
        }

        SliverMath.ThrowIfNegative(ItemExtent, nameof(ItemExtent));
        SliverMath.ThrowIfNegative(Spacing, nameof(Spacing));
    }
}

public sealed class SliverFixedExtentListLayout : ISliverLayout
{
    public SliverFixedExtentListLayout(SliverFixedExtentListOptions options)
    {
        Options = options;
        Options.Validate();
    }

    public SliverFixedExtentListOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();
        var scrollExtent = GetScrollExtent(Options.ItemCount, Options.ItemExtent, Options.Spacing);
        var slots = BuildLinearSlots(
            Options.ItemCount,
            Options.ItemExtent,
            Options.Spacing,
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent,
            constraints.RemainingPaintExtent,
            constraints.CrossAxisExtent);

        return new SliverLayoutResult(
            BuildGeometry(scrollExtent, constraints),
            slots);
    }

    public static double GetScrollExtent(int itemCount, double itemExtent, double spacing)
    {
        return itemCount <= 0 ? 0d : (itemCount * itemExtent) + ((itemCount - 1) * spacing);
    }

    internal static IReadOnlyList<SliverLayoutSlot> BuildLinearSlots(
        int itemCount,
        double itemExtent,
        double spacing,
        double scrollOffset,
        double cacheOrigin,
        double remainingCacheExtent,
        double remainingPaintExtent,
        double crossAxisExtent)
    {
        if (itemCount <= 0)
        {
            return Array.Empty<SliverLayoutSlot>();
        }

        var interval = itemExtent + spacing;
        if (interval <= SliverMath.Epsilon)
        {
            return Array.Empty<SliverLayoutSlot>();
        }

        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(scrollOffset, cacheOrigin, remainingCacheExtent);
        var visibleStart = scrollOffset;
        var visibleEnd = scrollOffset + remainingPaintExtent;
        var startIndex = Math.Max(0, (int)Math.Floor(cacheStart / interval) - 1);
        var slots = new List<SliverLayoutSlot>();

        for (var index = startIndex; index < itemCount; index++)
        {
            var itemStart = index * interval;
            var itemEnd = itemStart + itemExtent;

            if (itemStart - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }

            if (!SliverMath.RangesOverlap(itemStart, itemEnd, cacheStart, cacheEnd))
            {
                continue;
            }

            slots.Add(new SliverLayoutSlot(
                index,
                itemStart - scrollOffset,
                0d,
                itemExtent,
                crossAxisExtent,
                IsCacheOnly: SliverMath.IsCacheOnly(itemStart, itemEnd, visibleStart, visibleEnd)));
        }

        return slots;
    }

    internal static SliverGeometry BuildGeometry(double scrollExtent, in SliverConstraints constraints)
    {
        var paintExtent = SliverMath.ClampPaintExtent(scrollExtent, constraints.ScrollOffset, constraints.RemainingPaintExtent);
        var cacheExtent = SliverMath.Clamp(scrollExtent - constraints.ScrollOffset - constraints.CacheOrigin, 0d, constraints.RemainingCacheExtent);

        return new SliverGeometry
        {
            ScrollExtent = scrollExtent,
            PaintExtent = paintExtent,
            LayoutExtent = paintExtent,
            MaxPaintExtent = scrollExtent,
            HitTestExtent = paintExtent,
            Visible = paintExtent > SliverMath.Epsilon,
            HasVisualOverflow = constraints.ScrollOffset > SliverMath.Epsilon ||
                                scrollExtent - constraints.RemainingPaintExtent > SliverMath.Epsilon,
            CacheExtent = cacheExtent,
            CrossAxisExtent = constraints.CrossAxisExtent
        };
    }
}

public sealed record SliverListOptions(IReadOnlyList<double> ItemExtents, double Spacing = 0d)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(ItemExtents);
        SliverMath.ThrowIfNegative(Spacing, nameof(Spacing));

        for (var i = 0; i < ItemExtents.Count; i++)
        {
            SliverMath.ThrowIfNegative(ItemExtents[i], $"{nameof(ItemExtents)}[{i}]");
        }
    }
}

public sealed class SliverListLayout : ISliverLayout
{
    public SliverListLayout(SliverListOptions options)
    {
        Options = options;
        Options.Validate();
    }

    public SliverListOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var scrollExtent = GetScrollExtent();
        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var slots = new List<SliverLayoutSlot>();
        var cursor = 0d;

        for (var index = 0; index < Options.ItemExtents.Count; index++)
        {
            var extent = Options.ItemExtents[index];
            var itemStart = cursor;
            var itemEnd = itemStart + extent;

            if (SliverMath.RangesOverlap(itemStart, itemEnd, cacheStart, cacheEnd))
            {
                slots.Add(new SliverLayoutSlot(
                    index,
                    itemStart - constraints.ScrollOffset,
                    0d,
                    extent,
                    constraints.CrossAxisExtent,
                    IsCacheOnly: SliverMath.IsCacheOnly(itemStart, itemEnd, visibleStart, visibleEnd)));
            }

            cursor = itemEnd + Options.Spacing;

            if (itemStart - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }
        }

        return new SliverLayoutResult(SliverFixedExtentListLayout.BuildGeometry(scrollExtent, constraints), slots);
    }

    private double GetScrollExtent()
    {
        if (Options.ItemExtents.Count == 0)
        {
            return 0d;
        }

        var scrollExtent = 0d;
        for (var index = 0; index < Options.ItemExtents.Count; index++)
        {
            scrollExtent += Options.ItemExtents[index];
        }

        return scrollExtent + ((Options.ItemExtents.Count - 1) * Options.Spacing);
    }
}

public enum SliverGridSizingMode
{
    FixedCrossAxisCount,
    MaxCrossAxisExtent
}

public sealed record SliverGridLayoutOptions
{
    private SliverGridLayoutOptions(
        int itemCount,
        SliverGridSizingMode sizingMode,
        int crossAxisCount,
        double maxCrossAxisExtent,
        double mainAxisSpacing,
        double crossAxisSpacing,
        double childAspectRatio,
        double? mainAxisExtent)
    {
        ItemCount = itemCount;
        SizingMode = sizingMode;
        CrossAxisCount = crossAxisCount;
        MaxCrossAxisExtent = maxCrossAxisExtent;
        MainAxisSpacing = mainAxisSpacing;
        CrossAxisSpacing = crossAxisSpacing;
        ChildAspectRatio = childAspectRatio;
        MainAxisExtent = mainAxisExtent;
        Validate();
    }

    public int ItemCount { get; }

    public SliverGridSizingMode SizingMode { get; }

    public int CrossAxisCount { get; }

    public double MaxCrossAxisExtent { get; }

    public double MainAxisSpacing { get; }

    public double CrossAxisSpacing { get; }

    public double ChildAspectRatio { get; }

    public double? MainAxisExtent { get; }

    public static SliverGridLayoutOptions FixedCrossAxisCount(
        int itemCount,
        int crossAxisCount,
        double mainAxisSpacing = 0d,
        double crossAxisSpacing = 0d,
        double childAspectRatio = 1d,
        double? mainAxisExtent = null)
    {
        return new SliverGridLayoutOptions(
            itemCount,
            SliverGridSizingMode.FixedCrossAxisCount,
            crossAxisCount,
            0d,
            mainAxisSpacing,
            crossAxisSpacing,
            childAspectRatio,
            mainAxisExtent);
    }

    public static SliverGridLayoutOptions WithMaxCrossAxisExtent(
        int itemCount,
        double maxCrossAxisExtent,
        double mainAxisSpacing = 0d,
        double crossAxisSpacing = 0d,
        double childAspectRatio = 1d,
        double? mainAxisExtent = null)
    {
        return new SliverGridLayoutOptions(
            itemCount,
            SliverGridSizingMode.MaxCrossAxisExtent,
            0,
            maxCrossAxisExtent,
            mainAxisSpacing,
            crossAxisSpacing,
            childAspectRatio,
            mainAxisExtent);
    }

    public void Validate()
    {
        if (ItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ItemCount));
        }

        if (SizingMode == SliverGridSizingMode.FixedCrossAxisCount && CrossAxisCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CrossAxisCount));
        }

        if (SizingMode == SliverGridSizingMode.MaxCrossAxisExtent && MaxCrossAxisExtent <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxCrossAxisExtent));
        }

        SliverMath.ThrowIfNegative(MainAxisSpacing, nameof(MainAxisSpacing));
        SliverMath.ThrowIfNegative(CrossAxisSpacing, nameof(CrossAxisSpacing));

        if (ChildAspectRatio <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(ChildAspectRatio));
        }

        if (MainAxisExtent.HasValue)
        {
            SliverMath.ThrowIfNegative(MainAxisExtent.Value, nameof(MainAxisExtent));
        }
    }
}

public sealed class SliverGridLayout : ISliverLayout
{
    public SliverGridLayout(SliverGridLayoutOptions options)
    {
        Options = options;
    }

    public SliverGridLayoutOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        if (Options.ItemCount == 0)
        {
            return SliverLayoutResult.Empty with
            {
                Geometry = SliverGeometry.Zero with { CrossAxisExtent = constraints.CrossAxisExtent }
            };
        }

        var crossAxisCount = ResolveCrossAxisCount(constraints.CrossAxisExtent);
        var tileCrossAxisExtent = Math.Max(
            0d,
            (constraints.CrossAxisExtent - ((crossAxisCount - 1) * Options.CrossAxisSpacing)) / crossAxisCount);
        var tileMainAxisExtent = Options.MainAxisExtent ?? tileCrossAxisExtent / Options.ChildAspectRatio;
        var rowCount = (int)Math.Ceiling((double)Options.ItemCount / crossAxisCount);
        var scrollExtent = SliverFixedExtentListLayout.GetScrollExtent(rowCount, tileMainAxisExtent, Options.MainAxisSpacing);
        var rowInterval = tileMainAxisExtent + Options.MainAxisSpacing;

        if (rowInterval <= SliverMath.Epsilon)
        {
            return new SliverLayoutResult(
                SliverFixedExtentListLayout.BuildGeometry(scrollExtent, constraints),
                Array.Empty<SliverLayoutSlot>());
        }

        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var startRow = Math.Max(0, (int)Math.Floor(cacheStart / rowInterval) - 1);
        var slots = new List<SliverLayoutSlot>();

        for (var row = startRow; row < rowCount; row++)
        {
            var rowStart = row * rowInterval;
            var rowEnd = rowStart + tileMainAxisExtent;

            if (rowStart - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }

            if (!SliverMath.RangesOverlap(rowStart, rowEnd, cacheStart, cacheEnd))
            {
                continue;
            }

            for (var column = 0; column < crossAxisCount; column++)
            {
                var index = (row * crossAxisCount) + column;
                if (index >= Options.ItemCount)
                {
                    break;
                }

                slots.Add(new SliverLayoutSlot(
                    index,
                    rowStart - constraints.ScrollOffset,
                    column * (tileCrossAxisExtent + Options.CrossAxisSpacing),
                    tileMainAxisExtent,
                    tileCrossAxisExtent,
                    IsCacheOnly: SliverMath.IsCacheOnly(rowStart, rowEnd, visibleStart, visibleEnd)));
            }
        }

        return new SliverLayoutResult(SliverFixedExtentListLayout.BuildGeometry(scrollExtent, constraints), slots);
    }

    public int ResolveCrossAxisCount(double crossAxisExtent)
    {
        if (Options.SizingMode == SliverGridSizingMode.FixedCrossAxisCount)
        {
            return Options.CrossAxisCount;
        }

        var candidate = (int)Math.Floor((crossAxisExtent + Options.CrossAxisSpacing) / (Options.MaxCrossAxisExtent + Options.CrossAxisSpacing));
        return Math.Max(1, candidate);
    }
}

public sealed record SliverPersistentHeaderOptions(double MinExtent, double MaxExtent, bool Pinned = false)
{
    public void Validate()
    {
        SliverMath.ThrowIfNegative(MinExtent, nameof(MinExtent));
        SliverMath.ThrowIfNegative(MaxExtent, nameof(MaxExtent));

        if (MaxExtent < MinExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxExtent), "MaxExtent must be greater than or equal to MinExtent.");
        }
    }
}

public sealed class SliverPersistentHeaderLayout : ISliverLayout
{
    public SliverPersistentHeaderLayout(SliverPersistentHeaderOptions options)
    {
        Options = options;
        Options.Validate();
    }

    public SliverPersistentHeaderOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var shrinkOffset = SliverMath.Clamp(constraints.ScrollOffset, 0d, Options.MaxExtent - Options.MinExtent);
        var currentExtent = SliverMath.Clamp(Options.MaxExtent - shrinkOffset, Options.MinExtent, Options.MaxExtent);
        var remainingNaturalPaint = SliverMath.ClampPaintExtent(Options.MaxExtent, constraints.ScrollOffset, constraints.RemainingPaintExtent);
        var paintExtent = Options.Pinned
            ? Math.Min(currentExtent, constraints.RemainingPaintExtent)
            : remainingNaturalPaint;
        var mainAxisOffset = Options.Pinned && constraints.ScrollOffset > Options.MaxExtent - Options.MinExtent
            ? 0d
            : -constraints.ScrollOffset;

        var geometry = new SliverGeometry
        {
            ScrollExtent = Options.MaxExtent,
            PaintExtent = paintExtent,
            LayoutExtent = paintExtent,
            MaxPaintExtent = Options.MaxExtent,
            MaxScrollObstructionExtent = Options.Pinned ? Options.MinExtent : 0d,
            HitTestExtent = paintExtent,
            Visible = paintExtent > SliverMath.Epsilon,
            HasVisualOverflow = Options.MaxExtent > constraints.RemainingPaintExtent ||
                                constraints.ScrollOffset > SliverMath.Epsilon,
            CacheExtent = SliverMath.ClampPaintExtent(Options.MaxExtent, constraints.ScrollOffset + constraints.CacheOrigin, constraints.RemainingCacheExtent),
            CrossAxisExtent = constraints.CrossAxisExtent
        };

        var slots = geometry.Visible
            ? new[]
            {
                new SliverLayoutSlot(
                    0,
                    mainAxisOffset,
                    0d,
                    currentExtent,
                    constraints.CrossAxisExtent,
                    IsPinned: Options.Pinned)
            }
            : Array.Empty<SliverLayoutSlot>();

        return new SliverLayoutResult(geometry, slots);
    }
}

public sealed record SliverFillRemainingOptions(double? ChildExtent = null, bool HasScrollBody = true)
{
    public void Validate()
    {
        if (ChildExtent.HasValue)
        {
            SliverMath.ThrowIfNegative(ChildExtent.Value, nameof(ChildExtent));
        }
    }
}

public sealed class SliverFillRemainingLayout : ISliverLayout
{
    public SliverFillRemainingLayout(SliverFillRemainingOptions options)
    {
        Options = options;
        Options.Validate();
    }

    public SliverFillRemainingOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();
        var remainingViewport = Math.Max(0d, constraints.ViewportMainAxisExtent - constraints.PrecedingScrollExtent);
        var naturalExtent = Options.ChildExtent ?? remainingViewport;
        var extent = Options.HasScrollBody
            ? Math.Max(remainingViewport, naturalExtent)
            : (constraints.PrecedingScrollExtent <= constraints.ViewportMainAxisExtent
                ? Math.Max(remainingViewport, naturalExtent)
                : naturalExtent);

        var geometry = SliverFixedExtentListLayout.BuildGeometry(extent, constraints);
        var slots = geometry.Visible
            ? new[]
            {
                new SliverLayoutSlot(0, -constraints.ScrollOffset, 0d, extent, constraints.CrossAxisExtent)
            }
            : Array.Empty<SliverLayoutSlot>();

        return new SliverLayoutResult(geometry, slots);
    }
}

public readonly record struct SliverEdgeInsets(double Before, double After, double CrossBefore = 0d, double CrossAfter = 0d)
{
    public void Validate()
    {
        SliverMath.ThrowIfNegative(Before, nameof(Before));
        SliverMath.ThrowIfNegative(After, nameof(After));
        SliverMath.ThrowIfNegative(CrossBefore, nameof(CrossBefore));
        SliverMath.ThrowIfNegative(CrossAfter, nameof(CrossAfter));
    }
}

public sealed class SliverPaddingLayout : ISliverLayout
{
    public SliverPaddingLayout(SliverEdgeInsets padding, ISliverLayout child)
    {
        Padding = padding;
        Padding.Validate();
        Child = child ?? throw new ArgumentNullException(nameof(child));
    }

    public SliverEdgeInsets Padding { get; }

    public ISliverLayout Child { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var childScrollOffset = Math.Max(0d, constraints.ScrollOffset - Padding.Before);
        var childCacheOrigin = constraints.ScrollOffset +
                               constraints.CacheOrigin -
                               Padding.Before -
                               childScrollOffset;
        var childConstraints = constraints with
        {
            ScrollOffset = childScrollOffset,
            PrecedingScrollExtent = constraints.PrecedingScrollExtent + Padding.Before,
            CrossAxisExtent = Math.Max(0d, constraints.CrossAxisExtent - Padding.CrossBefore - Padding.CrossAfter),
            RemainingPaintExtent = Math.Max(0d, constraints.RemainingPaintExtent - Math.Max(0d, Padding.Before - constraints.ScrollOffset)),
            CacheOrigin = childCacheOrigin,
            RemainingCacheExtent = constraints.RemainingCacheExtent
        };

        var childResult = Child.Layout(childConstraints);
        var scrollExtent = Padding.Before + childResult.Geometry.ScrollExtent + Padding.After;
        var geometry = SliverFixedExtentListLayout.BuildGeometry(scrollExtent, constraints) with
        {
            MaxPaintExtent = Padding.Before + childResult.Geometry.MaxPaintExtent + Padding.After,
            CrossAxisExtent = constraints.CrossAxisExtent
        };
        var leadingOffset = Math.Max(0d, Padding.Before - constraints.ScrollOffset);
        var slots = childResult.Slots
            .Select(slot => slot with
            {
                MainAxisOffset = slot.MainAxisOffset + leadingOffset,
                CrossAxisOffset = slot.CrossAxisOffset + Padding.CrossBefore
            })
            .ToArray();

        return new SliverLayoutResult(geometry, slots);
    }
}

public sealed class SliverVisibilityLayout : ISliverLayout
{
    public SliverVisibilityLayout(bool isVisible, ISliverLayout child, ISliverLayout? replacement = null, bool maintainSize = false)
    {
        IsVisible = isVisible;
        Child = child ?? throw new ArgumentNullException(nameof(child));
        Replacement = replacement;
        MaintainSize = maintainSize;
    }

    public bool IsVisible { get; }

    public ISliverLayout Child { get; }

    public ISliverLayout? Replacement { get; }

    public bool MaintainSize { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        if (IsVisible)
        {
            return Child.Layout(constraints);
        }

        if (MaintainSize)
        {
            return Child.Layout(constraints) with { Slots = Array.Empty<SliverLayoutSlot>() };
        }

        return Replacement?.Layout(constraints) ?? SliverLayoutResult.Empty;
    }
}
