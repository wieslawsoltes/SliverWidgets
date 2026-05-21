namespace SliverWidgets.Core;

public sealed class SliverVariableExtentListLayout : ISliverLayout
{
    public SliverVariableExtentListLayout(SliverChildExtentCache extentCache)
    {
        ExtentCache = extentCache ?? throw new ArgumentNullException(nameof(extentCache));
    }

    public SliverChildExtentCache ExtentCache { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        if (ExtentCache.ItemCount == 0)
        {
            return SliverLayoutResult.Empty with
            {
                Geometry = SliverGeometry.Zero with { CrossAxisExtent = constraints.CrossAxisExtent }
            };
        }

        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var startIndex = Math.Max(0, ExtentCache.GetIndexAtScrollOffset(cacheStart) - 1);
        var slots = new List<SliverLayoutSlot>();

        for (var index = startIndex; index < ExtentCache.ItemCount; index++)
        {
            var itemStart = ExtentCache.GetLeadingOffset(index);
            var itemExtent = ExtentCache.GetExtent(index);
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
                itemStart - constraints.ScrollOffset,
                0d,
                itemExtent,
                constraints.CrossAxisExtent,
                IsCacheOnly: SliverMath.IsCacheOnly(itemStart, itemEnd, visibleStart, visibleEnd)));
        }

        return new SliverLayoutResult(
            SliverFixedExtentListLayout.BuildGeometry(ExtentCache.EstimateScrollExtent(), constraints),
            slots);
    }
}

public sealed record SliverToBoxAdapterOptions(double MainAxisExtent, double? CrossAxisExtent = null)
{
    public void Validate()
    {
        SliverMath.ThrowIfNegative(MainAxisExtent, nameof(MainAxisExtent));

        if (CrossAxisExtent.HasValue)
        {
            SliverMath.ThrowIfNegative(CrossAxisExtent.Value, nameof(CrossAxisExtent));
        }
    }
}

public sealed class SliverToBoxAdapterLayout : ISliverLayout
{
    public SliverToBoxAdapterLayout(SliverToBoxAdapterOptions options)
    {
        Options = options;
        Options.Validate();
    }

    public SliverToBoxAdapterOptions Options { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var geometry = SliverFixedExtentListLayout.BuildGeometry(Options.MainAxisExtent, constraints);
        var (cacheStart, cacheEnd) = SliverMath.ResolveCacheRange(
            constraints.ScrollOffset,
            constraints.CacheOrigin,
            constraints.RemainingCacheExtent);
        var visibleStart = constraints.ScrollOffset;
        var visibleEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        var crossAxisExtent = Options.CrossAxisExtent ?? constraints.CrossAxisExtent;
        var hasCacheOverlap = SliverMath.RangesOverlap(0d, Options.MainAxisExtent, cacheStart, cacheEnd);
        var slots = hasCacheOverlap && Options.MainAxisExtent > SliverMath.Epsilon
            ? new[]
            {
                new SliverLayoutSlot(
                    0,
                    -constraints.ScrollOffset,
                    0d,
                    Options.MainAxisExtent,
                    crossAxisExtent,
                    IsCacheOnly: SliverMath.IsCacheOnly(0d, Options.MainAxisExtent, visibleStart, visibleEnd))
            }
            : Array.Empty<SliverLayoutSlot>();

        return new SliverLayoutResult(geometry with { CrossAxisExtent = crossAxisExtent }, slots);
    }
}

public sealed record SliverAdvancedPersistentHeaderOptions(
    double MinExtent,
    double MaxExtent,
    bool Pinned = false,
    bool Floating = false,
    bool Snap = false)
{
    public void Validate()
    {
        SliverMath.ThrowIfNegative(MinExtent, nameof(MinExtent));
        SliverMath.ThrowIfNegative(MaxExtent, nameof(MaxExtent));

        if (MaxExtent < MinExtent)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxExtent), "MaxExtent must be greater than or equal to MinExtent.");
        }

        if (Snap && !Floating)
        {
            throw new ArgumentException("Snap requires Floating to be enabled.", nameof(Snap));
        }
    }
}

public enum SliverHeaderSnapStatus
{
    Idle,
    SnappingToMin,
    SnappingToMax
}

public sealed class SliverPersistentHeaderState
{
    private bool _hasPreviousScrollOffset;

    public double CurrentExtent { get; private set; }

    public double LastScrollOffset { get; private set; }

    public double SnapTargetExtent { get; private set; }

    public SliverHeaderSnapStatus SnapStatus { get; private set; }

    public bool HasPreviousScrollOffset => _hasPreviousScrollOffset;

    public void Reset(double currentExtent = 0d)
    {
        SliverMath.ThrowIfNegative(currentExtent, nameof(currentExtent));
        CurrentExtent = currentExtent;
        LastScrollOffset = 0d;
        SnapTargetExtent = 0d;
        SnapStatus = SliverHeaderSnapStatus.Idle;
        _hasPreviousScrollOffset = false;
    }

    internal void SetCurrentExtent(double currentExtent)
    {
        CurrentExtent = currentExtent;
    }

    internal double ConsumeScrollDelta(double scrollOffset)
    {
        var delta = _hasPreviousScrollOffset ? scrollOffset - LastScrollOffset : 0d;
        LastScrollOffset = scrollOffset;
        _hasPreviousScrollOffset = true;
        return delta;
    }

    internal void SetSnapStatus(SliverHeaderSnapStatus snapStatus, double targetExtent)
    {
        SnapStatus = snapStatus;
        SnapTargetExtent = targetExtent;
    }
}

public readonly record struct SliverHeaderSnapAnimationContext(
    double MinExtent,
    double MaxExtent,
    SliverUserScrollDirection UserScrollDirection,
    double ScrollOffset);

