using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class ResumoImpressorasView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ObservableCollection<ImpressoraModel> ImpressorasBancoDados { get; set; } = new ObservableCollection<ImpressoraModel>();
        public ObservableCollection<ImpressoraModel> ListaImpressoras { get; set; } = new ObservableCollection<ImpressoraModel>();

        private string _textoPesquisa = string.Empty;
        public string TextoPesquisa
        {
            get => _textoPesquisa;
            set
            {
                _textoPesquisa = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoPesquisa)));
                AplicarFiltro();
            }
        }

        public string TextoTotalImpressoras => ImpressorasBancoDados.Count == 0 ? "0 impressoras cadastradas" :
                                              ImpressorasBancoDados.Count == 1 ? "1 impressora cadastrada" :
                                              $"{ImpressorasBancoDados.Count} impressoras cadastradas";

        public ResumoImpressorasView()
        {
            InitializeComponent();
            DataContext = this;

            ImpressorasBancoDados.CollectionChanged += (s, e) => {
                AplicarFiltro();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalImpressoras)));
            };

            AplicarFiltro();
            this.Loaded += (s, e) => AtualizarSubtituloDashboard();
        }

        private void AtualizarSubtituloDashboard()
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                dashboard.TextoSubtituloPagina.Text = "Cadastre e gerencie as impressoras utilizadas na produção.";
            }
        }

        private void AplicarFiltro()
        {
            ListaImpressoras.Clear();
            string termo = TextoPesquisa?.ToLower() ?? "";

            foreach (var item in ImpressorasBancoDados)
            {
                if (string.IsNullOrWhiteSpace(termo) || item.NomeModelo.ToLower().Contains(termo))
                {
                    ListaImpressoras.Add(item);
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalImpressoras)));
        }

        private void BtnNovaImpressora_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                dashboard.TextoSubtituloPagina.Text = "Informe os dados da impressora para cálculo de energia";
            }
            MudarDeTela(new CadastroImpressoraView());
        }

        private void AcaoEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ImpressoraModel impressora)
            {
                MudarDeTela(new CadastroImpressoraView());
            }
        }

        private void AcaoExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ImpressoraModel impressora)
            {
                ImpressorasBancoDados.Remove(impressora);
            }
        }

        private void MudarDeTela(UserControl novaTela)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = novaTela; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, novaTela); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }

    public class ImpressoraModel
    {
        public string CaminhoImagem { get; set; } = "/Assets/Images/IconeImpressoraCard.png";
        public string NomeModelo { get; set; } = string.Empty;
        public string PotenciaFormatada { get; set; } = string.Empty;
        public string CustoKwhFormatado { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }
}