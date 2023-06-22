using Godot;

namespace 球武道.scripts;

public partial class Main : Node2D {
	private int _限时 = 100 * 10;
	private PackedScene _ball;

	public CanvasGroup 地图;
	public RigidBody2D 红出生点;
	public RigidBody2D 黄出生点;
	public 境界榜 境界榜;
	public RigidBody2D 蓝出生点;
	public RigidBody2D 绿出生点;
	public int 年;

	public Label 年月;
	public RichTextLabel 日志;
	public VBoxContainer 杀戮榜;
	public int 月 = 1;


	public override void _Ready() {
		GlData.Singletons.Log += OnSingletonsOnLog;

		_ball = GD.Load<PackedScene>("res://scenes/ball.tscn");

		年月 = GetNode<Label>("%年月");
		日志 = GetNode<RichTextLabel>("%日志");
		境界榜 = GetNode<境界榜>("%境界榜");
		杀戮榜 = GetNode<VBoxContainer>("%杀戮榜");
		地图 = GetNode<CanvasGroup>("%地图");
		红出生点 = GetNode<RigidBody2D>("%红出生点");
		黄出生点 = GetNode<RigidBody2D>("%黄出生点");
		蓝出生点 = GetNode<RigidBody2D>("%蓝出生点");
		绿出生点 = GetNode<RigidBody2D>("%绿出生点");

		红出生点.Modulate = 设定.阵营["红"];
		黄出生点.Modulate = 设定.阵营["黄"];
		蓝出生点.Modulate = 设定.阵营["蓝"];
		绿出生点.Modulate = 设定.阵营["绿"];

		GetNode<Label>("CanvasLayer2/ColorRect/Label").Text = $"限时 {_限时} 年，修为最高者所在的阵营获胜";
		var tween = CreateTween();
		tween.TweenInterval(2);
		tween.TweenProperty(GetNode<Label>("CanvasLayer2/ColorRect/Label"), "modulate:a", 0, 0.5);
		tween.TweenCallback(Callable.From(() => {
			GetNode<ColorRect>("CanvasLayer2/ColorRect").Visible = false;
			红出生点.Freeze = false;
			黄出生点.Freeze = false;
			蓝出生点.Freeze = false;
			绿出生点.Freeze = false;

			红出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			黄出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			蓝出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");
			绿出生点.GetNode<AnimationPlayer>("AnimationPlayer").Play("闪烁");

			GetNode<Timer>("Timer").Start();
		}));
	}

	private void _OnTimerTimeout() {
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
		}

		境界榜.更新();
		年月.Text = $"{年}年{月}月";
	}

	public Ball 出生(string 身份, Vector2 pos) {
		var ball = _ball.Instantiate<Ball>();
		ball.GlobalPosition = pos;
		ball.名字 = GlData.GetGenerateRandomChineseCharacter();
		ball.身份 = 身份;
		ball.LinearVelocity = new Vector2((float)GD.RandRange(-10.0, 10.0), (float)GD.RandRange(-10.0, 10.0)).Normalized() * 25;
		ball.AddToGroup("武者");
		ball.Ready += () => {
			ball.境界 = 设定.境界.武徒;
			ball.年龄 = 0;
		};
		return ball;
	}

	private void OnSingletonsOnLog(string text) {
		日志.AppendText(text);
		日志.AppendText("[font_size=6]\n\n[/font_size]");
	}
}