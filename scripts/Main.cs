using System.Linq;
using Godot;
using Godot.Collections;
using Timer = Godot.Timer;

namespace 球武道.scripts;

public partial class Main : Node2D {
	private bool _t = true;

	[Export] public int 限时 = 100;

	public CanvasGroup 地图;
	public RigidBody2D 红出生点;
	public RigidBody2D 黄出生点;
	public RigidBody2D 蓝出生点;
	public RigidBody2D 绿出生点;


	public Label 年月;
	public RichTextLabel 日志;
	public 境界榜 境界榜;
	public 杀戮榜 杀戮榜;
	public 资质榜 资质榜;
	public int 月 = 1;
	public int 年;


	public override void _Ready() {
		GlData.Singletons.Log += OnSingletonsOnLog;
		// _thread = new Thread(() => {
		// 	while (年 <= 限时) {
		// 		Thread.Sleep(50);
		// 		CallDeferred("_OnTimerTimeout");
		// 		if(年 > 限时) return;
		// 	}
		// 	
		// });
		// Engine.TimeScale = 2.0;

		年月 = GetNode<Label>("%年月");
		日志 = GetNode<RichTextLabel>("%日志");
		境界榜 = GetNode<境界榜>("%境界榜");
		杀戮榜 = GetNode<杀戮榜>("%杀戮榜");
		资质榜 = GetNode<资质榜>("%资质榜");
		地图 = GetNode<CanvasGroup>("%地图");
		红出生点 = GetNode<RigidBody2D>("%红出生点");
		黄出生点 = GetNode<RigidBody2D>("%黄出生点");
		蓝出生点 = GetNode<RigidBody2D>("%蓝出生点");
		绿出生点 = GetNode<RigidBody2D>("%绿出生点");

		红出生点.Modulate = 设定.阵营["红"];
		黄出生点.Modulate = 设定.阵营["黄"];
		蓝出生点.Modulate = 设定.阵营["蓝"];
		绿出生点.Modulate = 设定.阵营["绿"];

		GetNode<Label>("CanvasLayer2/ColorRect/Label").Text = $"限时 {限时} 年，修为最高者所在的阵营获胜";
		var tween = CreateTween();
		tween.TweenInterval(2);
		tween.TweenProperty(GetNode<Label>("CanvasLayer2/ColorRect/Label"), "modulate:a", 0, 0.5);
		tween.TweenCallback(Callable.From(() => {
			GetNode<ColorRect>("CanvasLayer2/ColorRect").Visible = false;
			红出生点.Freeze = false;
			黄出生点.Freeze = false;
			蓝出生点.Freeze = false;
			绿出生点.Freeze = false;
			GetNode<RigidBody2D>("%机缘点").Freeze = false;

			红出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			黄出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			蓝出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			绿出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");

			GetNode<Timer>("Timer").Start();
			// _thread.Start();
			地图.AddChild(出生("红", 红出生点.GlobalPosition));
			地图.AddChild(出生("黄", 黄出生点.GlobalPosition));
			地图.AddChild(出生("绿", 绿出生点.GlobalPosition));
			地图.AddChild(出生("蓝", 蓝出生点.GlobalPosition));
		}));
	}

	private async void _OnTimerTimeout() {
		月 += 1;
		GetTree().CallGroup("武者", "过月");
		if (月 == 13) {
			月 = 1;
			年 += 1;
			GetTree().CallGroup("武者", "过年");
			if (年 % 5 == 0) {
				地图.AddChild(出生("红", 红出生点.GlobalPosition));
				地图.AddChild(出生("黄", 黄出生点.GlobalPosition));
				地图.AddChild(出生("绿", 绿出生点.GlobalPosition));
				地图.AddChild(出生("蓝", 蓝出生点.GlobalPosition));
			}

			if (年 > 限时) {
				// _t = false;
				红出生点.GetNode<AnimationPlayer>("AnimationPlayer").Stop();
				黄出生点.GetNode<AnimationPlayer>("AnimationPlayer").Stop();
				蓝出生点.GetNode<AnimationPlayer>("AnimationPlayer").Stop();
				绿出生点.GetNode<AnimationPlayer>("AnimationPlayer").Stop();
				GetNode<Timer>("Timer").Stop();

				var arr = new Array<Ball>((Array)GetTree().GetNodesInGroup("武者"));
				var balls = arr.OrderByDescending(a => a.境界).ThenByDescending(a => a.修为 / a.修为上限).ToArray();
				var one = balls[0];
				foreach (var ball in balls) {
					if (ball.身份 != one.身份) {
						ball.死();
					} else {
						ball.Sleeping = true;
						ball.Freeze = true;
					}
				}

				await ToSignal(GetTree().CreateTimer(1.5), Timer.SignalName.Timeout);
				结束(one);
				if (!OS.HasFeature("movie")) return;
				await ToSignal(GetTree().CreateTimer(5), Timer.SignalName.Timeout);
				GetTree().Quit();
				return;
			}
		}

		境界榜.更新();
		杀戮榜.更新();
		资质榜.更新();

		年月.Text = $"{年}年{月}月";
	}

