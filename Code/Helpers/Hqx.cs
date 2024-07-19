using Godot;
using System;

namespace vcrossing.Code.Helpers;

public class HqxScaler
{
	private const int MASK_DIF = 1;
	private const int MASK_SAME = 0;

	public static Image Upscale( Image srcImage, int factor )
	{
		if ( factor != 2 )
		{
			throw new ArgumentException( "Only hq2x scaling factor is implemented." );
		}

		// Get the original image data
		int width = srcImage.GetWidth();
		int height = srcImage.GetHeight();
		// srcImage.Lock();

		// Create a new image with the upscaled size
		Image dstImage = Image.CreateEmpty( width * factor, height * factor, false, srcImage.GetFormat() );

		// Arrays for storing pixels and output pixels
		Color[] inputPixels = new Color[width * height];
		Color[] outputPixels = new Color[width * height * 4];

		// Read pixels into input array
		for ( int y = 0; y < height; y++ )
		{
			for ( int x = 0; x < width; x++ )
			{
				inputPixels[y * width + x] = srcImage.GetPixel( x, y );
			}
		}

		// Apply hq2x scaling
		for ( int y = 0; y < height; y++ )
		{
			for ( int x = 0; x < width; x++ )
			{
				ApplyHq2x( x, y, width, height, inputPixels, outputPixels );
			}
		}

		// Write pixels to destination image
		for ( int y = 0; y < height * 2; y++ )
		{
			for ( int x = 0; x < width * 2; x++ )
			{
				dstImage.SetPixel( x, y, outputPixels[y * width * 2 + x] );
			}
		}

		return dstImage;
	}

	private static void ApplyHq2x( int x, int y, int width, int height, Color[] inputPixels, Color[] outputPixels )
	{
		Color[] c = new Color[9];
		int[] pattern = new int[9];

		for ( int i = 0; i < 9; i++ )
		{
			int nx = x + (i % 3) - 1;
			int ny = y + (i / 3) - 1;

			if ( nx < 0 || nx >= width || ny < 0 || ny >= height )
			{
				c[i] = inputPixels[y * width + x];
			}
			else
			{
				c[i] = inputPixels[ny * width + nx];
			}

			pattern[i] = (c[i] == c[4]) ? MASK_SAME : MASK_DIF;
		}

		Color p1, p2, p3, p4;
		if ( pattern[1] != MASK_DIF && pattern[3] != MASK_DIF )
		{
			p1 = c[4];
		}
		else
		{
			p1 = c[3];
		}

		if ( pattern[1] != MASK_DIF && pattern[5] != MASK_DIF )
		{
			p2 = c[4];
		}
		else
		{
			p2 = c[5];
		}

		if ( pattern[3] != MASK_DIF && pattern[7] != MASK_DIF )
		{
			p3 = c[4];
		}
		else
		{
			p3 = c[7];
		}

		if ( pattern[5] != MASK_DIF && pattern[7] != MASK_DIF )
		{
			p4 = c[4];
		}
		else
		{
			p4 = c[5];
		}

		int index = y * 2 * width * 2 + x * 2;
		outputPixels[index] = p1;
		outputPixels[index + 1] = p2;
		outputPixels[index + width * 2] = p3;
		outputPixels[index + width * 2 + 1] = p4;
	}
}
