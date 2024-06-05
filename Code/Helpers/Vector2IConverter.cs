namespace vcrossing.Code.Helpers;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public class Vector2IConverter : JsonConverter<Vector2I>
{
	public override Vector2I Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		if ( reader.TokenType != JsonTokenType.StartObject )
		{
			throw new JsonException();
		}

		int x = 0;
		int y = 0;

		while ( reader.Read() )
		{
			if ( reader.TokenType == JsonTokenType.EndObject )
			{
				return new Vector2I( x, y );
			}

			if ( reader.TokenType != JsonTokenType.PropertyName )
			{
				throw new JsonException();
			}

			string propertyName = reader.GetString();
			reader.Read();

			switch ( propertyName )
			{
				case "x":
					x = reader.GetInt32();
					break;
				case "y":
					y = reader.GetInt32();
					break;
				default:
					throw new JsonException();
			}
		}

		throw new JsonException();
	}

	public override void Write( Utf8JsonWriter writer, Vector2I value, JsonSerializerOptions options )
	{
		writer.WriteStartObject();
		writer.WriteNumber( "x", value.X );
		writer.WriteNumber( "y", value.Y );
		writer.WriteEndObject();
	}
}
