using System.IO;
using System.Text;

namespace vcrossing.Code.Helpers.ACNL;

// https://github.com/Thulinma/ACNLPatternTool/blob/master/src/libs/ACNLFormat.js


//ACNL data layout.
//
//QR codes are blocks of 540 bytes (normal) or 620 bytes (pro) each, providing this data in sequence:
//
//0x 00 - 0x 29 ( 42) = Pattern Title (21 chars)
//0x 2A - 0x 2B (  2) = User ID
//0x 2C - 0x 3D ( 18) = User Name (9 chars)
//0x 3E         (  1) = Gender
//0x 3F         (  1) = Zero padding(?)
//0x 40 - 0x 41 (  2) = Town ID
//0x 42 - 0x 53 ( 18) = Town Name (9 chars)
//0x 54         (  1) = Language
//0x 55         (  1) = Zero padding(?)
//0x 56         (  1) = Country
//0x 57         (  1) = Region
//0x 58 - 0x 66 ( 15) = Color code indexes
//0x 67         (  1) = "color" (probably a lookup for most prevalent color?)
//0x 68         (  1) = "looks" (probably a lookup for "quality"? Seems to always be 0x0A or 0x00)
//0x 69         (  1) = Pattern type (see below)
//0x 6A - 0x 6B (  2) = Zero padding(?)
//0x 6C - 0x26B (512) = Pattern Data 1 (mandatory)
//0x26C - 0x46B (512) = Pattern Data 2 (optional)
//0x46C - 0x66B (512) = Pattern Data 3 (optional)
//0x66C - 0x86B (512) = Pattern Data 4 (optional)
//0x86C - 0x86F (  4) = Zero padding
//
// Pattern types:
// 0x00 = Fullsleeve dress (pro)
// 0x01 = Halfsleeve dress (pro)
// 0x02 = Sleeveless dress (pro)
// 0x03 = Fullsleeve shirt (pro)
// 0x04 = Halfsleeve shirt (pro)
// 0x05 = Sleeveless shirt (pro)
// 0x06 = Hat with horns
// 0x07 = Hat
// 0x08 = Standee (pro)
// 0x09 = Plain pattern (easel)
// 0x0A = unknown (non-pro)
// 0x0B = unknown (non-pro)


class ACNLFormat
{

	public enum PatternType : byte
	{
		FullsleeveDress = 0x00,
		HalfsleeveDress = 0x01,
		SleevelessDress = 0x02,
		FullsleeveShirt = 0x03,
		HalfsleeveShirt = 0x04,
		SleevelessShirt = 0x05,
		HatWithHorns = 0x06,
		Hat = 0x07,
		Standee = 0x08,
		PlainPattern = 0x09,
		Unknown1 = 0x0A,
		Unknown2 = 0x0B
	}

	public string PatternTitle { get; set; }
	public short UserID;
	public string UserName;
	public byte Gender;
	public short TownID;
	public string TownName;
	public byte Language;
	public byte Country;
	public byte Region;
	public byte[] ColorCodeIndexes;
	public byte TColor;
	public byte Looks;
	public PatternType Type;
	public int TextureSize;

	public byte[] PatternData1;
	public byte[] PatternData2;
	public byte[] PatternData3;
	public byte[] PatternData4;

	/* public ACNLFormat( Stream stream )
	{
		var reader = new BinaryReader( stream );

		PatternTitle = Encoding.UTF8.GetString( reader.ReadBytes( 42 ) ).TrimEnd( '\0' );
		Creator = Encoding.UTF8.GetString( reader.ReadBytes( 18 ) ).TrimEnd( '\0' );
		TownName = Encoding.UTF8.GetString( reader.ReadBytes( 18 ) ).TrimEnd( '\0' );

		reader.Close();

		GD.Print( $"Pattern Title: {PatternTitle}" );
		GD.Print( $"Creator: {Creator}" );
		GD.Print( $"Town Name: {TownName}" );

	} */

