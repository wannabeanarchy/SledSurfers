using SledSurfers.Gameplay.Launch;
using SledSurfers.Gameplay.Run;

namespace SledSurfers.UI.Hud
{
    public sealed class GameplayHud
    {
        private readonly UIManager _ui;

        public GameplayHud(UIManager ui)
        {
            _ui = ui;
        }

        public static void Register(UIFactory factory)
        {
            factory.Register<LaunchPowerWidgetController, LaunchPowerWidgetView, float>(view => new LaunchPowerWidgetController(view));
            factory.Register<DistanceWidgetController, DistanceWidgetView, float>(view => new DistanceWidgetController(view));
            factory.Register<SpeedWidgetController, SpeedWidgetView, float>(view => new SpeedWidgetController(view));
            factory.Register<TrackProgressWidgetController, TrackProgressWidgetView, float>(view => new TrackProgressWidgetController(view));
        }

        public void Refresh(LaunchSession session, RunMetrics metrics)
        {
            if (session.Phase == RunPhase.Ready || session.Phase == RunPhase.Pulling)
            {
                _ui.Show<LaunchPowerWidgetController, LaunchPowerWidgetView, float>(session.Power);
                _ui.Hide<DistanceWidgetController>();
                _ui.Hide<SpeedWidgetController>();
                _ui.Hide<TrackProgressWidgetController>();
            }
            else
            {
                _ui.Hide<LaunchPowerWidgetController>();
                _ui.Show<DistanceWidgetController, DistanceWidgetView, float>(metrics.Distance);
                _ui.Show<SpeedWidgetController, SpeedWidgetView, float>(metrics.SpeedKmh);
                _ui.Show<TrackProgressWidgetController, TrackProgressWidgetView, float>(metrics.Progress);
            }
        }
    }
}
