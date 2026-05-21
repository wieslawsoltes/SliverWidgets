namespace SliverWidgets.Core;

public enum SliverDataGridColumnWidthMode
{
    Fixed,
    Auto,
    SizeToHeader,
    SizeToCells,
    Star,
    Fill,
    LastColumnFill
}

public enum SliverDataGridSortDirection
{
    Ascending,
    Descending
}

public enum SliverDataGridFilterOperator
{
    Contains,
    Equals,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual
}

public sealed record SliverDataGridColumnDefinition(
    string Key,
    string Header,
    SliverDataGridColumnWidthMode WidthMode = SliverDataGridColumnWidthMode.Fixed,
    double Width = 120d,
    double MinWidth = 40d,
    double MaxWidth = double.PositiveInfinity,
    double StarWeight = 1d,
    double HeaderWidth = 0d,
    double CellWidth = 0d,
    bool IsVisible = true)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new ArgumentException("A DataGrid column key is required.", nameof(Key));
        }

        ThrowIfInvalidExtent(Width, nameof(Width), allowInfinity: false);
        ThrowIfInvalidExtent(MinWidth, nameof(MinWidth), allowInfinity: false);
        ThrowIfInvalidExtent(MaxWidth, nameof(MaxWidth), allowInfinity: true);
        ThrowIfInvalidExtent(HeaderWidth, nameof(HeaderWidth), allowInfinity: false);
        ThrowIfInvalidExtent(CellWidth, nameof(CellWidth), allowInfinity: false);

        if (StarWeight <= 0d || !double.IsFinite(StarWeight))
        {
            throw new ArgumentOutOfRangeException(nameof(StarWeight));
        }

        if (MaxWidth + SliverMath.Epsilon < MinWidth)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxWidth), "Maximum width must be greater than or equal to minimum width.");
        }
    }

    internal double ResolveBaseWidth()
    {
        var width = WidthMode switch
        {
            SliverDataGridColumnWidthMode.Auto => Math.Max(Width, Math.Max(HeaderWidth, CellWidth)),
            SliverDataGridColumnWidthMode.SizeToHeader => Math.Max(Width, HeaderWidth),
            SliverDataGridColumnWidthMode.SizeToCells => Math.Max(Width, CellWidth),
            SliverDataGridColumnWidthMode.Star => MinWidth,
            SliverDataGridColumnWidthMode.Fill => MinWidth,
            SliverDataGridColumnWidthMode.LastColumnFill => Math.Max(Width, Math.Max(HeaderWidth, CellWidth)),
            _ => Width
        };

        return ClampWidth(width);
    }

    internal double ClampWidth(double width)
    {
        if (double.IsPositiveInfinity(MaxWidth))
        {
            return Math.Max(MinWidth, width);
        }

        return SliverMath.Clamp(width, MinWidth, MaxWidth);
    }

    private static void ThrowIfInvalidExtent(double value, string name, bool allowInfinity)
    {
        if (allowInfinity && double.IsPositiveInfinity(value))
        {
            return;
        }

        SliverMath.ThrowIfNegative(value, name);
    }
}

