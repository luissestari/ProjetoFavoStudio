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

        private ImpressoraModel? _impressoraParaExcluir = null;

        public ResumoImpressorasView()
        {
            InitializeComponent();
            DataContext = this;

            // Alimenta a memória inicial de impressoras se estiver vazia
            if (ImpressorasBancoDados.Count == 0)
            {
                ImpressorasBancoDados.Add(new ImpressoraModel { Nome = "Ender 3", Modelo = "V2", NomeModelo = "Ender 3 V2", PotenciaFormatada = "350 W", CustoKwhFormatado = "R$ 0,85/kWh", Observacoes = "Principal" });
                ImpressorasBancoDados.Add(new ImpressoraModel { Nome = "Bambu Lab", Modelo = "X1-Carbon", NomeModelo = "Bambu Lab X1-Carbon", PotenciaFormatada = "1000 W", CustoKwhFormatado = "R$ 0,85/kWh", Observacoes = "Alta velocidade" });
            }

            ImpressorasBancoDados.CollectionChanged += (s, e) => {
                AplicarFiltro();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalImpressoras)));
            };

            AplicarFiltro();

            this.Loaded += (s, e) =>
            {
                // Como adicionamos itens acima, a contagem não será zero, 
                // então ela vai carregar o resumo bonitinho em vez de redirecionar direto.
                if (ImpressorasBancoDados.Count == 0)
                {
                    MudarDeTela(new CadastroImpressoraView());
                    return;
                }

                AtualizarSubtituloDashboard();
            };
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

        // --- FUNÇÃO PARA FECHAR O POPUP ---
        private void FecharPopupAcoes(FrameworkElement elemento)
        {
            FrameworkElement? atual = elemento;
            while (atual != null)
            {
                if (atual.Parent is System.Windows.Controls.Primitives.Popup popup)
                {
                    popup.IsOpen = false;
                    return;
                }
                atual = atual.Parent as FrameworkElement;
            }
        }

        private void AcaoEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                FecharPopupAcoes(btn);
                if (btn.DataContext is ImpressoraModel impressora)
                {
                    // Passa a impressora para a tela de cadastro
                    MudarDeTela(new CadastroImpressoraView(impressora));
                }
            }
        }

        private void AcaoExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                FecharPopupAcoes(btn);
                if (btn.DataContext is ImpressoraModel impressora)
                {
                    _impressoraParaExcluir = impressora;
                    TxtMensagemExclusao.Text = $"Tem certeza que deseja excluir a impressora '{impressora.NomeModelo}'?";
                    ModalConfirmacaoExclusao.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnCancelarExclusao_Click(object sender, RoutedEventArgs e)
        {
            _impressoraParaExcluir = null;
            ModalConfirmacaoExclusao.Visibility = Visibility.Collapsed;
        }

        private void BtnConfirmarExclusao_Click(object sender, RoutedEventArgs e)
        {
            if (_impressoraParaExcluir != null)
            {
                ImpressorasBancoDados.Remove(_impressoraParaExcluir);
                _impressoraParaExcluir = null;

                // Regra extra: Se excluir a ÚLTIMA impressora, volta para o cadastro sozinho
                if (ImpressorasBancoDados.Count == 0)
                {
                    MudarDeTela(new CadastroImpressoraView());
                }
            }
            ModalConfirmacaoExclusao.Visibility = Visibility.Collapsed;
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
        public string CaminhoImagem { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NomeModelo { get; set; } = string.Empty;
        public string PotenciaFormatada { get; set; } = string.Empty;
        public string CustoKwhFormatado { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }
}