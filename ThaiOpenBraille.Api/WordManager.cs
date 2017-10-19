using ICU4NET;
using ICU4NETExtension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ThaiOpenBraille.Api
{
	public class WordManager : IWordManager
	{
		private readonly BrailleTable _table;
		private string _input;
		private readonly ThaiDigitsCompare _thaiDigitCompareTable;

		public WordManager(string input)
		{
			_table = new BrailleTable();
			_thaiDigitCompareTable = new ThaiDigitsCompare();
			_input = input;
		}

		# region Generate Function
		private List<string> WordBreak()
		{
			int quote = 0;
			List<string> prepareOutput = new List<string>();

			using (BreakIterator bi = BreakIterator.CreateWordInstance(Locale.GetUS()))
			{
				Regex pairReplace = new Regex(@"\(\d+,\d+\)");
				var pairReplaceResult = pairReplace.Matches(_input);
				foreach (Match match in pairReplaceResult)
				{
					string treat = match.Value;
					treat = treat.Replace(",", "⠠");
					_input = _input.Remove(match.Index, match.Length).Insert(match.Index, treat);
				}

				Regex expression = new Regex(@"[^\.][^\d+]\.\s");
				var results = expression.Matches(_input);
				foreach (Match match in results)
				{
					_input = Regex.Replace(_input, @"[^\.][^\d+]\.\s", match.Value.Replace(".", "⠸⠲"));
				}
				_input = Regex.Replace(_input, @"\.{3,}", "⠄⠄⠄");

				bi.SetText(_input);
				IEnumerable<String> spWord = bi.Enumerate();
				for (int i = 0; i < spWord.Count(); i++)
				{
					string spwordCheckCapital = spWord.ElementAt(i);
					//check wheather sentense capitalize or CAPS whole word
					if (Regex.IsMatch(spwordCheckCapital, @"[A-Z]+[A-Z]"))
					{
						spwordCheckCapital = "⠠⠠" + spwordCheckCapital;
					}
					else if (Char.IsUpper(spwordCheckCapital[0]))
					{
						spwordCheckCapital = "⠠" + spwordCheckCapital;
					}

					string vowelLiftFilter = spwordCheckCapital;

					expression = new Regex(@"\)|\?|[^\d]\,|\!|\:|;");
					results = expression.Matches(vowelLiftFilter);
					foreach (Match match in results)
					{
						//Check Thai or English
						Regex checkTHOrEN = new Regex(@"[\u0080-\u9fff]+");
						for (var j = i - 1; j >= 0; j--)
						{
							if (spWord.ElementAt(j).Equals(" ")) continue;
							Match m = checkTHOrEN.Match(spWord.ElementAt(j));
							if (m.Success)
							{
								vowelLiftFilter = GetBrailleInTable("T" + vowelLiftFilter);
								break;
							}
							vowelLiftFilter = GetBrailleInTable("E" + vowelLiftFilter);
							break;
						}
					}

					expression = new Regex(@"\(");
					results = expression.Matches(vowelLiftFilter);
					foreach (Match match in results)
					{
						Regex checkTHOrEN = new Regex(@"[\u0080-\u9fff]+");
						for (var j = i + 1; j <= spWord.Count(); j++)
						{
							if (spWord.ElementAt(j).Equals(" ")) continue;
							Match m = checkTHOrEN.Match(spWord.ElementAt(j));
							if (m.Success)
							{
								vowelLiftFilter = GetBrailleInTable("T" + vowelLiftFilter);
								break;
							}
							vowelLiftFilter = GetBrailleInTable("E" + vowelLiftFilter);
							break;
						}
					}

					string bt = SeparateVowel(vowelLiftFilter);
					// Detect double quote
					if (vowelLiftFilter.Equals("\""))
					{
						bt = quote % 2 == 0 ? "⠦" : "⠴";
						quote++;
					}
					prepareOutput.Add(bt);
				}
			}
			return prepareOutput;
		}

		private string SeparateVowel(string word)
		{
			if (word == "เทอญ" || word == "เทอม")
			{
				word = GetBrailleInTable(word);
			}

			//สระเอียะ
			Regex rgxEx = new Regex(@"\s*เ\p{L}{1,2}\p{M}+ยะ");
			var match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*เ|ื|ยะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex lift = new Regex(@"[^่้๊็๋]");
				String prepareL = lift.Replace(prepareF, "");
				Regex plain = new Regex(@"[่้๊็๋]");
				String prepareP = plain.Replace(prepareF, "");
				word = word.Replace(prepareB, prepareP + GetBrailleInTable("เอือ") + prepareL);
			}

			//สระเอีย
			rgxEx = new Regex(@"เ\p{L}+ี\p{M}*ย");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*เ|ี|ย");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex lift = new Regex(@"[^่้๊็๋]");
				String prepareL = lift.Replace(prepareF, "");
				Regex plain = new Regex(@"[่้๊็๋]");
				String prepareP = plain.Replace(prepareF, "");
				word = word.Replace(prepareB, prepareP + GetBrailleInTable("เอีย") + prepareL);
			}

			//สระเอือ
			rgxEx = new Regex(@"\s*เ\p{L}{1,2}\p{M}+อ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*เ|ื|อ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex lift = new Regex(@"[^่้๊็๋]");
				String prepareL = lift.Replace(prepareF, "");
				Regex plain = new Regex(@"[่้๊็๋]");
				String prepareP = plain.Replace(prepareF, "");
				word = word.Replace(prepareB, prepareP + GetBrailleInTable("เอือ") + prepareL);
			}

			//สระเอาะ
			rgxEx = new Regex(@"\s*เ\p{L}{1,2}\p{M}*าะ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*เ|าะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex lift = new Regex(@"[^่้๊็๋]");
				String prepareL = lift.Replace(prepareF, "");
				Regex plain = new Regex(@"[่้๊็๋]");
				String prepareP = plain.Replace(prepareF, "");
				word = word.Replace(prepareB, prepareP + GetBrailleInTable("เอาะ") + prepareL);
			}

			//สระเอา
			rgxEx = new Regex(@"เ\p{L}+\p{M}*า");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*เ|า");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex lift = new Regex(@"[^่้๊็๋]");
				String prepareL = lift.Replace(prepareF, "");
				Regex plain = new Regex(@"[่้๊็๋]");
				String prepareP = plain.Replace(prepareF, "");
				word = word.Replace(prepareB, prepareP + GetBrailleInTable("เอา") + prepareL);
			}

			//สระอัว
			rgxEx = new Regex(@"\p{L}+ั\p{M}*ว");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*ัว");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("อัว"));
			}

			//สระอำ
			rgxEx = new Regex(@"\p{L}+\p{M}*ำ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"\s*ำ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("ำ"));
			}

			//สระเออะ
			rgxEx = new Regex(@"เ[^\p{M}*]อะ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"เ|อะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("เออ") + GetBrailleInTable("ะ"));
			}

			//สระเอิอ
			rgxEx = new Regex(@"\s*เ\p{L}{1,2}ิ\w");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"เ|ิ\w+");
				String prepareF = rgxRemove.Replace(prepareB, "");
				Regex rgxRemoveU = new Regex(@"\s*เ\p{L}{1,2}ิ");
				String lastF = rgxRemoveU.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("เออ") + lastF);
			}

			//สระเออ
			rgxEx = new Regex(@"เ\p{L}+\p{M}*อ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{

				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"เ|\p{M}*อ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("เออ"));
			}

			//สระเอะ
			rgxEx = new Regex(@"เ[^\p{M}*]ะ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"เ|ะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("เ") + GetBrailleInTable("ะ"));
			}

			//สระแอะ
			rgxEx = new Regex(@"แ[^\p{M}*]ะ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"แ|ะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("แ") + GetBrailleInTable("ะ"));
			}

			//สระโอะ
			rgxEx = new Regex(@"โ\p{L}+\p{M}*ะ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				Regex rgxRemove = new Regex(@"โ|ะ");
				String prepareF = rgxRemove.Replace(prepareB, "");
				word = word.Replace(prepareB, prepareF + GetBrailleInTable("โ") + GetBrailleInTable("ะ"));
			}

			//นิคหิต
			rgxEx = new Regex(@"\p{L}+ํ");
			match = rgxEx.Matches(word);
			foreach (Match m in match)
			{
				String prepareB = m.Value;
				int foundIdx = prepareB.IndexOf('ํ');
				String prepareF = prepareB.Remove(foundIdx, 1);
				prepareF = prepareF.Insert(foundIdx - 1, "ํ");
				word = prepareF;
			}

			string numberStyle = "⠼";
			if (_thaiDigitCompareTable.numDict.Keys.Contains(word[0]))
			{
				word = ParseThaiNum(word).ToString();
				numberStyle = "⠠⠼";
			}

			double n;
			bool isNumeric = double.TryParse(word.Length > 1 ? word.Replace(",", "") : word, out n);
			word = isNumeric ? numberStyle + (word.Contains(",") ? String.Format("{0:n0}", n).Replace(",", "⠂") : word) : word;
			if (word.Equals("ฯลฯ"))
			{
				return GetBrailleInTable("ฯลฯ");
			}

			String converted = "";
			foreach (Char element in word)
			{
				converted += GetBrailleInTable(element.ToString());
			}
			return converted;
		}

		private string GetBrailleInTable(string ret)
		{
			try
			{
				ret = _table.AllBrailleTable[ret];
			}
			catch (KeyNotFoundException)
			{
			}

			return ret;
		}

		private double ParseThaiNum(string thainum)
		{
			double num = 0;
			string temp = "";
			for (var i = 0; i < thainum.Length; i++)
			{
				try
				{
					temp += _thaiDigitCompareTable.numDict[thainum[i]];
				}
				catch (KeyNotFoundException) { temp += thainum[i]; };
			}
			double.TryParse(temp, out num);
			return num;
		}
		#endregion

		public string Output()
		{
			return string.Join("\n", WordBreak());
		}
	}
}
