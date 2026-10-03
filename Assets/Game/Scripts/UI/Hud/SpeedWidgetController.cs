namespace SledSurfers.UI.Hud
{
    [UIView(typeof(SpeedWidgetView), ViewType.Widget)]
    public sealed class SpeedWidgetController : UIController<SpeedWidgetView, float>
    {
        public SpeedWidgetController(SpeedWidgetView view) : base(view) { }

        public override void Configure(float value)
        {
            TypedView.SetValue(value);
        }
    }
}
