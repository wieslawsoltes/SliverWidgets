---
title: Persistent Headers
description: Pinned, floating, and snapping header behavior.
---

# Persistent Headers

Persistent headers are slivers that collapse between a maximum and minimum extent. They are commonly used for toolbar-like surfaces, section headers, and prominent page headers that remain visible while content scrolls underneath.

## Basic Header

```csharp
var layout = new SliverPersistentHeaderLayout(
    new SliverPersistentHeaderOptions(
        MinExtent: 56,
        MaxExtent: 180,
        Pinned: true));
```

When `Pinned` is `true`, the header contributes `MaxScrollObstructionExtent` equal to its minimum extent, paints at the leading edge, and reports a `LayoutExtent` that shrinks from `MaxExtent` to zero as the header scrolls through the viewport.

When `Pinned` is `false`, the header follows Flutter's scrolling persistent-header behavior: it shrinks at the leading edge until it reaches `MinExtent`, then scrolls away while its `LayoutExtent` follows only the visible paint extent. Following slivers therefore move up continuously instead of waiting behind an empty minimum-size header slot.

## Advanced Header

`SliverAdvancedPersistentHeaderLayout` adds floating and snap behavior:

```csharp
var state = new SliverPersistentHeaderState();
var layout = new SliverAdvancedPersistentHeaderLayout(
    new SliverAdvancedPersistentHeaderOptions(
        MinExtent: 56,
        MaxExtent: 180,
        Pinned: true,
        Floating: true,
        Snap: true),
    state,
    new SliverStepHeaderSnapAnimationService(stepExtent: 12));
```

`Snap` requires `Floating`. The snap service is framework-neutral, so adapters can test behavior without depending on a UI animation clock.

## State

`SliverPersistentHeaderState` tracks:

- current extent
- last scroll offset
- snap target
- snap status

State should be preserved across layout passes for floating and snapping headers.

## Avalonia Adapter

Avalonia exposes `SliverPersistentHeader`, a `Decorator` that maps a single child to the basic persistent header layout:

```xml
<slivers:SliverPersistentHeader
    xmlns:slivers="using:SliverWidgets.Avalonia"
    MinExtent="56"
    MaxExtent="180"
    Pinned="True">
  <Border>
    <TextBlock Text="Section" />
  </Border>
</slivers:SliverPersistentHeader>
```

## Gallery Coverage

The galleries demonstrate collapsed/pinned header behavior as part of mixed scroll compositions. Advanced floating and snap behavior is implemented in core and ready for deeper framework animation adapters.
