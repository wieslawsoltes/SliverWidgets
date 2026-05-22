namespace SliverWidgets.Core;

public sealed class SliverChildExtentCache
{
    private readonly SortedDictionary<int, double> _observedExtents = new();

    public SliverChildExtentCache(int itemCount, double defaultExtent, double spacing = 0d)
    {
        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemCount));
        }

        SliverMath.ThrowIfNegative(defaultExtent, nameof(defaultExtent));
        SliverMath.ThrowIfNegative(spacing, nameof(spacing));

        ItemCount = itemCount;
        DefaultExtent = defaultExtent;
        Spacing = spacing;
    }

    public int ItemCount { get; }

    public double DefaultExtent { get; }

    public double Spacing { get; }

    public int ObservedCount => _observedExtents.Count;

    public double ObservedExtentSum { get; private set; }

    public double DeadReckonedExtent => ObservedCount == 0
        ? DefaultExtent
        : ObservedExtentSum / ObservedCount;

    public void Observe(int index, double extent)
    {
        ThrowIfIndexOutOfRange(index);
        SliverMath.ThrowIfNegative(extent, nameof(extent));

        if (_observedExtents.TryGetValue(index, out var previous))
        {
            ObservedExtentSum -= previous;
        }

        _observedExtents[index] = extent;
        ObservedExtentSum += extent;
    }

    public bool TryGetObservedExtent(int index, out double extent)
    {
        ThrowIfIndexOutOfRange(index);
        return _observedExtents.TryGetValue(index, out extent);
    }

    public double GetExtent(int index)
    {
        ThrowIfIndexOutOfRange(index);
        return _observedExtents.TryGetValue(index, out var extent)
            ? extent
            : DeadReckonedExtent;
    }

    public double GetLeadingOffset(int index)
    {
        if (index < 0 || index > ItemCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index == 0)
        {
            return 0d;
        }

        var estimate = DeadReckonedExtent;
        var offset = 0d;
        var cursor = 0;

        foreach (var observed in _observedExtents)
        {
            if (observed.Key >= index)
            {
                break;
            }

            offset += (observed.Key - cursor) * estimate;
            offset += observed.Value;
            cursor = observed.Key + 1;
        }

        offset += (index - cursor) * estimate;
        offset += index * Spacing;
        return offset;
    }

    public double GetTrailingOffset(int index) => GetLeadingOffset(index) + GetExtent(index);

    public double EstimateScrollExtent()
    {
        if (ItemCount == 0)
        {
            return 0d;
        }

        var missingCount = ItemCount - ObservedCount;
        return ObservedExtentSum + (missingCount * DeadReckonedExtent) + ((ItemCount - 1) * Spacing);
    }

    public int GetIndexAtScrollOffset(double scrollOffset)
    {
        SliverMath.ThrowIfNegative(scrollOffset, nameof(scrollOffset));

        if (ItemCount == 0)
        {
            return 0;
        }

        var low = 0;
        var high = ItemCount - 1;
        var best = 0;

        while (low <= high)
        {
            var mid = low + ((high - low) / 2);
            var leadingOffset = GetLeadingOffset(mid);

            if (leadingOffset <= scrollOffset)
            {
                best = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return best;
    }

    public void Clear()
    {
        _observedExtents.Clear();
        ObservedExtentSum = 0d;
    }

    private void ThrowIfIndexOutOfRange(int index)
    {
        if (index < 0 || index >= ItemCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
