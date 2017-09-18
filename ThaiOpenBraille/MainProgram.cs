using ICU4NET;
using ICU4NETExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThaiOpenBraille
{
    public delegate void OpenFormDelegate();
    public partial class MainProgram : Form
    {

        private Processing loadingForm = new Processing();

        public MainProgram()
        {
            InitializeComponent();
        }

        List<String> prepareOutput = new List<string>();
        static BrailleTable table = new BrailleTable();
        Boolean inputChanged = false;

        private void translateButton_Click(object sender, EventArgs e)
        {
            if (inputChanged && inputTextBox.Text.Length > 0)
            {
                Thread loadinThread = new Thread(new ThreadStart(loadingFunction));
                Thread translateThread = new Thread(new ThreadStart(translateFunction));
                loadinThread.IsBackground = true;
                translateThread.IsBackground = true;
                loadinThread.Start();
                translateThread.Start();
            }
        }

        private void translateFunction()
        {
            this.BeginInvoke(new OpenFormDelegate(doTranslate));
        }
        private void loadingFunction()
        {
            try
            {
                loadingForm.ShowDialog();
            }
            catch (Exception ex)
            {
                loadingForm.Hide();
            }
        }

        private void doTranslate()
        {
            //Clear data.
            outputTextBox.Clear();
            prepareOutput.Clear();
            wordBreak();
            outputTextBox.Text = string.Join("\n", prepareOutput);
            inputChanged = false;
            loadingForm.Invoke((MethodInvoker)(() => loadingForm.Hide()));
        }

        private void wordBreak()
        {
            int quote = 0;

            using (BreakIterator bi = BreakIterator.CreateWordInstance(Locale.GetUS()))
            {
                var input = inputTextBox.Text;

                Regex pairReplace = new Regex(@"\(\d+,\d+\)");
                var pairReplaceResult = pairReplace.Matches(inputTextBox.Text);
                foreach (Match match in pairReplaceResult)
                {
                    string treat = match.Value;
                    treat = treat.Replace(",", "⠠");
                    input = input.Remove(match.Index, match.Length).Insert(match.Index, treat);
                }

                Regex expression = new Regex(@"[^\.][^\d+]\.\s");
                var results = expression.Matches(input);
                foreach (Match match in results)
                {
                    input = Regex.Replace(input, @"[^\.][^\d+]\.\s", match.Value.Replace(".", "⠸⠲"));
                }
                input = Regex.Replace(input, @"\.{3,}", "⠄⠄⠄");

                bi.SetText(input);
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
                                vowelLiftFilter = getBrailleInTable("T" + vowelLiftFilter);
                                break;
                            }
                            vowelLiftFilter = getBrailleInTable("E" + vowelLiftFilter);
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
                                vowelLiftFilter = getBrailleInTable("T" + vowelLiftFilter);
                                break;
                            }
                            vowelLiftFilter = getBrailleInTable("E" + vowelLiftFilter);
                            break;
                        }
                    }

                    string bt = separateVowel(vowelLiftFilter);
                    // Detect double quote
                    if (vowelLiftFilter.Equals("\""))
                    {
                        bt = quote % 2 == 0 ? "⠦" : "⠴";
                        quote++;
                    }
                    prepareOutput.Add(bt);
                }
            }
        }
        
        private String separateVowel(String word)
        {
            if (word == "เทอญ" || word == "เทอม")
            {
                word = getBrailleInTable(word);
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
                word = word.Replace(prepareB, prepareP + getBrailleInTable("เอือ") + prepareL);
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
                word = word.Replace(prepareB, prepareP + getBrailleInTable("เอีย") + prepareL);
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
                word = word.Replace(prepareB, prepareP + getBrailleInTable("เอือ") + prepareL);
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
                word = word.Replace(prepareB, prepareP + getBrailleInTable("เอาะ") + prepareL);
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
                word = word.Replace(prepareB, prepareP + getBrailleInTable("เอา") + prepareL);
            }

            //สระอัว
            rgxEx = new Regex(@"\p{L}+ั\p{M}*ว");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"\s*ัว");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("อัว"));
            }

            //สระอำ
            rgxEx = new Regex(@"\p{L}+\p{M}*ำ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"\s*ำ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("ำ"));
            }

            //สระเออะ
            rgxEx = new Regex(@"เ[^\p{M}*]อะ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"เ|อะ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("เออ") + getBrailleInTable("ะ"));
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
                word = word.Replace(prepareB, prepareF + getBrailleInTable("เออ") + lastF);
            }

            //สระเออ
            rgxEx = new Regex(@"เ\p{L}+\p{M}*อ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {

                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"เ|\p{M}*อ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("เออ"));
            }

            //สระเอะ
            rgxEx = new Regex(@"เ[^\p{M}*]ะ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"เ|ะ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("เ") + getBrailleInTable("ะ"));
            }

            //สระแอะ
            rgxEx = new Regex(@"แ[^\p{M}*]ะ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"แ|ะ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("แ") + getBrailleInTable("ะ"));
            }

            //สระโอะ
            rgxEx = new Regex(@"โ\p{L}+\p{M}*ะ");
            match = rgxEx.Matches(word);
            foreach (Match m in match)
            {
                String prepareB = m.Value;
                Regex rgxRemove = new Regex(@"โ|ะ");
                String prepareF = rgxRemove.Replace(prepareB, "");
                word = word.Replace(prepareB, prepareF + getBrailleInTable("โ") + getBrailleInTable("ะ"));
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
            if (numDict.Keys.Contains(word[0]))
            {
                word = parseThaiNum(word).ToString();
                numberStyle = "⠠⠼";
            }

            double n;
            bool isNumeric = double.TryParse(word.Length > 1 ? word.Replace(",", "") : word, out n);
            word = isNumeric ? numberStyle + (word.Contains(",") ? String.Format("{0:n0}", n).Replace(",", "⠂") : word) : word;
            if (word.Equals("ฯลฯ"))
            {
                return getBrailleInTable("ฯลฯ");
            }

            String converted = "";
            foreach (Char element in word)
            {
                converted += getBrailleInTable(element.ToString());
            }
            return converted;
        }
        public static String getBrailleInTable(String ret)
        {
            try
            {
                ret = table.AllBrailleTable[ret];
            }
            catch (KeyNotFoundException ex)
            {
            }

            return ret;
        }

        //Parse Thai Number (Digits)
        Dictionary<char, int> numDict = new Dictionary<char, int>() { {'๐',0}, { '๑', 1 }, { '๒', 2 },
            { '๓', 3 }, { '๔', 4 }, { '๕', 5 },
            {'๖',6},{'๗',7},{'๘',8},{'๙',9}};

        private double parseThaiNum(string thainum)
        {
            double num = 0;
            string temp = "";
            for (var i = 0; i < thainum.Length; i++)
            {
                try
                {
                    temp += numDict[thainum[i]];
                }
                catch (KeyNotFoundException ex) { temp += thainum[i]; };
            }
            double.TryParse(temp, out num);
            return num;
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            inputChanged = true;
        }

        private void help_Click(object sender, EventArgs e)
        {
            About aboutBox = new About();
            aboutBox.ShowDialog();
        }
    }
}
