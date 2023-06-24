using Godot;

namespace 球武道.scripts; 

public partial class 传承 : Area2D {
	public double Duration;
	public 设定.属性值 属性;
	public StringName 身份;
	private double _计时 = 1.0;

	public double 计时 {
		set {
			_计时 = value;
			((ShaderMaterial)GetNode<MeshInstance2D>("MeshInstance2D2").Material).SetShaderParameter("fill_ratio", 计时);
		}
		get => _计时;
	}

	public override void _Ready() {
		Modulate = 设定.阵营[身份];
		var tween = CreateTween();
		tween.TweenProperty(this, "计时", 0.0, Duration);
		tween.TweenCallback(Callable.From(() => {
			QueueFree();
			GetNode("CollisionShape2D").SetDeferred("disabled", true);
		}));

		BodyEntered += body => {
			if (body is not Ball ball || ball.身份 != 身份) return;
			ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.资质, 属性.资质 ?? 0.0);
			ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, 属性.修为 ?? 0.0);
			ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.生命, Mathf.CeilToInt(ball.生命上限 * 0.1));
			QueueFree();
			GetNode("CollisionShape2D").SetDeferred("disabled", true);
		};
	}
}