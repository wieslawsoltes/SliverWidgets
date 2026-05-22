using Microsoft.Maui.Controls;
using SliverWidgets.Core;

namespace SliverWidgets.Maui;

public enum SliverCollectionLayoutMode
{
    FixedExtentList,
    FixedExtentGrid
}

public class SliverCollectionView : CollectionView
{
    private DataTemplate? _fixedExtentItemTemplate;
    private bool _applyingItemTemplate;
    private DataTemplate? _userItemTemplate;

    public static readonly BindableProperty AxisProperty =
        BindableProperty.Create(
            nameof(Axis),
            typeof(SliverAxis),
            typeof(SliverCollectionView),
            SliverAxis.Vertical,
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty LayoutModeProperty =
        BindableProperty.Create(
            nameof(LayoutMode),
            typeof(SliverCollectionLayoutMode),
            typeof(SliverCollectionView),
            SliverCollectionLayoutMode.FixedExtentList,
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty ItemExtentProperty =
        BindableProperty.Create(
            nameof(ItemExtent),
            typeof(double),
            typeof(SliverCollectionView),
            48d,
            validateValue: (_, value) => IsFiniteNonNegative((double)value),
            propertyChanged: OnSizingPropertyChanged);

    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(
            nameof(Spacing),
            typeof(double),
            typeof(SliverCollectionView),
            0d,
            validateValue: (_, value) => IsFiniteNonNegative((double)value),
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty CrossAxisCountProperty =
        BindableProperty.Create(
            nameof(CrossAxisCount),
            typeof(int),
            typeof(SliverCollectionView),
            1,
            validateValue: (_, value) => (int)value > 0,
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty MainAxisSpacingProperty =
        BindableProperty.Create(
            nameof(MainAxisSpacing),
            typeof(double),
            typeof(SliverCollectionView),
            0d,
            validateValue: (_, value) => IsFiniteNonNegative((double)value),
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty CrossAxisSpacingProperty =
        BindableProperty.Create(
            nameof(CrossAxisSpacing),
            typeof(double),
            typeof(SliverCollectionView),
            0d,
            validateValue: (_, value) => IsFiniteNonNegative((double)value),
            propertyChanged: OnNativeLayoutPropertyChanged);

    public static readonly BindableProperty CacheExtentProperty =
        BindableProperty.Create(
            nameof(CacheExtent),
            typeof(double),
            typeof(SliverCollectionView),
            0d,
            validateValue: (_, value) => IsFiniteNonNegative((double)value));

    public SliverCollectionView()
    {
        ApplyNativeVirtualizationSettings();
    }

    public SliverAxis Axis
    {
        get => (SliverAxis)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public SliverCollectionLayoutMode LayoutMode
    {
        get => (SliverCollectionLayoutMode)GetValue(LayoutModeProperty);
        set => SetValue(LayoutModeProperty, value);
    }

    public double ItemExtent
    {
        get => (double)GetValue(ItemExtentProperty);
        set => SetValue(ItemExtentProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public int CrossAxisCount
    {
        get => (int)GetValue(CrossAxisCountProperty);
        set => SetValue(CrossAxisCountProperty, value);
    }

    public double MainAxisSpacing
    {
        get => (double)GetValue(MainAxisSpacingProperty);
        set => SetValue(MainAxisSpacingProperty, value);
    }

    public double CrossAxisSpacing
    {
        get => (double)GetValue(CrossAxisSpacingProperty);
        set => SetValue(CrossAxisSpacingProperty, value);
    }

    public double CacheExtent
    {
        get => (double)GetValue(CacheExtentProperty);
        set => SetValue(CacheExtentProperty, value);
    }

    public bool UsesNativeVirtualization => true;

    public double EffectiveMainAxisSpacing => LayoutMode == SliverCollectionLayoutMode.FixedExtentList
        ? Spacing
        : MainAxisSpacing;

    public ItemsLayout CreateItemsLayout()
    {
        return LayoutMode switch
        {
            SliverCollectionLayoutMode.FixedExtentList =>
                SliverItemsLayoutFactory.CreateFixedExtentList(Axis, ItemExtent, Spacing),
            SliverCollectionLayoutMode.FixedExtentGrid =>
                SliverItemsLayoutFactory.CreateFixedExtentGrid(Axis, CrossAxisCount, MainAxisSpacing, CrossAxisSpacing),
            _ => throw new ArgumentOutOfRangeException(nameof(LayoutMode), LayoutMode, null)
        };
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ApplyNativeVirtualizationSettings();
    }

    private void ApplyNativeVirtualizationSettings()
    {
        ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
        ItemsLayout = CreateItemsLayout();
        ApplyFixedExtentItemTemplate();
        InvalidateMeasure();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(ItemTemplate) && !_applyingItemTemplate)
        {
            _userItemTemplate = ItemTemplate;
            ApplyFixedExtentItemTemplate();
        }
    }

    private static void OnNativeLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SliverCollectionView collectionView)
        {
            collectionView.ApplyNativeVirtualizationSettings();
        }
    }

    private static void OnSizingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SliverCollectionView collectionView)
        {
            collectionView.ApplyNativeVirtualizationSettings();
        }
    }

    private void ApplyFixedExtentItemTemplate()
    {
        if (_applyingItemTemplate)
        {
            return;
        }

        if (LayoutMode != SliverCollectionLayoutMode.FixedExtentList)
        {
            if (_fixedExtentItemTemplate is not null && ReferenceEquals(ItemTemplate, _fixedExtentItemTemplate))
            {
                SetNativeItemTemplate(_userItemTemplate);
            }

            return;
        }

        var userTemplate = _userItemTemplate;
        if (userTemplate is null && !ReferenceEquals(ItemTemplate, _fixedExtentItemTemplate))
        {
            userTemplate = ItemTemplate;
            _userItemTemplate = userTemplate;
        }

        if (userTemplate is null)
        {
            return;
        }

        _fixedExtentItemTemplate = CreateFixedExtentItemTemplate(userTemplate);
        SetNativeItemTemplate(_fixedExtentItemTemplate);
    }

    private void SetNativeItemTemplate(DataTemplate? template)
    {
        _applyingItemTemplate = true;
        try
        {
            ItemTemplate = template;
        }
        finally
        {
            _applyingItemTemplate = false;
        }
    }

    private DataTemplate CreateFixedExtentItemTemplate(DataTemplate userTemplate)
    {
        return new DataTemplate(() =>
        {
            var content = userTemplate.CreateContent();
            if (content is not View view)
            {
                throw new InvalidOperationException("SliverCollectionView item templates must create a View.");
            }

            var wrapper = new ContentView
            {
                Content = view,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            var extentProperty = Axis == SliverAxis.Vertical
                ? HeightRequestProperty
                : WidthRequestProperty;
            wrapper.SetBinding(extentProperty, new Binding(nameof(ItemExtent)) { Source = this });
            return wrapper;
        });
    }

    private static bool IsFiniteNonNegative(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0d;
    }
}
