using Godot;

namespace 球武道.scripts;

public partial class Tip : Label {
	public bool 正负;

	public override void _Ready() {
		var tween = CreateTween();
		tween.Parallel().TweenProperty(this, "position:y", Position.Y + 75 * (正负 ? 1 : -1), 1.5 * Engine.TimeScale);
		tween.Parallel().TweenProperty(this, "modulate:a", 0, 0.5 * Engine.TimeScale).SetDelay(1.0 * Engine.TimeScale);
		tween.TweenCallback(Callable.From(QueueFree));
	}
}