	public ACNLFormat( byte[] bytes )
	{
		var reader = new BinaryReader( new MemoryStream( bytes ) );

		PatternTitle = Encoding.Unicode.GetString( reader.ReadBytes( 42 ) );
		UserID = reader.ReadInt16();
		UserName = Encoding.Unicode.GetString( reader.ReadBytes( 18 ) );
		Gender = reader.ReadByte();
		reader.ReadByte();
		TownID = reader.ReadInt16();
		TownName = Encoding.Unicode.GetString( reader.ReadBytes( 18 ) );
		Language = reader.ReadByte();
		reader.ReadByte();
		Country = reader.ReadByte();
		Region = reader.ReadByte();
		ColorCodeIndexes = reader.ReadBytes( 15 );
		TColor = reader.ReadByte();
		Looks = reader.ReadByte();
		Type = (PatternType)reader.ReadByte();
		reader.ReadBytes( 2 );
		PatternData1 = reader.ReadBytes( 512 );
		// PatternData2 = reader.ReadBytes( 512 );

		reader.Close();

		GD.Print( $"Pattern Title: {PatternTitle}" );
		GD.Print( $"User ID: {UserID}" );
		GD.Print( $"User Name: {UserName}" );

	}

	private static string[] paletteColors = [
		//Back in June 2013 when I first made this list, I worked off of a heavily post-processed PHOTOGRAPH of the 3DS screen.
		//Now, in 2020, we have emulators. I took an oversampled screenshot of the editor screen and went through and fixed all these colors by hand.
		//Now, my question to everyone else: why did you all just copy my wrong list of colors, instead of grabbing an emulator and finding the correct values...? Come on guys, put some work in! -_-
		//Pinks (0x00 - 0x08)
		"#FFEEFF", "#FF99AA", "#EE5599", "#FF66AA", "#FF0066", "#BB4477", "#CC0055", "#990033", "#552233",
		"","","","","","",//0x09-0x0E unused / unknown
		"#FFFFFF", //0x0F: Grey 1
		//Reds (0x10 - 0x18)
		"#FFBBCC", "#FF7777", "#DD3210", "#FF5544", "#FF0000", "#CC6666", "#BB4444", "#BB0000", "#882222",
		"","","","","","",//0x19-0x1E unused / unknown
		"#EEEEEE", //0x1F: Grey 2
		//Oranges (0x20 - 0x28)
		"#DDCDBB", "#FFCD66", "#DD6622", "#FFAA22", "#FF6600", "#BB8855", "#DD4400", "#BB4400", "#663210",
		"","","","","","",//0x29-0x2E unused / unknown
		"#DDDDDD", //0x2F: Grey 3
		//Pastels or something, I guess? (0x30 - 0x38)
		"#FFEEDD", "#FFDDCC", "#FFCDAA", "#FFBB88", "#FFAA88", "#DD8866", "#BB6644", "#995533", "#884422",
		"","","","","","",//0x39-0x3E unused / unknown
		"#CCCDCC", //0x3F: Grey 4
		//Purple (0x40 - 0x48)
		"#FFCDFF", "#EE88FF", "#CC66DD", "#BB88CC", "#CC00FF", "#996699", "#8800AA", "#550077", "#330044",
		"","","","","","",//0x49-0x4E unused / unknown
		"#BBBBBB", //0x4F: Grey 5
		//Pink (0x50 - 0x58)
		"#FFBBFF", "#FF99FF", "#DD22BB", "#FF55EE", "#FF00CC", "#885577", "#BB0099", "#880066", "#550044",
		"","","","","","",//0x59-0x5E unused / unknown
		"#AAAAAA", //0x5F: Grey 6
		//Brown (0x60 - 0x68)
		"#DDBB99", "#CCAA77", "#774433", "#AA7744", "#993200", "#773222", "#552200", "#331000", "#221000",
		"","","","","","",//0x69-0x6E unused / unknown
		"#999999", //0x6F: Grey 7
		//Yellow (0x70 - 0x78)
		"#FFFFCC", "#FFFF77", "#DDDD22", "#FFFF00", "#FFDD00", "#CCAA00", "#999900", "#887700", "#555500",
		"","","","","","",//0x79-0x7E unused / unknown
		"#888888", //0x7F: Grey 8
		//Blue (0x80 - 0x88)
		"#DDBBFF", "#BB99EE", "#6632CC", "#9955FF", "#6600FF", "#554488", "#440099", "#220066", "#221033",
		"","","","","","",//0x89-0x8E unused / unknown
		"#777777", //0x8F: Grey 9
		//Ehm... also blue? (0x90 - 0x98)
		"#BBBBFF", "#8899FF", "#3332AA", "#3355EE", "#0000FF", "#333288", "#0000AA", "#101066", "#000022",
		"","","","","","",//0x99-0x9E unused / unknown
		"#666666", //0x9F: Grey 10
		//Green (0xA0 - 0xA8)
		"#99EEBB", "#66CD77", "#226610", "#44AA33", "#008833", "#557755", "#225500", "#103222", "#002210",
		"","","","","","",//0xA9-0xAE unused / unknown
		"#555555", //0xAF: Grey 11
		//Icky greenish yellow (0xB0 - 0xB8)
		"#DDFFBB", "#CCFF88", "#88AA55", "#AADD88", "#88FF00", "#AABB99", "#66BB00", "#559900", "#336600",
		"","","","","","",//0xB9-0xBE unused / unknown
		"#444444", //0xBF: Grey 12
		//Wtf? More blue? (0xC0 - 0xC8)
		"#BBDDFF", "#77CDFF", "#335599", "#6699FF", "#1077FF", "#4477AA", "#224477", "#002277", "#001044",
		"","","","","","",//0xC9-0xCE unused / unknown
		"#333233", //0xCF: Grey 13
		//Gonna call this cyan (0xD0 - 0xD8)
		"#AAFFFF", "#55FFFF", "#0088BB", "#55BBCC", "#00CDFF", "#4499AA", "#006688", "#004455", "#002233",
		"","","","","","",//0xD9-0xDE unused / unknown
		"#222222", //0xDF: Grey 14
		//More cyan, because we didn't have enough blue-like colors yet (0xE0 - 0xE8)
		"#CCFFEE", "#AAEEDD", "#33CDAA", "#55EEBB", "#00FFCC", "#77AAAA", "#00AA99", "#008877", "#004433",
		"","","","","","",//0xE9-0xEE unused / unknown
		"#000000", //0xEF: Grey 15
		//Also green. ---- it, whatever. (0xF0 - 0xF8)
		"#AAFFAA", "#77FF77", "#66DD44", "#00FF00", "#22DD22", "#55BB55", "#00BB00", "#008800", "#224422",
		"","","","","","",//0xF9-0xFE unused / unknown
		"", //0xFF unused (white in-game, editing freezes the game)
	];

