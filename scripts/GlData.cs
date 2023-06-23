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
		资质
	}
	public struct 属性值 {
		public double? 寿命;
		public double? 年龄;
		public double? 生命上限;
		public double? 生命;
		public double? 修为上限;
		public double? 修为;
		public double? 资质;
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

		武神
	}

	public static readonly Dictionary<string, Color> 阵营 = new() {
		{ "红", new Color(0xff4527ff) },
		{ "黄", new Color(0xffd700ff) },
		{ "蓝", new Color(0x00bfffff) },
		{ "绿", new Color(0x6bde32ff) }
	};

	public static readonly Dictionary<境界, 属性值> 属性 = new() {
		{ 境界.武徒, new 属性值 { 寿命 = 1.0, 生命上限 = 1.0, 修为上限 = 1.0 } },
		{ 境界.炼精化气, new 属性值 { 寿命 = 1.5, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 2.0 } },
		{ 境界.炼气化神, new 属性值 { 寿命 = 1.5, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 2.0 } },
		{ 境界.炼神还虚, new 属性值 { 寿命 = 1.5, 生命上限 = 1.5, 生命 = 1.5, 修为上限 = 2.0 } },
		{ 境界.铜皮铁骨, new 属性值 { 寿命 = 2.0, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 3.0 } },
		{ 境界.毫发不爽, new 属性值 { 寿命 = 2.0, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 3.0 } },
		{ 境界.心领神会, new 属性值 { 寿命 = 2.0, 生命上限 = 2.0, 生命 = 2.0, 修为上限 = 3.0 } },
		{ 境界.滴血重生, new 属性值 { 寿命 = 2.5, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 4.0 } },
		{ 境界.合道同归, new 属性值 { 寿命 = 2.5, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 4.0 } },
		{ 境界.独步乾坤, new 属性值 { 寿命 = 2.5, 生命上限 = 2.5, 生命 = 2.5, 修为上限 = 4.0 } },
		{ 境界.武神, new 属性值 { 寿命 = 3.0, 生命上限 = 3.0, 生命 = 3.0, 修为上限 = 5.0 } }
	};
}

public partial class GlData : Node {
	[Signal]
	public delegate void LogEventHandler(string text);

	public PackedScene Ball;
	public PackedScene Particles;
	public PackedScene Tip;

	public GlData() {
		Singletons = this;
	}

	public static GlData Singletons { get; private set; }

	public override void _Ready() {
		ProcessMode = ProcessModeEnum.Always;
		Ball = GD.Load<PackedScene>("res://scenes/ball.tscn");
		Particles = GD.Load<PackedScene>("res://scenes/particles.tscn");
		Tip = GD.Load<PackedScene>("res://scenes/tip.tscn");
	}

	public override void _Input(InputEvent @event) {
		if (Input.IsActionPressed("ui_accept")) {
			GetTree().Paused = !GetTree().Paused;
		}
	}

	public static void MainLog(string text) {
		Singletons.EmitSignal(SignalName.Log, text);
	}

	public static StringName GetGenerateRandomChineseCharacter() {
		var unicode = (char)GD.RandRange(0x4E00, 0x9FA5 + 1);
		return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in unicode), 1).ToString();
	}

	public static StringName GetAgeGroup(double 年龄, double 寿命) {
		return (年龄 / 寿命) switch {
			< 0.05 => "幼年",
			< 0.15 => "少年",
			< 0.4 => "青年",
			< 0.6 => "中年",
			< 0.8 => "老年",
			< 1.0 => "晚年",
			>= 1.0 => "已死",
			_ => (StringName)"幼年"
		};
	}
}

class CustomComparer : IComparer<Ball>, IComparer<Node> {
	public int Compare(Ball a, Ball b) {
		if ((int)a!.境界 > (int)b!.境界) {
			return -1;
		}

		if ((int)a.境界 < (int)b.境界) {
			return 1;
		}

		return a.修为.CompareTo(b.修为) * -1;
	}

	public int Compare(Node x, Node y) {
		return Compare(x as Ball, y as Ball);
	}
}