public sealed record SliverDataGridLayoutOptions(
    IReadOnlyList<double> RowExtents,
    IReadOnlyList<SliverDataGridColumnDefinition> Columns,
    IReadOnlyList<int>? SourceRowIndexes = null,
    double HeaderExtent = 44d,
    double RowSpacing = 0d,
    double ColumnSpacing = 0d,
    double HorizontalScrollOffset = 0d,
    double HorizontalCacheOrigin = 0d,
    double RemainingHorizontalCacheExtent = 0d,
    int FrozenColumnCount = 0,
    bool PinHeader = true)
{
    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(RowExtents);
        ArgumentNullException.ThrowIfNull(Columns);
        SliverMath.ThrowIfNegative(HeaderExtent, nameof(HeaderExtent));
        SliverMath.ThrowIfNegative(RowSpacing, nameof(RowSpacing));
        SliverMath.ThrowIfNegative(ColumnSpacing, nameof(ColumnSpacing));
        SliverMath.ThrowIfNegative(HorizontalScrollOffset, nameof(HorizontalScrollOffset));
        SliverMath.ThrowIfNegative(RemainingHorizontalCacheExtent, nameof(RemainingHorizontalCacheExtent));

        if (!double.IsFinite(HorizontalCacheOrigin) || HorizontalCacheOrigin > SliverMath.Epsilon)
        {
            throw new ArgumentOutOfRangeException(nameof(HorizontalCacheOrigin), "Horizontal cache origin must be finite and zero or negative.");
        }

        if (FrozenColumnCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(FrozenColumnCount));
        }

        for (var row = 0; row < RowExtents.Count; row++)
        {
            SliverMath.ThrowIfNegative(RowExtents[row], $"{nameof(RowExtents)}[{row}]");
        }

        for (var column = 0; column < Columns.Count; column++)
        {
            Columns[column].Validate();
        }

        if (SourceRowIndexes is not null)
        {
            for (var row = 0; row < SourceRowIndexes.Count; row++)
            {
                var sourceRow = SourceRowIndexes[row];
                if ((uint)sourceRow >= (uint)RowExtents.Count)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(SourceRowIndexes)}[{row}]");
                }
            }
        }
    }
}

public readonly record struct SliverDataGridResolvedColumn(
    int Index,
    string Key,
    string Header,
    SliverDataGridColumnWidthMode WidthMode,
    double CrossAxisOffset,
    double CrossAxisExtent,
    bool IsFrozen);

public readonly record struct SliverDataGridRowSlot(
    int RowIndex,
    int SourceRowIndex,
    double MainAxisOffset,
    double MainAxisExtent,
    bool IsCacheOnly);

public readonly record struct SliverDataGridColumnSlot(
    int ColumnIndex,
    string ColumnKey,
    double CrossAxisOffset,
    double CrossAxisExtent,
    bool IsFrozen,
    bool IsCacheOnly);

public readonly record struct SliverDataGridCellSlot(
    int RowIndex,
    int SourceRowIndex,
    int ColumnIndex,
    string ColumnKey,
    double MainAxisOffset,
    double CrossAxisOffset,
    double MainAxisExtent,
    double CrossAxisExtent,
    bool IsHeader = false,
    bool IsFrozenColumn = false,
    bool IsRowCacheOnly = false,
    bool IsColumnCacheOnly = false);

public sealed record SliverDataGridLayoutResult(
    SliverGeometry Geometry,
    IReadOnlyList<SliverDataGridRowSlot> Rows,
    IReadOnlyList<SliverDataGridColumnSlot> Columns,
    IReadOnlyList<SliverDataGridCellSlot> Cells,
    double TotalCrossAxisExtent,
    double FrozenCrossAxisExtent);

public sealed class SliverDataGridLayout : ISliverLayout
{
    private DataGridMetrics? _metrics;

    public SliverDataGridLayout(SliverDataGridLayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options;
        Options.Validate();
    }

