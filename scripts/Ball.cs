using System;
using System.ComponentModel;
using Godot;

namespace 球武道.scripts;

public partial class Ball : RigidBody2D {

	[Signal]
	public delegate void 死亡事件EventHandler(string 死因);

	[Signal]
	public delegate void 属性事件EventHandler(设定.属性名 name, double num);

	[Signal]
	public delegate void 攻击事件EventHandler(Ball ball, Ball dBall);

	private 设定.境界 _境界;
	private int _年龄;
	private int _生命 = 100;
	private int _生命上限 = 100;
	private int _寿命 = 50;
	private double _修为;
	private int _击杀数;
	private int _tipIndex0;
	private int _tipIndex1;

	public int 累计修为;
	[Export] public StringName 名字;
	[Export] public StringName 身份;
	public int 修为上限 = 120;
	public bool 已死;
	[Export] public double 资质 = 0.2;

	public int 击杀数 {
		set {
			_击杀数 = value;
			if (击杀数 % 5 == 0) {
				EmitSignal(SignalName.属性事件, (int)设定.属性名.修为上限, Mathf.CeilToInt(修为上限 * 0.05));
			}
		}
		get => _击杀数;
	}

	public MeshInstance2D Body;
	

	[Export]
	public 设定.境界 境界 {
		set {
			if (!Enum.IsDefined(typeof(设定.境界), value)) {
				throw new InvalidEnumArgumentException(nameof(value));
			}

			_境界 = value;
			Mass = (int)境界 + 1;
			GetNode<Label>("%境界").Text = 境界.ToString();
			GetNode<Label>("%境界").AddThemeFontSizeOverride("font_size", 16 + (int)境界 * 2);
			GetNode<Label>("%境界").AddThemeColorOverride("font_outline_color",
				new Color((float)((double)境界 / 10.0), (float)((double)境界 / 10.0), (float)((double)境界 / 10.0)));
		}
		get => _境界;
	}

	[Export]
	public double 修为 {
		private set {
			_修为 = Mathf.Snapped(value, 0.1);
			((ShaderMaterial)GetNode<MeshInstance2D>("%修为").Material).SetShaderParameter("fill_ratio", 修为 / 修为上限 * 0.5);
			if (修为 > 修为上限) {
				突破();
			}
		}
		get => _修为;
	}

	public int 寿命 {
		private set {
			_寿命 = value;
			GetNode<Label>("%年龄").Text = GlData.GetAgeGroup(年龄, 寿命);
			// if (年龄 >= 寿命) {
			// 	死($"寿尽而亡");
			// }
		}
		get => _寿命;
	}

	public int 年龄 {
		set {
			_年龄 = value;
			GetNode<Label>("%年龄").Text = GlData.GetAgeGroup(年龄, 寿命);
			if (年龄 >= 寿命) {
				死($"寿尽而亡");
				var cc = GlData.Singletons.Inherited.Instantiate<传承>();
				cc.身份 = 身份;
				cc.属性 = new 设定.属性值 {
					资质 = 资质,
					修为 = (累计修为 + 修为) * 0.25
				};
				cc.Duration = 寿命 * 1.2 * (1.0 + (int)境界 * 0.5) * 2;
				cc.GlobalPosition = GlobalPosition;
				AddSibling(cc);
			}
		}
		get => _年龄;
	}

	public int 生命上限 {
		private set {
			_生命上限 = value;
			((ShaderMaterial)GetNode<MeshInstance2D>("%生命").Material).SetShaderParameter("fill_ratio", (double)生命 / 生命上限 * 0.5);
		}
		get => _生命上限;
	}

	public int 生命 {
		private set {
			_生命 = Mathf.Min(value, 生命上限);
			((ShaderMaterial)GetNode<MeshInstance2D>("%生命").Material).SetShaderParameter("fill_ratio", (double)生命 / 生命上限 * 0.5);
		}
		get => _生命;
	}


