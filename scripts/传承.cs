using Godot;

namespace 球武道.scripts;

public partial class 传承 : Area2D {
	public double Duration;
	public 设定.属性值 属性;
	public string 名字;
	public string 身份;
	private int _计时 = 100;
	private MeshInstance2D _meshInstance2D;
	private MeshInstance2D _meshInstance2D2;
	private Area2D _area2D;

	public int 计时 {
		set {
			_计时 = value;
			((ShaderMaterial)_meshInstance2D2.Material).SetShaderParameter("fill_ratio", 计时 / 100.0);
		}
		get => _计时;
	}

	public override void _Ready() {
		_meshInstance2D = GetNode<MeshInstance2D>("MeshInstance2D");
		_meshInstance2D2 = GetNode<MeshInstance2D>("MeshInstance2D2");
		_area2D = GetNode<Area2D>("Area2D");
		Modulate = 设定.阵营[身份];
		var tween = CreateTween();
		tween.TweenProperty(this, "计时", 0.0, Duration);
		tween.TweenCallback(Callable.From(() => {
			身份 = "无";
			_meshInstance2D2.Modulate = Modulate;
			Modulate = new Color((float)0.8, (float)0.8, (float)0.8, (float)0.5);
		}));

		BodyEntered += body => {
			if (body is Ball ball) {
				if (ball.身份 == 身份) {
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.资质, 属性.资质 ?? 0.0);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, 属性.修为 ?? 0.0);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.生命, Mathf.CeilToInt(ball.生命上限 * 0.01 + 属性.生命 ?? 0.0));
					GetNode("CollisionShape2D").SetDeferred("disabled", true);
					GetNode("Area2D/CollisionShape2D").SetDeferred("disabled", true);
					QueueFree();
				} else if (身份 == "无") {
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.资质, (属性.资质 ?? 0.0) * 0.5);
					ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, (属性.修为 ?? 0.0) * 0.5);
					ball.EmitSignal(Ball.SignalName.属性事件,
						(int)设定.属性名.生命,
						-Mathf.CeilToInt((ball.生命上限 * 0.01 + (属性.生命 ?? 0.0) * 0.8) * 属性.伤害 ?? 1.0));
					if (ball.生命 <= 0) {
						Modulate = (Modulate + ball.Body.Modulate) / (float)2.0;
						// Modulate = Modulate with { A = (float)0.5 };
						属性 = 属性 with {
							资质 = 属性.资质 * 0.5 + ball.资质 * (1.0 + (int)ball.境界 * 0.05),
							修为 = 属性.修为 + ball.总修为 * 0.05,
							生命 = 属性.生命 + ball.生命上限 * 0.05,
							伤害 = 属性.伤害 + 0.02
						};
						// Modulate = Modulate with { A = (float)(Modulate.A - 0.1) };
						计时 += 1;

						ball.死($"陨落于[color={_meshInstance2D2.Modulate.ToHtml()}][font_size=21]【{名字}】[/font_size][/color]之禁地");
						if (计时 == 1) {
							_meshInstance2D.Modulate = _meshInstance2D.Modulate with { A = (float)0.5 };
							_area2D.Gravity = -5;
							GetNode<GpuParticles2D>("GPUParticles2D").Emitting = true;
							GlData.MainLog($"[color={_meshInstance2D2.Modulate.ToHtml()}][font_size=21]【{名字}】[/font_size][/color]之禁地出世了");
						} else {
							_area2D.Gravity -= 1;
							GetNode<GpuParticles2D>("GPUParticles2D").Amount += 2;
						}
					} else {
						GetNode("CollisionShape2D").SetDeferred("disabled", true);
						GetNode("Area2D/CollisionShape2D").SetDeferred("disabled", true);
						QueueFree();
						if (计时 == 0) return;
						GlData.MainLog($"[color={_meshInstance2D2.Modulate.ToHtml()}][font_size=21]【{名字}】[/font_size][/color]之禁地被 [color={
							ball.Body.Modulate.ToHtml()
						}][font_size=21]{
							ball.名字
						}【{
							ball.境界
						}】[/font_size][/color]覆灭了");
						// ball.EmitSignal(Ball.SignalName.属性事件,
						// 	(int)设定.属性名.生命,
						// 	Mathf.CeilToInt(ball.生命上限 * 0.25 * (计时 / 100.0)));
						ball.EmitSignal(Ball.SignalName.属性事件,
							(int)设定.属性名.寿命,
							-Mathf.CeilToInt(ball.寿命 * 0.25 * (计时 / 100.0)));
					}
				}
			}
		};
	}
}