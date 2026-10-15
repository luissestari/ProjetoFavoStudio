using System.Windows;
using System.Windows.Input; // Necessário para detetar o clique do rato no StackPanel

namespace FavoStudio3D.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
        }

        // Ação que acontece quando clicamos no botão Sair
        private void BotaoSair_Click(object sender, MouseButtonEventArgs e)
        {
            // 1. Cria a janela de boas-vindas novamente
            MainWindow telaInicio = new MainWindow();

            // 2. Mostra a tela de boas-vindas no ecrã
            telaInicio.Show();

            // 3. Fecha o Dashboard (a janela atual)
            this.Close();
        }
    }
}