    public SliverDataGridLayoutOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        var result = LayoutDataGrid(constraints);
        var crossAxisExtent = constraints.CrossAxisExtent;
        return new SliverLayoutResult(
            result.Geometry,
            result.Rows.Select(row => new SliverLayoutSlot(
                row.RowIndex,
                row.MainAxisOffset,
                0d,
                row.MainAxisExtent,
                crossAxisExtent,
                IsCacheOnly: row.IsCacheOnly)).ToArray());
    }

    public SliverDataGridLayoutResult LayoutDataGrid(in SliverConstraints constraints)
    {
        constraints.Validate();

        if (constraints.Axis != SliverAxis.Vertical)
        {
            throw new NotSupportedException("DataGrid sliver layout currently supports vertical row scrolling.");
        }

        var metrics = ResolveMetrics(constraints.CrossAxisExtent);
        var rowSlots = BuildRowSlots(metrics, constraints);
        var columnSlots = BuildColumnSlots(metrics, constraints.CrossAxisExtent);
        var cells = BuildCells(rowSlots, columnSlots, metrics, constraints);
        var scrollExtent = Options.HeaderExtent + (metrics.RowCount > 0 ? Options.RowSpacing : 0d) + metrics.RowsScrollExtent;

        return new SliverDataGridLayoutResult(
            SliverFixedExtentListLayout.BuildGeometry(scrollExtent, constraints),
            rowSlots,
            columnSlots,
            cells,
            metrics.TotalCrossAxisExtent,
            metrics.FrozenCrossAxisExtent);
    }

    public double GetRowMainAxisOffset(int rowIndex)
    {
        var metrics = ResolveMetrics(0d);
        if ((uint)rowIndex >= (uint)metrics.RowCount)
        {
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        }

        return Options.HeaderExtent + (metrics.RowCount > 0 ? Options.RowSpacing : 0d) + metrics.RowOffsets[rowIndex];
    }

    public IReadOnlyList<SliverDataGridResolvedColumn> ResolveColumns(double viewportCrossAxisExtent)
    {
        return ResolveMetrics(viewportCrossAxisExtent).Columns;
    }

    private IReadOnlyList<SliverDataGridRowSlot> BuildRowSlots(DataGridMetrics metrics, in SliverConstraints constraints)
    {
        if (metrics.RowCount == 0)
        {
            return Array.Empty<SliverDataGridRowSlot>();
        }

        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var rowsStart = Options.HeaderExtent + Options.RowSpacing;
        var rowLocalCacheStart = Math.Max(0d, cacheStart - rowsStart);
        var startRow = FindFirstIndex(metrics.RowOffsets, metrics.RowsScrollExtent, rowLocalCacheStart);
        var slots = new List<SliverDataGridRowSlot>();

        for (var row = startRow; row < metrics.RowCount; row++)
        {
            var rowStart = rowsStart + metrics.RowOffsets[row];
            var rowEnd = rowStart + GetVisibleRowExtent(row);

            if (rowStart - cacheEnd >= -SliverMath.Epsilon)
            {
                break;
            }

            if (!SliverMath.RangesOverlap(rowStart, rowEnd, cacheStart, cacheEnd))
            {
                continue;
            }

            slots.Add(new SliverDataGridRowSlot(
                row,
                GetSourceRowIndex(row),
                rowStart - constraints.ScrollOffset,
                GetVisibleRowExtent(row),
                SliverMath.IsCacheOnly(rowStart, rowEnd, visibleStart, visibleEnd)));
        }

        return slots;
    }

    private IReadOnlyList<SliverDataGridColumnSlot> BuildColumnSlots(DataGridMetrics metrics, double viewportCrossAxisExtent)
    {
        if (metrics.Columns.Count == 0)
        {
            return Array.Empty<SliverDataGridColumnSlot>();
        }

        var viewportAfterFrozen = Math.Max(0d, viewportCrossAxisExtent - metrics.FrozenCrossAxisExtent);
        var horizontalCacheExtent = Options.RemainingHorizontalCacheExtent > SliverMath.Epsilon
            ? Options.RemainingHorizontalCacheExtent
            : viewportAfterFrozen;
        var visibleStart = metrics.FrozenCrossAxisExtent + Options.HorizontalScrollOffset;
        var visibleEnd = visibleStart + viewportAfterFrozen;
        var cacheStart = Math.Max(metrics.FrozenCrossAxisExtent, visibleStart + Options.HorizontalCacheOrigin);
        var cacheEnd = visibleStart + Math.Max(horizontalCacheExtent, viewportAfterFrozen);
        var slots = new List<SliverDataGridColumnSlot>();

        foreach (var column in metrics.Columns)
        {
            var columnStart = column.CrossAxisOffset;
            var columnEnd = columnStart + column.CrossAxisExtent;
            var isVisible = column.IsFrozen || SliverMath.RangesOverlap(columnStart, columnEnd, cacheStart, cacheEnd);

            if (!isVisible)
            {
                continue;
            }

            var offset = column.IsFrozen
                ? columnStart
                : metrics.FrozenCrossAxisExtent + (columnStart - metrics.FrozenCrossAxisExtent) - Options.HorizontalScrollOffset;
            var isCacheOnly = !column.IsFrozen && !SliverMath.RangesOverlap(columnStart, columnEnd, visibleStart, visibleEnd);
            slots.Add(new SliverDataGridColumnSlot(
                column.Index,
                column.Key,
                offset,
                column.CrossAxisExtent,
                column.IsFrozen,
                isCacheOnly));
        }

        return slots;
    }

    private IReadOnlyList<SliverDataGridCellSlot> BuildCells(
        IReadOnlyList<SliverDataGridRowSlot> rows,
        IReadOnlyList<SliverDataGridColumnSlot> columns,
        DataGridMetrics metrics,
        in SliverConstraints constraints)
    {
        if (columns.Count == 0)
        {
            return Array.Empty<SliverDataGridCellSlot>();
        }

        var cells = new List<SliverDataGridCellSlot>(columns.Count * Math.Max(1, rows.Count + 1));
        var headerOffset = Options.PinHeader
            ? 0d
            : -constraints.ScrollOffset;
        var headerIsCacheOnly = !Options.PinHeader &&
                                !IntersectsViewport(headerOffset, Options.HeaderExtent, constraints.RemainingPaintExtent);

        foreach (var column in columns)
        {
            cells.Add(new SliverDataGridCellSlot(
                -1,
                -1,
                column.ColumnIndex,
                column.ColumnKey,
                headerOffset,
                column.CrossAxisOffset,
                Options.HeaderExtent,
                column.CrossAxisExtent,
                IsHeader: true,
                IsFrozenColumn: column.IsFrozen,
                IsRowCacheOnly: headerIsCacheOnly,
                IsColumnCacheOnly: column.IsCacheOnly));
        }

        foreach (var row in rows)
        {
            foreach (var column in columns)
            {
                cells.Add(new SliverDataGridCellSlot(
                    row.RowIndex,
                    row.SourceRowIndex,
                    column.ColumnIndex,
                    column.ColumnKey,
                    row.MainAxisOffset,
                    column.CrossAxisOffset,
                    row.MainAxisExtent,
                    column.CrossAxisExtent,
                    IsFrozenColumn: column.IsFrozen,
                    IsRowCacheOnly: row.IsCacheOnly,
                    IsColumnCacheOnly: column.IsCacheOnly));
            }
        }

        return cells;
    }

    private DataGridMetrics ResolveMetrics(double viewportCrossAxisExtent)
    {
        var visibleColumnKeys = string.Join("|", Options.Columns.Where(column => column.IsVisible).Select(column => column.Key));
        var key = new DataGridMetricsKey(
            viewportCrossAxisExtent,
            Options.RowExtents.Count,
            Options.SourceRowIndexes?.Count ?? Options.RowExtents.Count,
            Options.Columns.Count,
            visibleColumnKeys,
            Options.RowSpacing,
            Options.ColumnSpacing,
            Options.FrozenColumnCount);

        if (_metrics is not null && _metrics.Key == key)
        {
            return _metrics;
        }

        var rowOffsets = BuildRowOffsets();
        var columns = ResolveColumnMetrics(viewportCrossAxisExtent);
        _metrics = new DataGridMetrics(
            key,
            rowOffsets,
            rowOffsets.Length == 0 ? 0d : rowOffsets[^1],
            columns.Columns,
            columns.TotalCrossAxisExtent,
            columns.FrozenCrossAxisExtent);
        return _metrics;
    }

    private double[] BuildRowOffsets()
    {
        var rowCount = Options.SourceRowIndexes?.Count ?? Options.RowExtents.Count;
        if (rowCount == 0)
        {
            return [];
        }

        var offsets = new double[rowCount + 1];
        var cursor = 0d;

        for (var row = 0; row < rowCount; row++)
        {
            offsets[row] = cursor;
            cursor += GetVisibleRowExtent(row);
            if (row < rowCount - 1)
            {
                cursor += Options.RowSpacing;
            }
        }

        offsets[rowCount] = cursor;
        return offsets;
    }

    private (IReadOnlyList<SliverDataGridResolvedColumn> Columns, double TotalCrossAxisExtent, double FrozenCrossAxisExtent)
        ResolveColumnMetrics(double viewportCrossAxisExtent)
    {
        var visibleColumns = Options.Columns
            .Select((column, index) => (Column: column, Index: index))
            .Where(item => item.Column.IsVisible)
            .ToArray();

        if (visibleColumns.Length == 0)
        {
            return (Array.Empty<SliverDataGridResolvedColumn>(), 0d, 0d);
        }

        var widths = new double[visibleColumns.Length];
        var starIndexes = new List<int>();
        var fillIndexes = new List<int>();
        var occupied = 0d;

        for (var index = 0; index < visibleColumns.Length; index++)
        {
            var column = visibleColumns[index].Column;
            widths[index] = column.ResolveBaseWidth();
            if (column.WidthMode == SliverDataGridColumnWidthMode.Star)
            {
                starIndexes.Add(index);
            }
            else if (column.WidthMode == SliverDataGridColumnWidthMode.Fill)
            {
                fillIndexes.Add(index);
            }

            occupied += widths[index];
        }

        occupied += (visibleColumns.Length - 1) * Options.ColumnSpacing;
        DistributeStarWidths(visibleColumns, widths, starIndexes, viewportCrossAxisExtent, occupied);
        DistributeFillWidths(visibleColumns, widths, fillIndexes, viewportCrossAxisExtent);
        ApplyLastColumnFill(visibleColumns, widths, viewportCrossAxisExtent);

        var resolved = new SliverDataGridResolvedColumn[visibleColumns.Length];
        var cursor = 0d;
        var frozenCount = Math.Min(Options.FrozenColumnCount, visibleColumns.Length);
        var frozenExtent = 0d;

        for (var index = 0; index < visibleColumns.Length; index++)
        {
            var item = visibleColumns[index];
            resolved[index] = new SliverDataGridResolvedColumn(
                item.Index,
                item.Column.Key,
                item.Column.Header,
                item.Column.WidthMode,
                cursor,
                widths[index],
                index < frozenCount);

            cursor += widths[index];
            if (index < visibleColumns.Length - 1)
            {
                cursor += Options.ColumnSpacing;
            }

            if (index + 1 == frozenCount)
            {
                frozenExtent = cursor;
            }
        }

        return (resolved, cursor, frozenExtent);
    }

    private static void DistributeStarWidths(
        (SliverDataGridColumnDefinition Column, int Index)[] visibleColumns,
        double[] widths,
        IReadOnlyList<int> starIndexes,
        double viewportCrossAxisExtent,
        double occupied)
    {
        if (starIndexes.Count == 0 || viewportCrossAxisExtent <= SliverMath.Epsilon)
        {
            return;
        }

        var remaining = Math.Max(0d, viewportCrossAxisExtent - occupied);
        if (remaining <= SliverMath.Epsilon)
        {
            return;
        }

        var totalWeight = starIndexes.Sum(index => visibleColumns[index].Column.StarWeight);
        foreach (var index in starIndexes)
        {
            var column = visibleColumns[index].Column;
            widths[index] = column.ClampWidth(widths[index] + (remaining * column.StarWeight / totalWeight));
        }
    }

    private void DistributeFillWidths(
        (SliverDataGridColumnDefinition Column, int Index)[] visibleColumns,
        double[] widths,
        IReadOnlyList<int> fillIndexes,
        double viewportCrossAxisExtent)
    {
        if (fillIndexes.Count == 0 || viewportCrossAxisExtent <= SliverMath.Epsilon)
        {
            return;
        }

        var total = widths.Sum() + ((visibleColumns.Length - 1) * Options.ColumnSpacing);
        var remaining = Math.Max(0d, viewportCrossAxisExtent - total);
        if (remaining <= SliverMath.Epsilon)
        {
            return;
        }

        var share = remaining / fillIndexes.Count;
        foreach (var index in fillIndexes)
        {
            widths[index] = visibleColumns[index].Column.ClampWidth(widths[index] + share);
        }
    }

    private void ApplyLastColumnFill(
        (SliverDataGridColumnDefinition Column, int Index)[] visibleColumns,
        double[] widths,
        double viewportCrossAxisExtent)
    {
        if (viewportCrossAxisExtent <= SliverMath.Epsilon)
        {
            return;
        }

        var lastFillIndex = Array.FindLastIndex(
            visibleColumns,
            item => item.Column.WidthMode == SliverDataGridColumnWidthMode.LastColumnFill);
        if (lastFillIndex < 0)
        {
            return;
        }

        var totalWithoutLast = 0d;
        for (var index = 0; index < visibleColumns.Length; index++)
        {
            if (index != lastFillIndex)
            {
                totalWithoutLast += widths[index];
            }
        }

        totalWithoutLast += (visibleColumns.Length - 1) * Options.ColumnSpacing;
        var width = Math.Max(widths[lastFillIndex], viewportCrossAxisExtent - totalWithoutLast);
        widths[lastFillIndex] = visibleColumns[lastFillIndex].Column.ClampWidth(width);
    }

    private int GetSourceRowIndex(int visibleRowIndex)
    {
        return Options.SourceRowIndexes is { } map ? map[visibleRowIndex] : visibleRowIndex;
    }

    private double GetVisibleRowExtent(int visibleRowIndex)
    {
        return Options.RowExtents[GetSourceRowIndex(visibleRowIndex)];
    }

    private static int FindFirstIndex(double[] offsets, double scrollExtent, double cacheStart)
    {
        if (offsets.Length <= 1)
        {
            return 0;
        }

        var rowCount = offsets.Length - 1;
        var low = 0;
        var high = rowCount;

        while (low < high)
        {
            var middle = low + ((high - low) / 2);
            var itemEnd = middle + 1 < offsets.Length ? offsets[middle + 1] : scrollExtent;
            if (itemEnd - cacheStart > SliverMath.Epsilon)
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }

        return Math.Max(0, low);
    }

    private static bool IntersectsViewport(double offset, double extent, double viewportExtent)
    {
        return offset + extent > SliverMath.Epsilon && viewportExtent - offset > SliverMath.Epsilon;
    }

    private sealed record DataGridMetrics(
        DataGridMetricsKey Key,
        double[] RowOffsets,
        double RowsScrollExtent,
        IReadOnlyList<SliverDataGridResolvedColumn> Columns,
        double TotalCrossAxisExtent,
        double FrozenCrossAxisExtent)
    {
        public int RowCount => RowOffsets.Length == 0 ? 0 : RowOffsets.Length - 1;
    }

    private readonly record struct DataGridMetricsKey(
        double ViewportCrossAxisExtent,
        int SourceRowCount,
        int VisibleRowCount,
        int SourceColumnCount,
        string VisibleColumnKeys,
        double RowSpacing,
        double ColumnSpacing,
        int FrozenColumnCount);
}

