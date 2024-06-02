using System;
using System.Threading;
using System.Windows.Forms;
using ThaiOpenBraille.Api;

namespace ThaiOpenBraille
{
    public delegate void OpenFormDelegate();
    public partial class MainProgram : Form
    {

        private Processing LoadingForm = new Processing();

        public MainProgram()
        {
            InitializeComponent();
        }
        Boolean inputChanged = false;

        private void TranslateButton_Click(object sender, EventArgs e)
        {
            if (inputChanged && inputTextBox.Text.Length > 0)
            {
                Thread loadinThread = new Thread(new ThreadStart(LoadingFunction));
                Thread translateThread = new Thread(new ThreadStart(TranslateFunction));
                loadinThread.IsBackground = true;
                translateThread.IsBackground = true;
                loadinThread.Start();
                translateThread.Start();
            }
        }

        private void TranslateFunction()
        {
            this.BeginInvoke(new OpenFormDelegate(DoTranslate));
        }
        private void LoadingFunction()
        {
            try
            {
                LoadingForm.ShowDialog();
            }
            catch (Exception)
            {
                LoadingForm.Hide();
            }
        }

        private void DoTranslate()
        {
            //Clear data.
            outputTextBox.Clear();
			IWordManager translateResult = new WordManager(inputTextBox.Text);
			outputTextBox.Text = translateResult.Output();
            inputChanged = false;
            LoadingForm.Invoke((MethodInvoker)(() => LoadingForm.Hide()));
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

        private void clearButton_Click(object sender, EventArgs e)
        {
            inputTextBox.Text = "";
            outputTextBox.Text = "";
        }
    }
}
