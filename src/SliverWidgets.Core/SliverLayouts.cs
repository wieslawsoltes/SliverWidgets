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
        ArgumentNullException.ThrowIfNull(options);
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
        var paintExtent = SliverMath.CalculatePaintOffset(constraints, 0d, scrollExtent);
        var cacheExtent = SliverMath.CalculateCacheOffset(constraints, 0d, scrollExtent);

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
        ArgumentNullException.ThrowIfNull(options);
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
        ArgumentNullException.ThrowIfNull(options);
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

        var candidate = (int)Math.Ceiling(crossAxisExtent / (Options.MaxCrossAxisExtent + Options.CrossAxisSpacing));
        return Math.Max(1, candidate);
    }
}

public readonly record struct SliverWrapItemExtent(double MainAxisExtent, double CrossAxisExtent)
{
    public void Validate(string name)
    {
        SliverMath.ThrowIfNegative(MainAxisExtent, $"{name}.{nameof(MainAxisExtent)}");
        SliverMath.ThrowIfNegative(CrossAxisExtent, $"{name}.{nameof(CrossAxisExtent)}");
    }
}

public sealed record SliverWrapLayoutOptions(
    IReadOnlyList<SliverWrapItemExtent> ItemExtents,
    double MainAxisSpacing = 0d,
    double CrossAxisSpacing = 0d)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(ItemExtents);
        SliverMath.ThrowIfNegative(MainAxisSpacing, nameof(MainAxisSpacing));
        SliverMath.ThrowIfNegative(CrossAxisSpacing, nameof(CrossAxisSpacing));

        if (ItemExtents is SliverDeterministicWrapExtentList)
        {
            return;
        }

        for (var index = 0; index < ItemExtents.Count; index++)
        {
            ItemExtents[index].Validate($"{nameof(ItemExtents)}[{index}]");
        }
    }
}

public sealed class SliverDeterministicWrapExtentList : IReadOnlyList<SliverWrapItemExtent>
{
    public SliverDeterministicWrapExtentList(
        int count,
        double minMainAxisExtent = 56d,
        double maxMainAxisExtent = 132d,
        double minCrossAxisExtent = 120d,
        double maxCrossAxisExtent = 280d)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        SliverMath.ThrowIfNegative(minMainAxisExtent, nameof(minMainAxisExtent));
        SliverMath.ThrowIfNegative(maxMainAxisExtent, nameof(maxMainAxisExtent));
        SliverMath.ThrowIfNegative(minCrossAxisExtent, nameof(minCrossAxisExtent));
        SliverMath.ThrowIfNegative(maxCrossAxisExtent, nameof(maxCrossAxisExtent));

        if (maxMainAxisExtent < minMainAxisExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMainAxisExtent), "Maximum main-axis extent must be greater than or equal to the minimum.");
        }

        if (maxCrossAxisExtent < minCrossAxisExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCrossAxisExtent), "Maximum cross-axis extent must be greater than or equal to the minimum.");
        }

        Count = count;
        MinMainAxisExtent = minMainAxisExtent;
        MaxMainAxisExtent = maxMainAxisExtent;
        MinCrossAxisExtent = minCrossAxisExtent;
        MaxCrossAxisExtent = maxCrossAxisExtent;
    }

    public int Count { get; }

    public double MinMainAxisExtent { get; }

    public double MaxMainAxisExtent { get; }

    public double MinCrossAxisExtent { get; }

    public double MaxCrossAxisExtent { get; }

    public SliverWrapItemExtent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var main = Interpolate(MinMainAxisExtent, MaxMainAxisExtent, ((index * 37) + 17) % 101);
            var cross = Interpolate(MinCrossAxisExtent, MaxCrossAxisExtent, ((index * 53) + 29) % 101);
            return new SliverWrapItemExtent(main, cross);
        }
    }

    public IEnumerator<SliverWrapItemExtent> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private static double Interpolate(double minimum, double maximum, int bucket)
    {
        if (Math.Abs(maximum - minimum) <= SliverMath.Epsilon)
        {
            return minimum;
        }

        return minimum + ((maximum - minimum) * bucket / 100d);
    }
}

