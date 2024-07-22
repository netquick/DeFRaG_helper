using System.Threading.Tasks;
using System.Windows;

namespace DeFRaG_Helper.Windows
{
    public partial class CustomMessageBox : Window
    {
        public bool DialogResult { get; private set; }

        public CustomMessageBox(string message, string title, string button1text, string button2text)
        {
            InitializeComponent();
            this.txtTitle.Text = title;
            this.txtMessage.Text = message;
            this.btnOne.Content = button1text;
            this.btnTwo.Content = button2text;
        }

        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Button 1 was pressed
            this.Close();
        }

        private void btnTwo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Button 2 was pressed
            this.Close();
        }

        // Static method to show the dialog
        public static async Task<bool> Show(string message, string title, string button1text, string button2text)
        {
            var dialog = new CustomMessageBox(message, title, button1text, button2text);
            dialog.ShowDialog();
            return dialog.DialogResult;
        }

    }
}
