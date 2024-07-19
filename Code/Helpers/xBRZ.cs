using System.IO;
using System.Text;

namespace vcrossing.Code.Helpers;

public static class xBRZ
{

	private static byte[] GetColor( byte[] data, int width, int height, int x, int y )
	{
		var i = y * width + x;
		var src = i * 4;
		return new byte[] { data[src], data[src + 1], data[src + 2], data[src + 3] };
	}

	private static void SetColor( byte[] data, int width, int height, int x, int y, byte[] color )
	{
		var i = y * width + x;
		var dst = i * 4;
		data[dst] = color[0];
		data[dst + 1] = color[1];
		data[dst + 2] = color[2];
		data[dst + 3] = color[3];
	}

	public static Image xBRZ4x( Image image, int width, int height )
	{

		var data = image.GetData();
		var newData = new byte[width * height * 4];

		for ( int y = 0; y < height; y++ )
		{
			for ( int x = 0; x < width; x++ )
			{
				var i = y * width + x;

				var src = y * width * 4 + x * 4;
				var dst = y * width * 4 * 4 + x * 4 * 4;

				// Center
				var color = GetColor( data, width, height, x, y );
				SetColor( newData, width * 4, height * 4, x * 4, y * 4, color );

				// Top
				if ( y > 0 )
				{
					color = GetColor( data, width, height, x, y - 1 );
					SetColor( newData, width * 4, height * 4, x * 4, y * 4 - 1, color );
				}

				// Bottom
				if ( y < height - 1 )
				{
					color = GetColor( data, width, height, x, y + 1 );
					SetColor( newData, width * 4, height * 4, x * 4, y * 4 + 1, color );
				}

				// Left
				if ( x > 0 )
				{
					color = GetColor( data, width, height, x - 1, y );
					SetColor( newData, width * 4, height * 4, x * 4 - 1, y * 4, color );
				}

				// Right
				if ( x < width - 1 )
				{
					color = GetColor( data, width, height, x + 1, y );
					SetColor( newData, width * 4, height * 4, x * 4 + 1, y * 4, color );
				}

			}
		}

		var newImage = Image.CreateEmpty( width * 4, height * 4, false, Image.Format.Rgba8 );
		newImage.SetData( width * 4, height * 4, false, Image.Format.Rgba8, newData );

		return newImage;

	}

}