public sealed class SliverWrapLayout : ISliverLayout
{
    private WrapMetrics? _metrics;
    private double _metricsCrossAxisExtent = double.NaN;

    public SliverWrapLayout(SliverWrapLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        Options.Validate();
    }

    public SliverWrapLayoutOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        if (Options.ItemExtents.Count == 0 || constraints.CrossAxisExtent <= SliverMath.Epsilon)
        {
            return SliverLayoutResult.Empty with
            {
                Geometry = SliverGeometry.Zero with { CrossAxisExtent = constraints.CrossAxisExtent }
            };
        }

        var metrics = ResolveMetrics(constraints.CrossAxisExtent);
        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var startLine = FindFirstLine(metrics.Lines, cacheStart);
        var slots = new List<SliverLayoutSlot>();

        for (var lineIndex = startLine; lineIndex < metrics.Lines.Length; lineIndex++)
        {
            var line = metrics.Lines[lineIndex];
            var lineEnd = line.MainAxisOffset + line.MainAxisExtent;

            if (line.MainAxisOffset - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }

            if (!SliverMath.RangesOverlap(line.MainAxisOffset, lineEnd, cacheStart, cacheEnd))
            {
                continue;
            }

            for (var index = line.StartIndex; index < line.StartIndex + line.Count; index++)
            {
                var item = Options.ItemExtents[index];
                var itemMainExtent = item.MainAxisExtent;
                var itemEnd = line.MainAxisOffset + itemMainExtent;

                if (!SliverMath.RangesOverlap(line.MainAxisOffset, itemEnd, cacheStart, cacheEnd))
                {
                    continue;
                }

                slots.Add(new SliverLayoutSlot(
                    index,
                    line.MainAxisOffset - constraints.ScrollOffset,
                    metrics.CrossAxisOffsets[index],
                    itemMainExtent,
                    metrics.CrossAxisExtents[index],
                    IsCacheOnly: SliverMath.IsCacheOnly(line.MainAxisOffset, itemEnd, visibleStart, visibleEnd)));
            }
        }