	private void 结束(Ball ball) {
		GetNode<ColorRect>("CanvasLayer2/ColorRect").Visible = true;
		var label = GetNode<Label>("CanvasLayer2/Label");
		label.AddThemeColorOverride("font_color", ball.Body.Modulate);
		label.Text = $"时辰到，{ball.身份}色胜利！\n名字：{
			ball.名字
		}\n境界：{
			ball.境界
		}\n寿命：{
			ball.年龄
		}/{
			ball.寿命
		}\n生命：{
			ball.生命
		}/{
			ball.生命上限
		}\n修为：{
			ball.修为
			:F1}/{
			ball.修为上限
		}\n资质：{
			ball.资质
			:F1}\n伤害：{
			ball.伤害
			:F1}\n杀敌：{
			ball.击杀数
		}";
	}

	public static Ball 出生(string 身份, Vector2 pos, Ball ball = null) {
		ball ??= GlData.Ball.Instantiate<Ball>();
		ball.GlobalPosition = pos;
		ball.名字 = GlData.GetGenerateRandomChineseCharacter();
		ball.身份 = 身份;
		ball.境界 = 设定.境界.武徒;
		ball.年龄 = 0;
		ball.AddToGroup(身份);
		ball.LinearVelocity = new Vector2((float)GD.RandRange(-10.0, 10.0), (float)GD.RandRange(-10.0, 10.0)).Normalized() * 25;
		ball.AddToGroup("武者");
		ball.Ready += () => {
			GlData.MainLog(
				$"[color={
					ball.Body.Modulate.ToHtml()
				}][font_size=21]{
					ball.名字
				}[/font_size][/color] 出生了：境界【{
					ball.境界
				}】寿命【{
					ball.年龄
				}/{
					ball.寿命
				}】生命【{
					ball.生命
				}/{
					ball.生命上限
				}】修为【{
					ball.修为
					:F1}/{
					ball.修为上限
				}】资质【{
					ball.资质
					:F1}】伤害【{
					ball.伤害
					:F1}】");
			ball.BodyEntered += body => {
				if (body is Ball 敌人) {
					if (ball.身份 != 敌人.身份) {
						var 伤害 = Mathf.CeilToInt(10 * ((int)ball.境界 * 1.25 + 1.0) * (1.0 + (ball.境界 - 敌人.境界) * 0.1) * ball.伤害);
						var 敌伤 = Mathf.CeilToInt(10 * ((int)敌人.境界 * 1.25 + 1.0) * (1.0 + (敌人.境界 - ball.境界) * 0.1) * 敌人.伤害);
						var 收获 = Mathf.Snapped(敌人.修为上限 * 0.05 * (1.0 - (int)ball.境界 * 0.05), 0.1);

						敌人.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.生命, -伤害);
						if (ball.生命 > 敌伤 || ball.修为 + 收获 > ball.修为上限) {
							ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, 收获);
						}

						if (敌人.生命 > 0) return;
						敌人.死($"被 [color={
							ball.Body.Modulate.ToHtml()
						}][font_size=21]{
							ball.名字
						}【{
							ball.境界
						}】[/font_size][/color]杀死了");
						
						ball.击杀数++;

						var x = 敌人.修为上限 * 0.1;
						if (ball.境界 <= 敌人.境界) {
							x += 敌人.累计修为 * 0.05;
							x += GD.RandRange(敌人.修为 * 0.7, 敌人.修为);
							x *= 1 + (敌人.境界 - ball.境界) * 0.1;
							ball.EmitSignal(Ball.SignalName.属性事件,
								(int)设定.属性名.资质,
								Mathf.Snapped(Mathf.Max(敌人.资质 * 0.5, 0.1), 0.1));
						} else {
							x += 敌人.累计修为 * 0.05 / (ball.境界 - 敌人.境界 + 1);
							x += (1 / (double)(ball.境界 - 敌人.境界)) * GD.RandRange(敌人.修为 * 0.5, 敌人.修为 * 0.8);
							ball.EmitSignal(Ball.SignalName.属性事件,
								(int)设定.属性名.资质,
								Mathf.Snapped(敌人.资质 * 0.5 / (ball.境界 - 敌人.境界 + 1), 0.1));
						}

						ball.EmitSignal(Ball.SignalName.属性事件, (int)设定.属性名.修为, Mathf.Snapped(x, 0.1));
					}
				}
			};
		};
		return ball;
	}

	private void OnSingletonsOnLog(string text) {
		日志.AppendText(text);
		日志.AppendText("[font_size=6]\n\n[/font_size]");
	}

}