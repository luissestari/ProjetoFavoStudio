using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FavoStudio3D.Data;
using FavoStudio3D.Models;

namespace FavoStudio3D.ViewModels
{
    public class ResumoCustosMateriaisViewModel : INotifyPropertyChanged
    {
        private string _textoPesquisaFilamento = string.Empty;
        private string _textoPesquisaCusto = string.Empty;

        public ObservableCollection<Filamento> ListaFilamentos { get; set; }
        public ObservableCollection<CustoAdicional> ListaOutrosCustos { get; set; }

        public string TextoPesquisaFilamento
        {
            get => _textoPesquisaFilamento;
            set
            {
                _textoPesquisaFilamento = value;
                OnPropertyChanged();
                FiltrarFilamentos();
            }
        }

        public string TextoPesquisaCusto
        {
            get => _textoPesquisaCusto;
            set
            {
                _textoPesquisaCusto = value;
                OnPropertyChanged();
                FiltrarCustos();
            }
        }

        public string TextoTotalFilamentos => $"{ListaFilamentos.Count} filamentos cadastrados";

        public ResumoCustosMateriaisViewModel()
        {
            // Aponta para as listas salvas em memória
            ListaFilamentos = new ObservableCollection<Filamento>(DadosNaMemoriaData.Filamentos);
            ListaOutrosCustos = new ObservableCollection<CustoAdicional>(DadosNaMemoriaData.Custos);
        }

        private void FiltrarFilamentos()
        {
            var filtrados = DadosNaMemoriaData.Filamentos
                .Where(f => string.IsNullOrWhiteSpace(TextoPesquisaFilamento) ||
                            f.Nome.ToLower().Contains(TextoPesquisaFilamento.ToLower()) ||
                            f.Tipo.ToLower().Contains(TextoPesquisaFilamento.ToLower()))
                .ToList();

            ListaFilamentos.Clear();
            foreach (var item in filtrados)
            {
                ListaFilamentos.Add(item);
            }
            OnPropertyChanged(nameof(TextoTotalFilamentos));
        }

        private void FiltrarCustos()
        {
            var filtrados = DadosNaMemoriaData.Custos
                .Where(c => string.IsNullOrWhiteSpace(TextoPesquisaCusto) ||
                            c.Descricao.ToLower().Contains(TextoPesquisaCusto.ToLower()) ||
                            c.Tipo.ToLower().Contains(TextoPesquisaCusto.ToLower()))
                .ToList();

            ListaOutrosCustos.Clear();
            foreach (var item in filtrados)
            {
                ListaOutrosCustos.Add(item);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}