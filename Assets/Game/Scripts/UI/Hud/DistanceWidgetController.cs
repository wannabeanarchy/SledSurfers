namespace SledSurfers.UI.Hud
{
    [UIView(typeof(DistanceWidgetView), ViewType.Widget)]
    public sealed class DistanceWidgetController : UIController<DistanceWidgetView, float>
    {
        public DistanceWidgetController(DistanceWidgetView view) : base(view) { }

        public override void Configure(float value)
        {
            TypedView.SetValue(value);
        }
    }
}
