using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class ResumoCalculadoraView : UserControl
    {
        public ResumoCalculadoraView(ResultadoCalculoModel dados)
        {
            InitializeComponent();
            DataContext = dados;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    try
                    {
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                            txtTitulo.Text = "Revisar e salvar";
                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = "Revise as informações antes de salvar o produto.";
                    }
                    catch { }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                try
                {
                    if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                        txtTitulo.Text = "Calculadora";
                    if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                        txtSub.Text = "Calcule os custos de produção e defina o preço de venda dos seus produtos 3D.";
                }
                catch { }
            }

            var telaCalculadora = new CalculadoraView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaCalculadora; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaCalculadora); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Produto salvo no banco de dados com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // ========================================================
    // ESTRUTURA DE DADOS PARA O BACKEND (Bindings da Tela)
    // ========================================================
    public class ResultadoCalculoModel
    {
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoDescricao { get; set; } = string.Empty;

        public ObservableCollection<ParametroInfo> Parametros { get; set; } = new ObservableCollection<ParametroInfo>();
        public ObservableCollection<CustoInfo> CustosDetalhados { get; set; } = new ObservableCollection<CustoInfo>();

        public string CustoTotalProducao { get; set; } = "R$ 0,00";
        public string PrecoVendaSugerido { get; set; } = "R$ 0,00";
    }

    public class ParametroInfo
    {
        public string Icone { get; set; } = string.Empty;
        public string Rotulo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class CustoInfo
    {
        public string Icone { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string ValorFormatado { get; set; } = "R$ 0,00";
    }
}