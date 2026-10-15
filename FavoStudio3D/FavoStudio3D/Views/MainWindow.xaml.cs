using System.Windows;

namespace FavoStudio3D.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BotaoEntrar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bem-vindo ao Favo Studio 3D!", "Acesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}