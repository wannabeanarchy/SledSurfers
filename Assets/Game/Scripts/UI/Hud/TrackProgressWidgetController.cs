namespace SledSurfers.UI.Hud
{
    [UIView(typeof(TrackProgressWidgetView), ViewType.Widget)]
    public sealed class TrackProgressWidgetController : UIController<TrackProgressWidgetView, float>
    {
        public TrackProgressWidgetController(TrackProgressWidgetView view) : base(view) { }

        public override void Configure(float value)
        {
            TypedView.SetValue(value);
        }
    }
}