        return new SliverLayoutResult(
            SliverFixedExtentListLayout.BuildGeometry(metrics.ScrollExtent, constraints),
            slots);
    }

    public double GetItemMainAxisOffset(int index, double crossAxisExtent)
    {
        if ((uint)index >= (uint)Options.ItemExtents.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (!double.IsFinite(crossAxisExtent) || crossAxisExtent < -SliverMath.Epsilon)
        {
            throw new ArgumentOutOfRangeException(nameof(crossAxisExtent));
        }

        if (crossAxisExtent <= SliverMath.Epsilon)
        {
            return 0d;
        }

        var metrics = ResolveMetrics(crossAxisExtent);
        var lineIndex = metrics.LineIndexes[index];
        return lineIndex >= 0 ? metrics.Lines[lineIndex].MainAxisOffset : 0d;
    }

    private WrapMetrics ResolveMetrics(double crossAxisExtent)
    {
        var normalizedCrossAxisExtent = Math.Max(0d, crossAxisExtent);
        if (_metrics is { } metrics &&
            Math.Abs(_metricsCrossAxisExtent - normalizedCrossAxisExtent) <= SliverMath.Epsilon)
        {
            return metrics;
        }

        metrics = BuildMetrics(normalizedCrossAxisExtent);
        _metrics = metrics;
        _metricsCrossAxisExtent = normalizedCrossAxisExtent;
        return metrics;
    }

    private WrapMetrics BuildMetrics(double crossAxisExtent)
    {
        var itemCount = Options.ItemExtents.Count;
        var lines = new List<WrapLine>();
        var crossOffsets = new double[itemCount];
        var crossExtents = new double[itemCount];
        var lineIndexes = new int[itemCount];
        Array.Fill(lineIndexes, -1);

        var currentLineStart = 0;
        var currentLineCount = 0;
        var currentLineCrossOffset = 0d;
        var currentLineMainExtent = 0d;
        var currentMainOffset = 0d;
        var mainAxisSpacing = Options.MainAxisSpacing;
        var crossAxisSpacing = Options.CrossAxisSpacing;

        for (var index = 0; index < itemCount; index++)
        {
            var item = Options.ItemExtents[index];
            var itemCrossExtent = Math.Min(item.CrossAxisExtent, crossAxisExtent);
            var projectedCrossEnd = currentLineCount == 0
                ? itemCrossExtent
                : currentLineCrossOffset + crossAxisSpacing + itemCrossExtent;

            if (currentLineCount > 0 && projectedCrossEnd - crossAxisExtent > SliverMath.Epsilon)
            {
                FinalizeLine();
            }

            if (currentLineCount == 0)
            {
                currentLineStart = index;
                currentLineCrossOffset = 0d;
            }
            else
            {
                currentLineCrossOffset += crossAxisSpacing;
            }

            crossOffsets[index] = currentLineCrossOffset;
            crossExtents[index] = itemCrossExtent;
            currentLineCrossOffset += itemCrossExtent;
            currentLineMainExtent = Math.Max(currentLineMainExtent, item.MainAxisExtent);
            currentLineCount++;
        }

        if (currentLineCount > 0)
        {
            FinalizeLine();
        }

        return new WrapMetrics(
            currentMainOffset,
            lines.ToArray(),
            crossOffsets,
            crossExtents,
            lineIndexes);

        void FinalizeLine()
        {
            var lineIndex = lines.Count;
            lines.Add(new WrapLine(currentLineStart, currentLineCount, currentMainOffset, currentLineMainExtent));

            for (var itemIndex = currentLineStart; itemIndex < currentLineStart + currentLineCount; itemIndex++)
            {
                lineIndexes[itemIndex] = lineIndex;
            }

            currentMainOffset += currentLineMainExtent;
            if (currentLineStart + currentLineCount < itemCount)
            {
                currentMainOffset += mainAxisSpacing;
            }

            currentLineCount = 0;
            currentLineCrossOffset = 0d;
            currentLineMainExtent = 0d;
        }
    }

    private static int FindFirstLine(IReadOnlyList<WrapLine> lines, double cacheStart)
    {
        var low = 0;
        var high = lines.Count;

        while (low < high)
        {
            var middle = low + ((high - low) / 2);
            var lineEnd = lines[middle].MainAxisOffset + lines[middle].MainAxisExtent;
            if (lineEnd - cacheStart > SliverMath.Epsilon)
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }

        return Math.Max(0, low - 1);
    }

    private readonly record struct WrapLine(int StartIndex, int Count, double MainAxisOffset, double MainAxisExtent);

    private sealed record WrapMetrics(
        double ScrollExtent,
        WrapLine[] Lines,
        double[] CrossAxisOffsets,
        double[] CrossAxisExtents,
        int[] LineIndexes);
}

public enum SliverCrossAxisAlignment
{
    Start,
    Center,
    End,
    Stretch
}

public readonly record struct SliverStackItemExtent(double MainAxisExtent, double CrossAxisExtent)
{
    public void Validate(string name)
    {
        SliverMath.ThrowIfNegative(MainAxisExtent, $"{name}.{nameof(MainAxisExtent)}");
        SliverMath.ThrowIfNegative(CrossAxisExtent, $"{name}.{nameof(CrossAxisExtent)}");
    }
}

public sealed record SliverStackLayoutOptions(
    IReadOnlyList<SliverStackItemExtent> ItemExtents,
    double Spacing = 0d,
    SliverCrossAxisAlignment CrossAxisAlignment = SliverCrossAxisAlignment.Start)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(ItemExtents);
        SliverMath.ThrowIfNegative(Spacing, nameof(Spacing));

        if (ItemExtents is SliverDeterministicStackExtentList)
        {
            return;
        }

        for (var index = 0; index < ItemExtents.Count; index++)
        {
            ItemExtents[index].Validate($"{nameof(ItemExtents)}[{index}]");
        }
    }
}

