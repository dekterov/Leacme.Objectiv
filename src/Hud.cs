// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using Godot;
using System;

public class Hud : Node2D {

	private FileDialog filePopup = new FileDialog();
	private MeshInstance displayModel = new MeshInstance();
	private float camrotx = 0.0f;
	private float camrotz = 0.0f;

	private TextureRect vignette = new TextureRect() {
		Expand = true,
		Texture = new GradientTexture() {
			Gradient = new Gradient() { Colors = new[] { Colors.Transparent } }
		},
		Material = new ShaderMaterial() {
			Shader = new Shader() {
				Code = @"
					shader_type canvas_item;
					void fragment() {
						float iRad = 0.3;
						float oRad = 1.0;
						float opac = 0.5;
						vec2 uv = SCREEN_UV;
					    vec2 cent = uv - vec2(0.5);
					    vec4 tex = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0);
					    vec4 col = vec4(1.0);
					    col.rgb *= 1.0 - smoothstep(iRad, oRad, length(cent));
					    col *= tex;
					    col = mix(tex, col, opac);
					    COLOR = col;
					}"
			}
		}
	};

	public override void _Ready() {
		InitVignette();

		var btHolder = new VBoxContainer() { MarginTop = 20, RectMinSize = GetViewportRect().Size };
		AddChild(btHolder);

		using (var dir = new Directory()) {
			//workaround of .obj not being exported even if included
			dir.Copy("res://assets/Alien.md", OS.GetUserDataDir() + "/" + "Alien.obj");
			dir.Copy("res://assets/Rock.md", OS.GetUserDataDir() + "/" + "Rock.obj");
			dir.Copy("res://assets/Cup.md", OS.GetUserDataDir() + "/" + "Cup.obj");

			dir.Copy(OS.GetUserDataDir() + "/" + "Alien.obj", "res://assets/Alien.obj");
			dir.Copy(OS.GetUserDataDir() + "/" + "Rock.obj", "res://assets/Rock.obj");
			dir.Copy(OS.GetUserDataDir() + "/" + "Cup.obj", "res://assets/Cup.obj");
		}

		GetTree().Root.GetNode("Main").CallDeferred("add_child", displayModel);

		Button openObjBt = new Button();
		openObjBt.Text = "Open .obj";
		openObjBt.Connect("pressed", this, nameof(OnOpenButton));
		openObjBt.SizeFlagsHorizontal = (int)Control.SizeFlags.ShrinkCenter;
		openObjBt.RectMinSize = new Vector2(btHolder.RectMinSize.x * 0.7f, 40);
		openObjBt.AddFontOverride("font", new DynamicFont() { FontData = GD.Load<DynamicFontData>("res://assets/default/Tuffy_Bold.ttf"), Size = 30 });
		btHolder.AddChild(openObjBt);

		filePopup.Connect("file_selected", this, nameof(OnFileSelected));
		AddChild(filePopup);

	}

	private void OnFileSelected(String path) {
		// workaround of only displaying added resources in "res://" until able to from "user://"
		displayModel.Mesh = GD.Load<Mesh>("res://assets/" + System.IO.Path.GetFileName(path));
	}

	private void OnOpenButton() {
		filePopup.PopupExclusive = true;

		filePopup.PopupCenteredRatio();
		filePopup.Access = FileDialog.AccessEnum.Userdata;
		filePopup.Mode = FileDialog.ModeEnum.OpenFile;
		filePopup.Filters = new[] { "*.obj ; Wavefront Models" };
		filePopup.CurrentPath = OS.GetUserDataDir() + "/";

	}

	public override void _Draw() {
		DrawBorder(this);
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventScreenDrag sd) {
			camrotx += sd.Relative.x * 0.005f;
			camrotz += sd.Relative.y * 0.005f;
			GetTree().Root.GetNode("Main").GetNode<Spatial>("campivot").Rotation = new Vector3(0, -camrotx, -camrotz);
		}

	}

	private void InitVignette() {
		vignette.RectMinSize = GetViewportRect().Size;
		AddChild(vignette);
		if (Lib.Node.VignetteEnabled) {
			vignette.Show();
		} else {
			vignette.Hide();
		}
	}

	public static void DrawBorder(CanvasItem canvas) {
		if (Lib.Node.BoderEnabled) {
			var vps = canvas.GetViewportRect().Size;
			int thickness = 4;
			var color = new Color(Lib.Node.BorderColorHtmlCode);
			canvas.DrawLine(new Vector2(0, 1), new Vector2(vps.x, 1), color, thickness);
			canvas.DrawLine(new Vector2(1, 0), new Vector2(1, vps.y), color, thickness);
			canvas.DrawLine(new Vector2(vps.x - 1, vps.y), new Vector2(vps.x - 1, 1), color, thickness);
			canvas.DrawLine(new Vector2(vps.x, vps.y - 1), new Vector2(1, vps.y - 1), color, thickness);
		}
	}
}
