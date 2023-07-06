using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;

namespace 球武道.scripts;

public static class 设定 {
	public enum 属性名 {
		寿命,
		年龄,
		生命上限,
		生命,
		修为上限,
		修为,
		资质,
		伤害
	}

	public struct 属性值 {
		public double? 寿命;
		public double? 年龄;
		public double? 生命上限;
		public double? 生命;
		public double? 修为上限;
		public double? 修为;
		public double? 资质;
		public double? 伤害;
	}

	public enum 境界 {
		武徒,

		炼精化气,
		炼气化神,
		炼神还虚,

		铜皮铁骨,
		毫发不爽,
		心领神会,

		滴血重生,
		合道同归,
		独步乾坤,

		武圣
	}

	public static readonly Dictionary<string, Color> 阵营 = new() {
		{ "红", new Color(0xff4527ff) },
		{ "黄", new Color(0xffd700ff) },
		{ "蓝", new Color(0x00bfffff) },
		{ "绿", new Color(0x6bde32ff) }
	};

	public static readonly Dictionary<境界, 属性值> 属性 = new() {
		{ 境界.武徒, new 属性值 { 寿命 = 1.0, 生命上限 = 1.0, 修为上限 = 2.0, 伤害 = 1.0 } },
		{ 境界.炼精化气, new 属性值 { 寿命 = 1.2, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 2.0, 伤害 = 1.1 } },
		{ 境界.炼气化神, new 属性值 { 寿命 = 1.2, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 2.0, 伤害 = 1.1 } },
		{ 境界.炼神还虚, new 属性值 { 寿命 = 1.2, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 3.5, 伤害 = 1.1 } },
		{ 境界.铜皮铁骨, new 属性值 { 寿命 = 1.5, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 3.5, 伤害 = 1.3 } },
		{ 境界.毫发不爽, new 属性值 { 寿命 = 1.5, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 3.5, 伤害 = 1.3 } },
		{ 境界.心领神会, new 属性值 { 寿命 = 1.5, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 5.0, 伤害 = 1.3 } },
		{ 境界.滴血重生, new 属性值 { 寿命 = 1.9, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 5.0, 伤害 = 1.6 } },
		{ 境界.合道同归, new 属性值 { 寿命 = 1.9, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 5.0, 伤害 = 1.6 } },
		{ 境界.独步乾坤, new 属性值 { 寿命 = 1.9, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 7.0, 伤害 = 1.6 } },
		{ 境界.武圣, new 属性值 { 寿命 = 5, 生命上限 = 5.0, 生命 = 5.0, 修为上限 = 10.0, 伤害 = 2.5 } }
	};
}

public partial class GlData : Node {
	[Signal]
	public delegate void LogEventHandler(string text, bool announcement = false);

	private PackedScene _ball;

	public static PackedScene Ball {
		get => Singletons._ball;
	}

	private PackedScene _particles;

	public static PackedScene Particles {
		get => Singletons._particles;
	}

	public static PackedScene Tip;
	public static PackedScene Inherited;

	public static readonly List<Ball> BallPool = new List<Ball>();

	public static Dictionary<设定.境界, bool> 境界组 = new();

	public GlData() {
		Singletons = this;
	}

	public static GlData Singletons { get; private set; }

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;
		_ball = GD.Load<PackedScene>("res://scenes/ball.tscn");
		_particles = GD.Load<PackedScene>("res://scenes/particles.tscn");
		Tip = GD.Load<PackedScene>("res://scenes/tip.tscn");
		Inherited = GD.Load<PackedScene>("res://scenes/传承.tscn");
		if (OS.HasFeature("movie")) {
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
	}

	public override void _Input(InputEvent @event) {
		if (!OS.HasFeature("movie") && Input.IsActionPressed("ui_accept")) {
			GetTree().Paused = !GetTree().Paused;
		}
	}

	public static Ball BallPush() {
		Ball ball;
		if (BallPool.Count < 1) {
			ball = Ball.Instantiate<Ball>();
		} else {
			ball = BallPool[0];
			BallPool.RemoveAt(0);
		}

		ball.SetProcess(true);
		ball.已死 = false;
		ball.境界 = 设定.境界.武徒;
		ball.年龄 = 0;
		ball.SetDeferred("sleeping", false);
		ball.SetDeferred("freeze", false);
		ball.GetNode("CollisionShape2D").SetDeferred("disabled", false);
		return ball;
	}

	public static void BallReturnPoll(Ball ball) {
		BallPool.Add(ball);
		ball.SetProcess(false);
		ball.修为 = 0.0;
		ball.修为上限 = 120;
		ball.生命 = 100;
		ball.生命上限 = 100;
		ball.寿命 = 50;
		ball.年龄 = 0;
		ball.境界 = 设定.境界.武徒;
		ball.击杀数 = 0;
		ball.累计修为 = 0;
	}

	public static void MainLog(string text, bool announcement = false) {
		Singletons.EmitSignal(SignalName.Log, text, announcement);
	}

	public static string GetGenerateRandomChineseCharacter() {
		// var unicode = (char)Random.Shared.Next(0x4E00, 0x9FA5 + 1);
		return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef((char)Random.Shared.Next(0x4E00, 0x9FA5 + 1)), 1).ToString();
	}

	public static string GetAgeGroup(double age, double lifespan) {
		if (age < 0 || lifespan < 0) {
			throw new ArgumentException("年龄和寿命不能为负数");
		}

		if (lifespan == 0) {
			throw new ArgumentException("寿命不能为零");
		}

		return (age / lifespan) switch {
			< 0.05 => "幼年",
			< 0.15 => "少年",
			< 0.4 => "青年",
			< 0.6 => "中年",
			< 0.8 => "老年",
			< 1.0 => "晚年",
			>= 1.0 => "已死",
			_ => "幼年"
		};
	}
}