public sealed class SliverDeterministicDataGridRowExtentList : IReadOnlyList<double>
{
    public SliverDeterministicDataGridRowExtentList(int count, double minExtent = 36d, double maxExtent = 96d)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        SliverMath.ThrowIfNegative(minExtent, nameof(minExtent));
        SliverMath.ThrowIfNegative(maxExtent, nameof(maxExtent));
        if (maxExtent < minExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExtent));
        }

        Count = count;
        MinExtent = minExtent;
        MaxExtent = maxExtent;
    }

    public int Count { get; }

    public double MinExtent { get; }

    public double MaxExtent { get; }

    public double this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (Math.Abs(MaxExtent - MinExtent) <= SliverMath.Epsilon)
            {
                return MinExtent;
            }

            return MinExtent + ((MaxExtent - MinExtent) * (((index * 47) + 19) % 101) / 100d);
        }
    }

    public IEnumerator<double> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public sealed record SliverDataGridColumnBinding<T>(
    string Key,
    Func<T, object?> ValueSelector,
    IComparer<object?>? Comparer = null)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new ArgumentException("A DataGrid column key is required.", nameof(Key));
        }

        ArgumentNullException.ThrowIfNull(ValueSelector);
    }
}

public sealed record SliverDataGridSortDescriptor(
    string ColumnKey,
    SliverDataGridSortDirection Direction = SliverDataGridSortDirection.Ascending);

