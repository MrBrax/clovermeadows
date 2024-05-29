using Godot;

namespace vcrossing2.Code.Helpers;

public static class Logger
{
	public static void Info( string module, string message )
	{
		// GD.Print( FormatContent( $"[{module}] {message}" ) );
		GD.PrintRich( FormatContent( $"[color=green]{module}[/color]\t{message}" ) );
	}

	public static void Info( string message )
	{
		GD.PrintRich( FormatContent( $"[color=red]INFO[/color]\t{message}" ) );
	}

	public static void Info( object message )
	{
		GD.PrintRich( FormatContent( message.ToString() ) );
	}

	public static void LogError( string message )
	{
		GD.PushError( message );
	}

	public static void LogError( string module, string message )
	{
		GD.PushError( FormatContent( $"[{module}] {message}" ) );
	}

	public static void Warn( string message )
	{
		GD.PushWarning( FormatContent( $"[WARN] {message}" ) );
	}

	public static void Warn( string module, string message )
	{
		GD.PushWarning( FormatContent( $"[{module}] {message}" ) );
	}

	public static void LogException( System.Exception e )
	{
		GD.PrintErr( e );
	}

	public static void Debug( string module, string message )
	{
		return;
		GD.Print( $"[{module}] {message}" );
	}

	public static void Debug( string message )
	{
		return;
		GD.Print( message );
	}


	private static string FormatContent( string content, Node? node = null )
	{
		// format timestamp to second with 3 decimal places
		var timestamp = (Time.GetTicksMsec() / 1000f).ToString("0.000");
		if ( node is not null )
		{
			var multiplayer = node.Multiplayer.HasMultiplayerPeer();
			var server = node.Multiplayer.IsServer() && node.IsMultiplayerAuthority();

			/*if (multiplayer)
			{
				string emoji = server ? ServerEmoji : ClientEmoji;
				return $"<{timestamp}> [{emoji}]: {content}";
			} */
		}

		return $"[color=lightblue]{timestamp}[/color] {content}";
	}
}
