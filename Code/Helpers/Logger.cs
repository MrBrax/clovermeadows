using Godot;

namespace vcrossing2.Code.Helpers;

public static class Logger
{
	
	public static void Info( string module, string message )
	{
		GD.Print( $"[{module}] {message}" );
	}
	
	public static void LogError( string message )
	{
		GD.PushError( message );
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
		return;
		GD.Print( $"[{module}] {message}" );
	}
	
}
