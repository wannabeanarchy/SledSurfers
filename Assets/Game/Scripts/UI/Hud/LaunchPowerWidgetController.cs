namespace SledSurfers.UI.Hud
{
    [UIView(typeof(LaunchPowerWidgetView), ViewType.Widget)]
    public sealed class LaunchPowerWidgetController : UIController<LaunchPowerWidgetView, float>
    {
        public LaunchPowerWidgetController(LaunchPowerWidgetView view) : base(view) { }

        public override void Configure(float value)
        {
            TypedView.SetValue(value);
        }
    }
}
