using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class ImpressorasView : UserControl
    {
        public ImpressorasView()
        {
            InitializeComponent();

            // Adicionado evento de carregamento para verificar se já existem cadastros
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Verifica se a lista de impressoras já possui itens salvos
            if (ResumoImpressorasView.ImpressorasBancoDados != null &&
                ResumoImpressorasView.ImpressorasBancoDados.Count > 0)
            {
                // Faz o desvio automático para a tela de Resumo de Impressoras
                var telaResumo = new ResumoImpressorasView();
                DependencyObject parent = VisualTreeHelper.GetParent(this);

                while (parent != null)
                {
                    if (parent is ContentControl cc)
                    {
                        cc.Content = telaResumo;
                        return;
                    }
                    if (parent is Panel panel)
                    {
                        int index = panel.Children.IndexOf(this);
                        if (index >= 0)
                        {
                            panel.Children.RemoveAt(index);
                            panel.Children.Insert(index, telaResumo);
                            return;
                        }
                    }
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
        }

        private void BtnCadastrarImpressora_Click(object sender, RoutedEventArgs e)
        {
            // Atualiza o subtítulo corretamente conforme o design do Figma
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                dashboard.TextoSubtituloPagina.Text = "Informe os dados da impressora para cálculo de energia";
            }

            var telaCadastro = new CadastroImpressoraView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaCadastro; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaCadastro); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}