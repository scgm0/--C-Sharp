using Godot;

namespace 球武道.scripts;

public partial class 传承 : Area2D {
	public double Duration;
	public 设定.属性值 属性;
	public StringName 身份;
	private double _计时 = 1.0;
	private MeshInstance2D _meshInstance2D;
	private MeshInstance2D _meshInstance2D2;

	public double 计时 {
		set {
			_计时 = value;
			((ShaderMaterial)_meshInstance2D2.Material).SetShaderParameter("fill_ratio", 计时);
		}
		get => _计时;
	}

	public override void _Ready() {
		_meshInstance2D = GetNode<MeshInstance2D>("MeshInstance2D");
		_meshInstance2D2 = GetNode<MeshInstance2D>("MeshInstance2D2");
		Modulate = 设定.阵营[身份];
		var tween = CreateTween();
		tween.TweenProperty(this, "计时", 0.0, Duration);
		tween.TweenCallback(Callable.From(() => { 
			身份 = "无";
			Modulate = Colors.White;
		}));

		BodyEntered += body => {
			if (body is Ball ball) {
				if (ball.身份 == 身份) {
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.资质, 属性.资质 ?? 0.0);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, 属性.修为 ?? 0.0);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.生命, Mathf.CeilToInt(ball.生命上限 * 0.01 + 属性.生命 ?? 0.0));
					QueueFree();
					GetNode("CollisionShape2D").SetDeferred("disabled", true);
				} else if (身份 == "无") {
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.资质, (属性.资质 ?? 0.0) * 0.5);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, (属性.修为 ?? 0.0) * 0.5);
					ball.EmitSignal(Ball.SignalName.属性事件,
						(int)设定.属性名.生命,
						-Mathf.CeilToInt((ball.生命上限 * 0.01 + (属性.生命 ?? 0.0) * 0.8) * 属性.伤害 ?? 1.0));
					if (ball.生命 <= 0) {
						属性 = 属性 with {
							资质 = (属性.资质 + ball.资质) * 0.5,
							修为 = 属性.修为 + ball.总修为 * 0.05,
							生命 = 属性.生命 + ball.生命上限 * 0.05,
							伤害 = 属性.伤害 + 0.05
						};
						// Modulate = Modulate with { A = (float)(Modulate.A - 0.1) };
						计时 += 0.01;
						ball.死($"死于无名传承");
					} else {
						
						QueueFree();
						GetNode("CollisionShape2D").SetDeferred("disabled", true);
					}
				}
			}
		};
	}
}