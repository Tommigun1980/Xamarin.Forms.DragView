using System;

namespace Xamarin.Forms.DragView
{
    [ContentProperty(nameof(ViewContent))]
    public partial class DragView : ContentView
    {
        private const string SwipeYAnimationName = "SwipeYAnimation";

        public static readonly BindableProperty ViewContentProperty = BindableProperty.Create(
            nameof(ViewContent), typeof(View), typeof(DragView));

        public static readonly BindableProperty MaxBoundsProperty = BindableProperty.Create(
            nameof(MaxBounds), typeof(double), typeof(DragView), 0.5);

        public static readonly BindableProperty MinBoundsProperty = BindableProperty.Create(
            nameof(MinBounds), typeof(double), typeof(DragView), 0.1);

        public static readonly BindableProperty SwipeThresholdProperty = BindableProperty.Create(
            nameof(SwipeThreshold), typeof(double), typeof(DragView), 50.0);

        public View ViewContent
        {
            get => (View)GetValue(DragView.ViewContentProperty);
            set => SetValue(DragView.ViewContentProperty, value);
        }
        // maximum the pane can be dragged to. 1.0 means upper screen edge (100% of screen height)
        public double MaxBounds
        {
            get => (double)GetValue(DragView.MaxBoundsProperty);
            set => SetValue(DragView.MaxBoundsProperty, value);
        }
        // minimum the pane can be dragged to (and also its start value). 0.1 means 10% of screen height, from the bottom
        public double MinBounds
        {
            get => (double)GetValue(DragView.MinBoundsProperty);
            set => SetValue(DragView.MinBoundsProperty, value);
        }
        public double SwipeThreshold
        {
            get => (double)GetValue(DragView.SwipeThresholdProperty);
            set => SetValue(DragView.SwipeThresholdProperty, value);
        }

        private double previousDeltaY;
        private Element previousParent;

        public DragView()
        {
            InitializeComponent();

            this.SizeChanged += (object sender, EventArgs e) => this.ResetIfParentChanged();
        }

        private void ResetIfParentChanged()
        {
            if (this.Parent != this.previousParent)
            {
                this.previousParent = this.Parent;

                this.AbortAnimation(SwipeYAnimationName);
                this.previousDeltaY = 0;
                this.SetHeightRequest(this.GetMinHeight());

                // HACK! content presenter's content is null if it's assigned when view is not drawn. TODO: REMOVEME once Xamarin fixes this
                this.contentPresenter.Content = this.ViewContent;
            }
        }

        private void MessagesPanel_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    var deltaY = e.TotalY - this.previousDeltaY;
                    this.PanPanel(deltaY);
                    this.previousDeltaY = e.TotalY;
                    break;

                case GestureStatus.Completed:
                    this.previousDeltaY = 0;
                    break;
            }
        }

        private void PanPanel(double deltaY)
        {
            if (this.AnimationIsRunning(SwipeYAnimationName))
                return;

            // negative y translation brings panel upwards, content grows upwards
            if (Math.Abs(deltaY) >= this.SwipeThreshold)
            {
                var newHeight = deltaY <= 0 ? this.GetMaxHeight() : this.GetMinHeight();

                var animation = new Animation(this.SetHeightRequest, this.Height, newHeight);
                animation.Commit(this, SwipeYAnimationName, 16, 250, Easing.BounceOut);
            }
            else
            {
                try
                {
                    var newHeight = this.Height + (-deltaY);
                    this.SetHeightRequest(newHeight);
                }
                catch (ArgumentException) { }
            }
        }

        private double GetMaxHeight()
        {
            return this.MaxBounds * this.GetContainerHeight();
        }
        private double GetMinHeight()
        {
            return this.MinBounds * this.GetContainerHeight();
        }
        private double GetContainerHeight()
        {
            var visualParent = this.Parent as VisualElement;
            return visualParent != null ? visualParent.Height : 0;
        }

        private void SetHeightRequest(double value)
        {
            try
            {
                value = /*Math.*/Clamp(value, this.GetMinHeight(), this.GetMaxHeight());
            }
            catch (ArgumentException) { }

            this.HeightRequest = value;
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