public sealed record SliverDataGridFilterDescriptor(
    string ColumnKey,
    SliverDataGridFilterOperator Operator,
    object? Value,
    bool CaseSensitive = false);

public sealed record SliverDataGridQuery(
    IReadOnlyList<SliverDataGridSortDescriptor> Sorts,
    IReadOnlyList<SliverDataGridFilterDescriptor> Filters)
{
    public static SliverDataGridQuery Empty { get; } = new(
        Array.Empty<SliverDataGridSortDescriptor>(),
        Array.Empty<SliverDataGridFilterDescriptor>());
}

public static class SliverDataGridQueryEngine
{
    public static IReadOnlyList<int> ProjectRows<T>(
        IReadOnlyList<T> rows,
        IReadOnlyList<SliverDataGridColumnBinding<T>> columns,
        SliverDataGridQuery query)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(query);

        var columnMap = new Dictionary<string, SliverDataGridColumnBinding<T>>(StringComparer.Ordinal);
        foreach (var column in columns)
        {
            column.Validate();
            columnMap[column.Key] = column;
        }

        var projected = new List<int>(rows.Count);
        for (var index = 0; index < rows.Count; index++)
        {
            if (MatchesFilters(rows[index], columnMap, query.Filters))
            {
                projected.Add(index);
            }
        }