public sealed class SliverDeterministicStackExtentList : IReadOnlyList<SliverStackItemExtent>
{
    public SliverDeterministicStackExtentList(
        int count,
        double minMainAxisExtent = 52d,
        double maxMainAxisExtent = 128d,
        double minCrossAxisExtent = 160d,
        double maxCrossAxisExtent = 640d)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        SliverMath.ThrowIfNegative(minMainAxisExtent, nameof(minMainAxisExtent));
        SliverMath.ThrowIfNegative(maxMainAxisExtent, nameof(maxMainAxisExtent));
        SliverMath.ThrowIfNegative(minCrossAxisExtent, nameof(minCrossAxisExtent));
        SliverMath.ThrowIfNegative(maxCrossAxisExtent, nameof(maxCrossAxisExtent));

        if (maxMainAxisExtent < minMainAxisExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMainAxisExtent), "Maximum main-axis extent must be greater than or equal to the minimum.");
        }

        if (maxCrossAxisExtent < minCrossAxisExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCrossAxisExtent), "Maximum cross-axis extent must be greater than or equal to the minimum.");
        }

        Count = count;
        MinMainAxisExtent = minMainAxisExtent;
        MaxMainAxisExtent = maxMainAxisExtent;
        MinCrossAxisExtent = minCrossAxisExtent;
        MaxCrossAxisExtent = maxCrossAxisExtent;
    }

    public int Count { get; }

    public double MinMainAxisExtent { get; }

    public double MaxMainAxisExtent { get; }

    public double MinCrossAxisExtent { get; }

    public double MaxCrossAxisExtent { get; }

    public SliverStackItemExtent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var main = Interpolate(MinMainAxisExtent, MaxMainAxisExtent, ((index * 43) + 11) % 101);
            var cross = Interpolate(MinCrossAxisExtent, MaxCrossAxisExtent, ((index * 61) + 23) % 101);
            return new SliverStackItemExtent(main, cross);
        }
    }

    public IEnumerator<SliverStackItemExtent> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    private static double Interpolate(double minimum, double maximum, int bucket)
    {
        if (Math.Abs(maximum - minimum) <= SliverMath.Epsilon)
        {
            return minimum;
        }

        return minimum + ((maximum - minimum) * bucket / 100d);
    }
}

public sealed class SliverStackLayout : ISliverLayout
{
    private StackMetrics? _metrics;

    public SliverStackLayout(SliverStackLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        Options.Validate();
    }

    public SliverStackLayoutOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        if (Options.ItemExtents.Count == 0)
        {
            return SliverLayoutResult.Empty with
            {
                Geometry = SliverGeometry.Zero with { CrossAxisExtent = constraints.CrossAxisExtent }
            };
        }

        var metrics = ResolveMetrics();
        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var startIndex = FindFirstIndex(metrics, cacheStart);
        var slots = new List<SliverLayoutSlot>();

        for (var index = startIndex; index < Options.ItemExtents.Count; index++)
        {
            var item = Options.ItemExtents[index];
            var itemStart = metrics.Offsets[index];
            var itemEnd = itemStart + item.MainAxisExtent;

            if (itemStart - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }

            if (!SliverMath.RangesOverlap(itemStart, itemEnd, cacheStart, cacheEnd))
            {
                continue;
            }

            var (crossAxisOffset, crossAxisExtent) = ResolveCrossAxisSlot(item.CrossAxisExtent, constraints.CrossAxisExtent);
            slots.Add(new SliverLayoutSlot(
                index,
                itemStart - constraints.ScrollOffset,
                crossAxisOffset,
                item.MainAxisExtent,
                crossAxisExtent,
                IsCacheOnly: SliverMath.IsCacheOnly(itemStart, itemEnd, visibleStart, visibleEnd)));
        }

