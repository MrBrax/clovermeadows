using Godot;

namespace vcrossing.Code.Helpers;

public static class Logger
{
	public static void Info( string module, string message )
	{
		// GD.Print( FormatContent( $"[{module}] {message}" ) );
		GD.PrintRich( FormatContent( module, message ) );
	}

	public static void Info( string message )
	{
		GD.PrintRich( FormatContent( $"INFO/TEXT", message ) );
	}

	public static void Info( object message )
	{
		GD.PrintRich( FormatContent( "INFO/OBJ", message.ToString() ) );
	}

	public static void LogError( string message )
	{
		GD.PushError( message );
	}

	public static void LogError( string module, string message )
	{
		GD.PushError( $"[{module}] {message}" );
	}

	public static void Warn( string message )
	{
		GD.PushWarning( message );
	}

	public static void Warn( string module, string message )
	{
		GD.PushWarning( $"[{module}] {message}" );
	}

	public static void LogException( System.Exception e )
	{
		GD.PrintErr( e );
	}

	public static void Debug( string module, string message )
	{
		/* if ( !OS.IsDebugBuild() )
		{
			return;
		} */

		return;

		GD.Print( $"[{module}] {message}" );
	}

	public static void Debug( string message )
	{
		/* if ( !OS.IsDebugBuild() )
		{
			return;
		} */

		return;

		GD.Print( message );
	}

	public static void Verbose( string module, string message )
	{
		if ( !OS.IsDebugBuild() )
		{
			return;
		}

		GD.Print( $"[{module}] {message}" );
	}

	private static int ModuleColumnWidth = 20;

	private static string FormatContent( string module, string message )
	{
		// format timestamp to second with 3 decimal places
		var timestamp = (Time.GetTicksMsec() / 1000f).ToString( "0.000" );
		/* if ( node is not null )
		{
			var multiplayer = node.Multiplayer.HasMultiplayerPeer();
			var server = node.Multiplayer.IsServer() && node.IsMultiplayerAuthority();

			/*if (multiplayer)
			{
				string emoji = server ? ServerEmoji : ClientEmoji;
				return $"<{timestamp}> [{emoji}]: {content}";
			} *
		} */

		// return $"[color=lightblue]{timestamp}[/color] {content}";

		var formattedContent = $"[color=lightblue]{timestamp}[/color]";

		formattedContent += $" [color=green]${module}[/color]";

		if ( module.Length < ModuleColumnWidth )
		{
			formattedContent += "[color=darkgray]" + new string( '.', ModuleColumnWidth - module.Length ) + "[/color]";
		}

		formattedContent += $"{message}";

		return formattedContent;
	}
}