        if (query.Sorts.Count == 0)
        {
            return projected;
        }

        projected.Sort((left, right) => CompareRows(rows, columnMap, query.Sorts, left, right));
        return projected;
    }

    private static bool MatchesFilters<T>(
        T row,
        IReadOnlyDictionary<string, SliverDataGridColumnBinding<T>> columns,
        IReadOnlyList<SliverDataGridFilterDescriptor> filters)
    {
        foreach (var filter in filters)
        {
            if (!columns.TryGetValue(filter.ColumnKey, out var column))
            {
                throw new KeyNotFoundException($"Column '{filter.ColumnKey}' was not found.");
            }

            if (!MatchesFilter(column.ValueSelector(row), filter))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesFilter(object? value, SliverDataGridFilterDescriptor filter)
    {
        var comparison = filter.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var left = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        var right = Convert.ToString(filter.Value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

        return filter.Operator switch
        {
            SliverDataGridFilterOperator.Contains => left.Contains(right, comparison),
            SliverDataGridFilterOperator.StartsWith => left.StartsWith(right, comparison),
            SliverDataGridFilterOperator.EndsWith => left.EndsWith(right, comparison),
            SliverDataGridFilterOperator.Equals => CompareValues(value, filter.Value, StringComparerFor(filter.CaseSensitive)) == 0,
            SliverDataGridFilterOperator.GreaterThan => CompareValues(value, filter.Value, StringComparerFor(filter.CaseSensitive)) > 0,
            SliverDataGridFilterOperator.GreaterThanOrEqual => CompareValues(value, filter.Value, StringComparerFor(filter.CaseSensitive)) >= 0,
            SliverDataGridFilterOperator.LessThan => CompareValues(value, filter.Value, StringComparerFor(filter.CaseSensitive)) < 0,
            SliverDataGridFilterOperator.LessThanOrEqual => CompareValues(value, filter.Value, StringComparerFor(filter.CaseSensitive)) <= 0,
            _ => false
        };
    }

    private static int CompareRows<T>(
        IReadOnlyList<T> rows,
        IReadOnlyDictionary<string, SliverDataGridColumnBinding<T>> columns,
        IReadOnlyList<SliverDataGridSortDescriptor> sorts,
        int left,
        int right)
    {
        foreach (var sort in sorts)
        {
            if (!columns.TryGetValue(sort.ColumnKey, out var column))
            {
                throw new KeyNotFoundException($"Column '{sort.ColumnKey}' was not found.");
            }

            var comparer = column.Comparer ?? SliverDataGridDefaultObjectComparer.Instance;
            var comparison = comparer.Compare(column.ValueSelector(rows[left]), column.ValueSelector(rows[right]));
            if (comparison != 0)
            {
                return sort.Direction == SliverDataGridSortDirection.Descending ? -comparison : comparison;
            }
        }

        return left.CompareTo(right);
    }

    private static int CompareValues(object? left, object? right, IComparer<object?> stringComparer)
    {
        if (left is null && right is null)
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        if (TryDouble(left, out var leftNumber) && TryDouble(right, out var rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        if (left is DateTime leftDate && right is DateTime rightDate)
        {
            return leftDate.CompareTo(rightDate);
        }

        if (left is string || right is string)
        {
            return stringComparer.Compare(left, right);
        }

        if (left is IComparable comparable && left.GetType().IsInstanceOfType(right))
        {
            return comparable.CompareTo(right);
        }

        return stringComparer.Compare(left, right);
    }

    private static bool TryDouble(object value, out double number)
    {
        if (value is IConvertible)
        {
            try
            {
                number = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                return double.IsFinite(number);
            }
            catch (FormatException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (OverflowException)
            {
            }
        }

        number = 0d;
        return false;
    }

    private static IComparer<object?> StringComparerFor(bool caseSensitive)
    {
        return caseSensitive
            ? SliverDataGridStringObjectComparer.Ordinal
            : SliverDataGridStringObjectComparer.OrdinalIgnoreCase;
    }

    private sealed class SliverDataGridDefaultObjectComparer : IComparer<object?>
    {
        public static SliverDataGridDefaultObjectComparer Instance { get; } = new();

        public int Compare(object? x, object? y)
        {
            return CompareValues(x, y, SliverDataGridStringObjectComparer.OrdinalIgnoreCase);
        }
    }

    private sealed class SliverDataGridStringObjectComparer : IComparer<object?>
    {
        public static SliverDataGridStringObjectComparer Ordinal { get; } = new(StringComparer.Ordinal);

        public static SliverDataGridStringObjectComparer OrdinalIgnoreCase { get; } = new(StringComparer.OrdinalIgnoreCase);

        private readonly StringComparer _comparer;

        private SliverDataGridStringObjectComparer(StringComparer comparer)
        {
            _comparer = comparer;
        }

        public int Compare(object? x, object? y)
        {
            var left = Convert.ToString(x, System.Globalization.CultureInfo.InvariantCulture);
            var right = Convert.ToString(y, System.Globalization.CultureInfo.InvariantCulture);
            return _comparer.Compare(left, right);
        }
    }
}