	private Color GetPaletteColor( byte value )
	{

		if ( value >= ColorCodeIndexes.Length )
		{
			// GD.PushWarning( $"Invalid color code index: {value} (max: {ColorCodeIndexes.Length})" );
			return new Color( 0, 0, 0, 0 );
		}

		// TODO: Check if this is correct
		if ( value == 0x15 )
		{
			return new Color( 0, 0, 0, 0 );
		}

		var paletteIndex = ColorCodeIndexes[value];

		return Color.FromHtml( paletteColors[paletteIndex] );

	}

	public Image GetImage()
	{
		var image = Image.CreateEmpty( 32, 32, false, Image.Format.Rgba8 );

		// image.Lock();
		/* var data = image.GetData();
		for ( int i = 0; i < 512; i++ )
		{
			var color = ColorCodeIndexes[PatternData1[i]];
			data[i * 4 + 0] = color;
			data[i * 4 + 1] = color;
			data[i * 4 + 2] = color;
			data[i * 4 + 3] = 255;
		}

		image.SetPixel( 0, 0, new Color( 1, 0, 0, 1 ) ); */

		/* for ( int y = 0; y < 32; y++ )
		{
			for ( int x = 0; x < 32; x++ )
			{
				GD.Print( $"x: {x}, y: {y}" );
				var color = GetPaletteColor( PatternData1[y * 32 + x] );
				image.SetPixel( x, y, color );
			}
		} */

		// byte[] data = new byte[512];

		var pixel = 0;

		for ( int i = 0; i < 512; i++ )
		{
			// var firstColor = GetPaletteColor( PatternData1[i] );
			// image.SetPixel( i % 32, i / 32, firstColor );
			// data[pixel++] = (byte)(PatternData1[i] & 0x0F);
			// data[pixel++] = (byte)((PatternData1[i] >> 4) & 0x0F);

			var color1 = GetPaletteColor( (byte)(PatternData1[i] & 0x0F) );
			image.SetPixel( pixel % 32, pixel / 32, color1 );
			pixel++;

			// GD.Print( $"Pixel: {pixel} - Color: {PatternData1[i] & 0x0F}" );

			var color2 = GetPaletteColor( (byte)((PatternData1[i] >> 4) & 0x0F) );
			image.SetPixel( pixel % 32, pixel / 32, color2 );
			pixel++;

			// GD.Print( $"Pixel: {pixel} - Color: {(PatternData1[i] >> 4) & 0x0F}" );

		}

		return image;
	}

}