public interface ISliverHeaderSnapAnimationService
{
    double Advance(double currentExtent, double targetExtent, in SliverHeaderSnapAnimationContext context);
}

public sealed class SliverInstantHeaderSnapAnimationService : ISliverHeaderSnapAnimationService
{
    public static SliverInstantHeaderSnapAnimationService Instance { get; } = new();

    private SliverInstantHeaderSnapAnimationService()
    {
    }

    public double Advance(double currentExtent, double targetExtent, in SliverHeaderSnapAnimationContext context)
    {
        SliverMath.ThrowIfNegative(currentExtent, nameof(currentExtent));
        SliverMath.ThrowIfNegative(targetExtent, nameof(targetExtent));
        return targetExtent;
    }
}

public sealed class SliverStepHeaderSnapAnimationService : ISliverHeaderSnapAnimationService
{
    public SliverStepHeaderSnapAnimationService(double stepExtent)
    {
        if (stepExtent <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(stepExtent));
        }

        StepExtent = stepExtent;
    }

    public double StepExtent { get; }

    public double Advance(double currentExtent, double targetExtent, in SliverHeaderSnapAnimationContext context)
    {
        SliverMath.ThrowIfNegative(currentExtent, nameof(currentExtent));
        SliverMath.ThrowIfNegative(targetExtent, nameof(targetExtent));

        if (Math.Abs(targetExtent - currentExtent) <= StepExtent)
        {
            return targetExtent;
        }

        return currentExtent < targetExtent
            ? currentExtent + StepExtent
            : currentExtent - StepExtent;
    }
}

public sealed class SliverAdvancedPersistentHeaderLayout : ISliverLayout
{
    public SliverAdvancedPersistentHeaderLayout(
        SliverAdvancedPersistentHeaderOptions options,
        SliverPersistentHeaderState? state = null,
        ISliverHeaderSnapAnimationService? snapAnimationService = null)
    {
        Options = options;
        Options.Validate();
        State = state ?? new SliverPersistentHeaderState();
        SnapAnimationService = snapAnimationService ?? SliverInstantHeaderSnapAnimationService.Instance;
    }

    public SliverAdvancedPersistentHeaderOptions Options { get; }

    public SliverPersistentHeaderState State { get; }

    public ISliverHeaderSnapAnimationService SnapAnimationService { get; }

    public SliverLayoutResult Layout(in SliverConstraints constraints)
    {
        constraints.Validate();

        var shrinkRange = Options.MaxExtent - Options.MinExtent;
        var naturalShrinkOffset = SliverMath.Clamp(constraints.ScrollOffset, 0d, shrinkRange);
        var naturalExtent = SliverMath.Clamp(Options.MaxExtent - naturalShrinkOffset, Options.MinExtent, Options.MaxExtent);
        var currentExtent = Options.Floating
            ? ResolveFloatingExtent(naturalExtent, constraints)
            : naturalExtent;

        if (Options.Snap && constraints.UserScrollDirection == SliverUserScrollDirection.Idle)
        {
            var midpoint = Options.MinExtent + ((Options.MaxExtent - Options.MinExtent) / 2d);
            var target = currentExtent >= midpoint ? Options.MaxExtent : Options.MinExtent;
            var context = new SliverHeaderSnapAnimationContext(
                Options.MinExtent,
                Options.MaxExtent,
                constraints.UserScrollDirection,
                constraints.ScrollOffset);
            currentExtent = SliverMath.Clamp(
                SnapAnimationService.Advance(currentExtent, target, context),
                Options.MinExtent,
                Options.MaxExtent);
            State.SetSnapStatus(
                target == Options.MaxExtent ? SliverHeaderSnapStatus.SnappingToMax : SliverHeaderSnapStatus.SnappingToMin,
                target);
        }
        else
        {
            State.SetSnapStatus(SliverHeaderSnapStatus.Idle, currentExtent);
        }

        State.SetCurrentExtent(currentExtent);

        var remainingNaturalPaint = SliverMath.ClampPaintExtent(Options.MaxExtent, constraints.ScrollOffset, constraints.RemainingPaintExtent);
        var canFloatIntoView = Options.Floating && currentExtent > Options.MinExtent;
        var paintExtent = Options.Pinned || canFloatIntoView
            ? Math.Min(currentExtent, constraints.RemainingPaintExtent)
            : remainingNaturalPaint;
        var shouldPin = canFloatIntoView
            ? constraints.ScrollOffset > SliverMath.Epsilon
            : Options.Pinned && constraints.ScrollOffset > shrinkRange + SliverMath.Epsilon;
        var mainAxisOffset = shouldPin
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

    private double ResolveFloatingExtent(double naturalExtent, in SliverConstraints constraints)
    {
        var currentExtent = State.HasPreviousScrollOffset ? State.CurrentExtent : naturalExtent;
        var scrollDelta = State.ConsumeScrollDelta(constraints.ScrollOffset);

        if (constraints.UserScrollDirection == SliverUserScrollDirection.Forward)
        {
            currentExtent += Math.Abs(scrollDelta);
        }
        else if (constraints.UserScrollDirection == SliverUserScrollDirection.Reverse)
        {
            currentExtent -= Math.Abs(scrollDelta);
        }
        else if (scrollDelta < -SliverMath.Epsilon || scrollDelta > SliverMath.Epsilon)
        {
            currentExtent -= scrollDelta;
        }
        else
        {
            currentExtent = Math.Max(currentExtent, naturalExtent);
        }

        return SliverMath.Clamp(currentExtent, Options.MinExtent, Options.MaxExtent);
    }
}
