# Xamarin.Forms.DragView
*A draggable pane component for Xamarin.Forms*

![DragView](Doc/DragView.gif)

## Usage

Import the DragView assembly:
```xaml
xmlns:dragview="clr-namespace:Xamarin.Forms.DragView;assembly=Xamarin.Forms.DragView"
```

Place your DragView inside an AbsoluteLayout and dock it to one of the edges. The following example docks the DragView to the bottom:
```xaml
<AbsoluteLayout>
    <dragview:DragView
        DockingEdge="Bottom"
        MaxBounds="0.5"
        MinBounds="0.1"

        AbsoluteLayout.LayoutFlags="PositionProportional, WidthProportional"
        AbsoluteLayout.LayoutBounds="0.5, 1, 1, AutoSize">

        <!-- Your controls... -->
    </dragview:DragView>
</AbsoluteLayout>
```

DockingEdge should be set to which edge the control is docked to and can be Top, Right, Left or Bottom.
MinBounds and MaxBounds declare the minimum and maximum space the DragView can take up, denoted as percentages of the parent container's size.

See [DragView.xaml.cs](Xamarin.Forms.DragView/DragView.xaml.cs) for all bindable properties.

## More Examples

Here is the XAML for the DragView setups as seen in the demonstration image above:

```xaml
<AbsoluteLayout>
    <!-- split view on the right side of the screen, lacking swipe gesture and drag knob -->
    <dragview:DragView
        DockingEdge="Right"
        MinBounds="0.025"
        MaxBounds="1.0"
        StartBounds="0.33"
        SwipeThreshold="-1"
        IsDragKnobVisible="False"
        CornerRadius="0"

        AbsoluteLayout.LayoutFlags="PositionProportional, HeightProportional"
        AbsoluteLayout.LayoutBounds="1, 0.5, AutoSize, 1">
        
        <Label Text="Split View on Right Side" VerticalOptions="Center" LineBreakMode="NoWrap" />
    </dragview:DragView>
    
    <!-- small drag view docked to the upper right of the screen -->
    <dragview:DragView
        DockingEdge="Right"
        MaxBounds="0.8"

        AbsoluteLayout.LayoutFlags="PositionProportional, HeightProportional"
        AbsoluteLayout.LayoutBounds="1, 0.1, AutoSize, 0.2">
        
        <Label Text="Right" VerticalOptions="Center" />
    </dragview:DragView>

    <!-- small drag view docked to the lower left of the screen -->
    <dragview:DragView
        DockingEdge="Left"
        MaxBounds="0.8"

        AbsoluteLayout.LayoutFlags="PositionProportional, HeightProportional"
        AbsoluteLayout.LayoutBounds="0, 0.8, AutoSize, 0.2">
        
        <Label Text="Left" VerticalOptions="Center" HorizontalOptions="End" />
    </dragview:DragView>

    <!-- drag view docked to the bottom of the screen -->
    <dragview:DragView
        DockingEdge="Bottom"

        AbsoluteLayout.LayoutFlags="PositionProportional, WidthProportional"
        AbsoluteLayout.LayoutBounds="0.5, 1, 1, AutoSize">
        
        <Label Text="Bottom" />
    </dragview:DragView>
</AbsoluteLayout>
```
