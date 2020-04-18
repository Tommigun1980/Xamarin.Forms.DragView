using System;

namespace Xamarin.Forms.DragView
{
    [ContentProperty(nameof(ViewContent))]
    public partial class DragView : ContentView
    {
        public enum DragDirectionType
        {
            Up,
            Left,
            Down,
            Right
        }

        public static readonly BindableProperty DragDirectionProperty = BindableProperty.Create(
            nameof(DragDirection), typeof(DragDirectionType), typeof(DragView), DragDirectionType.Up, propertyChanged: OnDockingEdgePropertyChanged);

        public static readonly BindableProperty ViewContentProperty = BindableProperty.Create(
            nameof(ViewContent), typeof(View), typeof(DragView), propertyChanged: OnViewContentPropertyChanged);

        public static readonly BindableProperty MaxBoundsProperty = BindableProperty.Create(
            nameof(MaxBounds), typeof(double), typeof(DragView), 0.5);

        public static readonly BindableProperty MinBoundsProperty = BindableProperty.Create(
            nameof(MinBounds), typeof(double), typeof(DragView), 0.1);

        // this gets clamped to min and max bounds
        public static readonly BindableProperty StartBoundsProperty = BindableProperty.Create(
            nameof(StartBounds), typeof(double), typeof(DragView), 0.0);

        public static readonly BindableProperty SwipeThresholdProperty = BindableProperty.Create(
            nameof(SwipeThreshold), typeof(double), typeof(DragView), 50.0);

        public static readonly BindableProperty IsDragKnobVisibleProperty = BindableProperty.Create(
            nameof(IsDragKnobVisible), typeof(bool), typeof(DragView), true);

        public static readonly BindableProperty AnimationEasingProperty = BindableProperty.Create(
            nameof(AnimationEasing), typeof(Easing), typeof(DragView), Easing.BounceOut);

        public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
            nameof(CornerRadius), typeof(double), typeof(DragView), 15.0);

        public DragDirectionType DragDirection
        {
            get => (DragDirectionType)GetValue(DragView.DragDirectionProperty);
            set => SetValue(DragView.DragDirectionProperty, value);
        }
        public View ViewContent
        {
            get => (View)GetValue(DragView.ViewContentProperty);
            set => SetValue(DragView.ViewContentProperty, value);
        }
        // maximum the pane can be dragged to. 1.0 means 100% of parent container's width or height (depending on DockingEdge)
        public double MaxBounds
        {
            get => (double)GetValue(DragView.MaxBoundsProperty);
            set => SetValue(DragView.MaxBoundsProperty, value);
        }
        // minimum the pane can be dragged to (and also its start value). 0.1 means 10% of parent container's width or height (depending on DockingEdge)
        public double MinBounds
        {
            get => (double)GetValue(DragView.MinBoundsProperty);
            set => SetValue(DragView.MinBoundsProperty, value);
        }
        public double StartBounds
        {
            get => (double)GetValue(DragView.StartBoundsProperty);
            set => SetValue(DragView.StartBoundsProperty, value);
        }
        public double SwipeThreshold
        {
            get => (double)GetValue(DragView.SwipeThresholdProperty);
            set => SetValue(DragView.SwipeThresholdProperty, value);
        }
        public bool IsDragKnobVisible
        {
            get => (bool)GetValue(DragView.IsDragKnobVisibleProperty);
            set => SetValue(DragView.IsDragKnobVisibleProperty, value);
        }
        public Easing AnimationEasing
        {
            get => (Easing)GetValue(DragView.AnimationEasingProperty);
            set => SetValue(DragView.AnimationEasingProperty, value);
        }
        public double CornerRadius
        {
            get => (double)GetValue(DragView.CornerRadiusProperty);
            set => SetValue(DragView.CornerRadiusProperty, value);
        }

