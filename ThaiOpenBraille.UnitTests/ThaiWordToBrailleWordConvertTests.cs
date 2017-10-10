using ThaiOpenBraille.Api;
using Xunit;

namespace ThaiOpenBraille.UnitTests
{
	public class ThaiWordToBrailleWordConvertTests
	{
		[Fact]
		public void Convert_มด_Word_To_Braille_Word_Test()
		{
			var input = "กด";
			var expect = @"⠛⠙";
			IWordManager converter = new WordManager(input);
			Assert.Equal(expect, converter.Output());
		}

		[Fact]
		public void Convert_ทด_Word_To_Braille_Word_Test()
		{
			var input = "ทด";
			var expect = @"⠾⠙";
			IWordManager converter = new WordManager(input);
			Assert.Equal(expect, converter.Output());
		}

		[Fact]
		public void Convert_กด_Word_To_Braille_Word_Test()
		{
			var input = "กด";
			var expect = @"⠛⠙";
			IWordManager converter = new WordManager(input);
			Assert.Equal(expect, converter.Output());
		}

		[Fact]
		public void Convert_คิดถึง_Word_To_Braille_Word_Test()
		{
			var input = "คิดถึง";
			var expect = @"⠥⠃⠙⠞⠪⠻";
			IWordManager converter = new WordManager(input);
			Assert.Equal(expect, converter.Output());
		}
	}
}