        return new SliverLayoutResult(
            SliverFixedExtentListLayout.BuildGeometry(metrics.ScrollExtent, constraints),
            slots);
    }

    public double GetItemMainAxisOffset(int index)
    {
        if ((uint)index >= (uint)Options.ItemExtents.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return ResolveMetrics().Offsets[index];
    }

    private StackMetrics ResolveMetrics()
    {
        if (_metrics is { } metrics)
        {
            return metrics;
        }

        var itemCount = Options.ItemExtents.Count;
        var offsets = new double[itemCount];
        var cursor = 0d;

        for (var index = 0; index < itemCount; index++)
        {
            offsets[index] = cursor;
            cursor += Options.ItemExtents[index].MainAxisExtent;
            if (index < itemCount - 1)
            {
                cursor += Options.Spacing;
            }
        }

        _metrics = new StackMetrics(offsets, cursor);
        return _metrics;
    }

    private (double Offset, double Extent) ResolveCrossAxisSlot(double itemCrossAxisExtent, double viewportCrossAxisExtent)
    {
        var viewportExtent = Math.Max(0d, viewportCrossAxisExtent);
        var extent = Options.CrossAxisAlignment == SliverCrossAxisAlignment.Stretch
            ? viewportExtent
            : Math.Min(itemCrossAxisExtent, viewportExtent);
        var offset = Options.CrossAxisAlignment switch
        {
            SliverCrossAxisAlignment.Center => Math.Max(0d, (viewportExtent - extent) / 2d),
            SliverCrossAxisAlignment.End => Math.Max(0d, viewportExtent - extent),
            _ => 0d
        };

        return (offset, extent);
    }

    private static int FindFirstIndex(StackMetrics metrics, double cacheStart)
    {
        var low = 0;
        var high = metrics.Offsets.Length;

        while (low < high)
        {
            var middle = low + ((high - low) / 2);
            var itemEnd = middle + 1 < metrics.Offsets.Length
                ? metrics.Offsets[middle + 1]
                : metrics.ScrollExtent;
            if (itemEnd - cacheStart > SliverMath.Epsilon)
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }

        return Math.Max(0, low - 1);
    }

    private sealed record StackMetrics(double[] Offsets, double ScrollExtent);
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
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        Options.Validate();
    }

    public SliverPersistentHeaderOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var shrinkOffset = SliverMath.Clamp(constraints.ScrollOffset, 0d, Options.MaxExtent - Options.MinExtent);
        var currentExtent = SliverMath.Clamp(Options.MaxExtent - shrinkOffset, Options.MinExtent, Options.MaxExtent);
        var remainingNaturalPaint = SliverMath.CalculatePaintOffset(constraints, 0d, Options.MaxExtent);
        var effectiveRemainingPaintExtent = Options.Pinned
            ? Math.Max(0d, constraints.RemainingPaintExtent - constraints.Overlap)
            : constraints.RemainingPaintExtent;
        var layoutExtent = Options.Pinned
            ? SliverMath.Clamp(Options.MaxExtent - constraints.ScrollOffset, 0d, effectiveRemainingPaintExtent)
            : remainingNaturalPaint;
        var paintExtent = Options.Pinned
            ? Math.Min(currentExtent, effectiveRemainingPaintExtent)
            : remainingNaturalPaint;
        var mainAxisOffset = Options.Pinned
            ? 0d
            : Math.Min(0d, remainingNaturalPaint - currentExtent);
        var cacheExtent = Options.Pinned && layoutExtent > SliverMath.Epsilon
            ? SliverMath.Clamp(-constraints.CacheOrigin + layoutExtent, 0d, constraints.RemainingCacheExtent)
            : SliverMath.CalculateCacheOffset(constraints, 0d, Options.MaxExtent);

        var geometry = new SliverGeometry
        {
            ScrollExtent = Options.MaxExtent,
            PaintOrigin = Options.Pinned ? constraints.Overlap : Math.Min(constraints.Overlap, 0d),
            PaintExtent = paintExtent,
            LayoutExtent = layoutExtent,
            MaxPaintExtent = Options.MaxExtent,
            MaxScrollObstructionExtent = Options.Pinned ? Options.MinExtent : 0d,
            HitTestExtent = paintExtent,
            Visible = paintExtent > SliverMath.Epsilon,
            HasVisualOverflow = Options.MaxExtent > constraints.RemainingPaintExtent ||
                                constraints.ScrollOffset > SliverMath.Epsilon,
            CacheExtent = cacheExtent,
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
        ArgumentNullException.ThrowIfNull(options);
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
            ? constraints.ViewportMainAxisExtent
            : Math.Max(remainingViewport, naturalExtent);

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

        var beforePaddingPaintExtent = SliverMath.CalculatePaintOffset(constraints, 0d, Padding.Before);
        var beforePaddingCacheExtent = SliverMath.CalculateCacheOffset(constraints, 0d, Padding.Before);
        var overlap = constraints.Overlap > 0d
            ? Math.Max(0d, constraints.Overlap - beforePaddingPaintExtent)
            : constraints.Overlap;
        var childScrollOffset = Math.Max(0d, constraints.ScrollOffset - Padding.Before);
        var childConstraints = constraints with
        {
            ScrollOffset = childScrollOffset,
            PrecedingScrollExtent = constraints.PrecedingScrollExtent + Padding.Before,
            Overlap = overlap,
            CrossAxisExtent = Math.Max(0d, constraints.CrossAxisExtent - Padding.CrossBefore - Padding.CrossAfter),
            RemainingPaintExtent = Math.Max(0d, constraints.RemainingPaintExtent - beforePaddingPaintExtent),
            CacheOrigin = Math.Min(0d, constraints.CacheOrigin + Padding.Before),
            RemainingCacheExtent = Math.Max(0d, constraints.RemainingCacheExtent - beforePaddingCacheExtent)
        };

        var childResult = Child.Layout(childConstraints);

        if (childResult.Geometry.ScrollOffsetCorrection is { } correction)
        {
            return new SliverLayoutResult(
                new SliverGeometry
                {
                    ScrollOffsetCorrection = correction,
                    CrossAxisExtent = constraints.CrossAxisExtent
                },
                Array.Empty<SliverLayoutSlot>());
        }

        var scrollExtent = Padding.Before + childResult.Geometry.ScrollExtent + Padding.After;
        var afterPaddingStart = Padding.Before + childResult.Geometry.ScrollExtent;
        var afterPaddingEnd = afterPaddingStart + Padding.After;
        var afterPaddingPaintExtent = SliverMath.CalculatePaintOffset(constraints, afterPaddingStart, afterPaddingEnd);
        var afterPaddingCacheExtent = SliverMath.CalculateCacheOffset(constraints, afterPaddingStart, afterPaddingEnd);
        var mainAxisPaddingPaintExtent = beforePaddingPaintExtent + afterPaddingPaintExtent;
        var mainAxisPaddingCacheExtent = beforePaddingCacheExtent + afterPaddingCacheExtent;
        var paintExtent = Math.Min(
            beforePaddingPaintExtent + Math.Max(
                childResult.Geometry.PaintExtent,
                childResult.Geometry.LayoutExtent + afterPaddingPaintExtent),
            constraints.RemainingPaintExtent);
        var geometry = new SliverGeometry
        {
            PaintOrigin = childResult.Geometry.PaintOrigin,
            ScrollExtent = scrollExtent,
            PaintExtent = paintExtent,
            LayoutExtent = Math.Min(mainAxisPaddingPaintExtent + childResult.Geometry.LayoutExtent, paintExtent),
            CacheExtent = Math.Min(mainAxisPaddingCacheExtent + childResult.Geometry.CacheExtent, constraints.RemainingCacheExtent),
            MaxPaintExtent = Padding.Before + childResult.Geometry.MaxPaintExtent + Padding.After,
            MaxScrollObstructionExtent = childResult.Geometry.MaxScrollObstructionExtent,
            HitTestExtent = Math.Max(
                mainAxisPaddingPaintExtent + childResult.Geometry.PaintExtent,
                beforePaddingPaintExtent + childResult.Geometry.HitTestExtent),
            Visible = paintExtent > SliverMath.Epsilon,
            HasVisualOverflow = childResult.Geometry.HasVisualOverflow,
            CrossAxisExtent = constraints.CrossAxisExtent
        };
        var leadingOffset = beforePaddingPaintExtent;
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
