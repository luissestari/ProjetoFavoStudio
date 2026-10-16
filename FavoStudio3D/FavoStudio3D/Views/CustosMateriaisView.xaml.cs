using System.Windows;
using System.Windows.Controls;

namespace FavoStudio3D.Views
{
    public partial class CustosMateriaisView : UserControl
    {
        public CustosMateriaisView()
        {
            InitializeComponent();
        }

        private void BotaoCadastrarNovo_Click(object sender, RoutedEventArgs e)
        {
            // Procura a janela principal (Dashboard) e diz para ela carregar a tela nova!
            var dashboard = Window.GetWindow(this) as DashboardWindow;
            if (dashboard != null)
            {
                dashboard.AreaConteudo.Content = new CadastroCustosView();
            }
        }
    }
}