        private static void OnDockingEdgePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
                ((DragView)bindable).RefreshVisualState();
        }

        private static void OnViewContentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
                ((DragView)bindable).ReassignContentPresenterAndPropagateBindingContext();
        }

        private const string SwipeAnimationName = "SwipeAnimation";

        private double previousDelta;
        private Element previousParent;

        public DragView()
        {
            InitializeComponent();

            this.RefreshVisualState();

            this.SizeChanged += (object sender, EventArgs args) => this.ResetIfParentChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            this.PropagateBindingContext();
        }

        private void RefreshVisualState()
        {
            VisualStateManager.GoToState(this.dragViewDragKnob, this.DragDirection.ToString());

            this.Reset();
        }

        private void ResetIfParentChanged()
        {
            if (this.Parent != this.previousParent)
            {
                this.previousParent = this.Parent;

                this.Reset();

                // HACK #10374! TODO: REMOVEME once Xamarin fixes this
                this.ReassignContentPresenterAndPropagateBindingContext();
            }
        }

        // HACK #10374! TODO: REMOVEME once Xamarin fixes this
        private void ReassignContentPresenterAndPropagateBindingContext()
        {
            this.contentPresenter.Content = this.ViewContent;

            this.PropagateBindingContext();
        }

        private void PropagateBindingContext()
        {
            if (this.ViewContent != null)
                BindableObject.SetInheritedBindingContext(this.ViewContent, this.BindingContext);
        }

        private void Reset()
        {
            this.AbortAnimation(SwipeAnimationName);
            this.previousDelta = 0;
            this.SetSizeRequest(this.GetContainerStartSize());
        }

        private void MessagesPanel_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    var total = this.IsVertical() ? e.TotalY : e.TotalX;

                    var delta = total - this.previousDelta;

                    if (!this.IsPositiveGrowthAxis())
                        delta = -delta;

                    this.PanPanel(delta);
                    this.previousDelta = total;
                    break;

                case GestureStatus.Completed:
                    this.previousDelta = 0;
                    break;
            }
        }

        private void PanPanel(double delta)
        {
            if (this.AnimationIsRunning(SwipeAnimationName))
                return;

            // negative y translation brings panel upwards, content grows upwards
            if (this.SwipeThreshold >= 0 && Math.Abs(delta) >= this.SwipeThreshold)
            {
                var newSize = delta <= 0 ? this.GetContainerMaxSize() : this.GetContainerMinSize();

                var animation = new Animation(this.SetSizeRequest, this.GetSizeFrom(this), newSize);
                animation.Commit(this, SwipeAnimationName, 16, 250, this.AnimationEasing);
            }
            else
            {
                var newSize = this.GetSizeFrom(this) + (-delta);
                this.SetSizeRequest(newSize);
            }
        }

        private bool IsVertical()
        {
            return this.DragDirection == DragDirectionType.Up || this.DragDirection == DragDirectionType.Down;
        }
        private bool IsPositiveGrowthAxis()
        {
            return this.DragDirection == DragDirectionType.Up || this.DragDirection == DragDirectionType.Left;
        }

        private double GetContainerMaxSize()
        {
            return this.MaxBounds * this.GetContainerSize();
        }
        private double GetContainerMinSize()
        {
            return this.MinBounds * this.GetContainerSize();
        }
        private double GetContainerStartSize()
        {
            return this.StartBounds * this.GetContainerSize();
        }

        private double GetContainerSize()
        {
            var visualParent = this.Parent as VisualElement;
            return visualParent != null ? this.GetSizeFrom(visualParent) : 0;
        }

        private void SetSizeRequest(double value)
        {
            try
            {
                value = /*Math.*/Clamp(value, this.GetContainerMinSize(), this.GetContainerMaxSize());
            }
            catch (ArgumentException) { }

            if (this.IsVertical())
                this.HeightRequest = value;
            else
                this.WidthRequest = value;
        }

        private double GetSizeFrom(VisualElement element)
        {
            return this.IsVertical() ? element.Height : element.Width;
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
