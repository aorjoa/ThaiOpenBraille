using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiOpenBraille.Api
{
    public class ThaiDigitsCompare
    {
		//Parse Thai Number (Digits)
		public Dictionary<char, int> numDict = new Dictionary<char, int>() { {'๐',0}, { '๑', 1 }, { '๒', 2 },
			{ '๓', 3 }, { '๔', 4 }, { '๕', 5 },
			{'๖',6},{'๗',7},{'๘',8},{'๙',9}};
	}
}