	public override void _Ready() {
		Body = GetNode<MeshInstance2D>("Body");
		Body.Modulate = 设定.阵营[身份];
		GetNode<Label>("%名字").Text = 名字;
		死亡事件 += 死因 => {
			var par = GlData.Singletons.Particles.Instantiate<GpuParticles2D>();
			par.Emitting = true;
			par.Modulate = Body.Modulate;
			par.GlobalPosition = GlobalPosition;
			AddSibling(par);
			GlData.MainLog($"[color={Body.Modulate.ToHtml()}][font_size=21]{名字}【{境界}】[/font_size][/color]{死因}：享年{年龄}岁");
		};

		属性事件 += (name, num) => {
			if (num == 0) return;
			AddTip($"{name}{(num > 0 ? $"+{num:F1}": num):F1}", num);
			switch (name) {
				case 设定.属性名.寿命: break;
				case 设定.属性名.年龄: break;
				case 设定.属性名.生命上限:
					生命上限 += (int)num;
					break;
				case 设定.属性名.生命:
					生命 += (int)num;
					break;
				case 设定.属性名.修为上限:
					修为上限 += (int)num;
					break;
				case 设定.属性名.修为:
					修为 += num;
					break;
				case 设定.属性名.资质:
					资质 += num;
					break;
				default: throw new ArgumentOutOfRangeException(nameof(name), name, null);
			}
			
		};
	}

	public void 过月() {
		修为 += 资质;
	}

	public void 过年() {
		年龄 += 1;
	}

	public async void 死(string 死因 = "") {
		已死 = true;
		RemoveFromGroup("武者");
		RemoveFromGroup(身份);
		Body.Visible = false;
		EmitSignal(SignalName.死亡事件, 死因);
		SetDeferred("sleeping", true);
		SetDeferred("freeze", true);
		GetNode("CollisionShape2D").SetDeferred("disabled", true);
		await ToSignal(GetTree().CreateTimer(2), Timer.SignalName.Timeout);
		QueueFree();
	}

	public void 突破() {
		if (已死) return;
		累计修为 += 修为上限;
		var old修为上限 = 修为上限;
		境界 ++;
		Mul(this, 设定.属性[境界]);
		EmitSignal(SignalName.属性事件, (int)设定.属性名.生命, (int)(生命上限 * 0.1));
		GlData.MainLog(
			$"[color={
				Body.Modulate.ToHtml()
			}][font_size=21]{
				名字
			}[/font_size][/color] 突破了[color={
				Body.Modulate.ToHtml()
			}][font_size=21]【{
				境界
			}】[/font_size][/color]： 寿命【{
				年龄
			}/{
				寿命
			}】 生命【{
				生命
			}/{
				生命上限
			}】 修为【{
				修为
				:F1}/{
				修为上限
			}】 资质【{
				资质
				:F1}】");
		修为 -= old修为上限;
	}

	public async void AddTip(string text, double num) {
		var tip = GlData.Singletons.Tip.Instantiate<Tip>();
		tip.Text = text;
		tip.正负 = num < 0;
		tip.AddThemeColorOverride("font_color", Body.Modulate);
		if (num > 0) {
			tip.Position = tip.Position with { Y = -30 };
			GetTree().CreateTimer(0.4).Timeout += () => _tipIndex0 -= 1;
			_tipIndex0++;
			await ToSignal(GetTree().CreateTimer((_tipIndex0 - 1) * 0.4), Timer.SignalName.Timeout);
		} else {
			tip.Position = tip.Position with { Y = 5 };
			GetTree().CreateTimer(0.4).Timeout += () => _tipIndex1 -= 1;
			_tipIndex1++;
			await ToSignal(GetTree().CreateTimer((_tipIndex1 - 1) * 0.4), Timer.SignalName.Timeout);
		}

		AddChild(tip);
	}

	public static void Add(Ball a, 设定.属性值 b) {
		if (b.寿命 != null) a.寿命 += Mathf.CeilToInt((double)b.寿命);
		if (b.年龄 != null) a.年龄 += Mathf.CeilToInt((double)b.年龄);
		if (b.生命上限 != null) a.生命上限 += Mathf.CeilToInt((double)b.生命上限);
		if (b.生命 != null) a.生命 += Mathf.CeilToInt((double)b.生命);
		if (b.修为上限 != null) a.修为上限 += Mathf.CeilToInt((double)b.修为上限);
		if (b.修为 != null) a.修为 += (double)b.修为;
		if (b.资质 != null) a.资质 += (double)b.资质;
	}

	public static void Mul(Ball a, 设定.属性值 b) {
		if (b.寿命 != null) a.寿命 = (int)(a.寿命 * b.寿命);
		if (b.年龄 != null) a.年龄 = (int)(a.年龄 * b.年龄);
		if (b.生命上限 != null) a.生命上限 = (int)(a.生命上限 * b.生命上限);
		if (b.生命 != null) a.生命 = (int)(a.生命 * b.生命);
		if (b.修为上限 != null) a.修为上限 = (int)(a.修为上限 * b.修为上限);
		if (b.修为 != null) a.修为 *= (double)b.修为;
		if (b.资质 != null) a.资质 *= (double)b.资质;
	}
}