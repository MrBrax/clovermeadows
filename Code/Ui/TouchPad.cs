using System;

namespace vcrossing.Code.Ui;

public partial class TouchPad : Button
{

	[Export] public Control Stick { get; set; }

	private bool _isPressed = false;

	public override void _Ready()
	{
		base._Ready();
		ButtonDown += OnButtonDown;
		ButtonUp += OnButtonUp;
		Stick?.Hide();
	}

	private void OnButtonUp()
	{
		_isPressed = false;
		_startMousePosition = Vector2.Zero;

		Input.ParseInputEvent( new InputEventJoypadMotion() { Axis = JoyAxis.LeftX, AxisValue = 0 } );
		Input.ParseInputEvent( new InputEventJoypadMotion() { Axis = JoyAxis.LeftY, AxisValue = 0 } );

		Stick?.Hide();
	}

	private void OnButtonDown()
	{
		_isPressed = true;
		_startMousePosition = GetLocalMousePosition();

		Stick?.Show();
	}

	private Vector2 _startMousePosition;

	private const float _stickSize = 150f;

	public override void _Process( double delta )
	{
		if ( _isPressed )
		{
			var currentMousePosition = GetLocalMousePosition();
			var diff = currentMousePosition - _startMousePosition;

			// TODO: make these more sensitive
			var x = diff.X / _stickSize;
			var y = diff.Y / _stickSize;

			var stickOffsetX = Math.Clamp( diff.X, -_stickSize, _stickSize );
			var stickOffsetY = Math.Clamp( diff.Y, -_stickSize, _stickSize );

			var stickOffset = new Vector2( stickOffsetX, stickOffsetY );

			Stick.Position = _startMousePosition + stickOffset;

			Input.ParseInputEvent( new InputEventJoypadMotion() { Axis = JoyAxis.LeftX, AxisValue = x } );
			Input.ParseInputEvent( new InputEventJoypadMotion() { Axis = JoyAxis.LeftY, AxisValue = y } );
			/* if ( diff.Length() > 10 )
			{
				if ( Math.Abs( diff.X ) > Math.Abs( diff.Y ) )
				{
					if ( diff.X > 0 )
					{
						Input.ParseInputEvent( new InputEventAction() { Action = "MoveRight", Pressed = true } );
					}
					else
					{
						Input.ParseInputEvent( new InputEventAction() { Action = "MoveLeft", Pressed = true } );
					}
				}
				else
				{
					if ( diff.Y > 0 )
					{
						Input.ParseInputEvent( new InputEventAction() { Action = "MoveDown", Pressed = true } );
					}
					else
					{
						Input.ParseInputEvent( new InputEventAction() { Action = "MoveUp", Pressed = true } );
					}
				}
			} */
		}
	}



}
