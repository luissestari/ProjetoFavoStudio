using System.Windows.Media;

namespace FavoStudio3D.Models
{
    public class Filamento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;      // Ex: "PLA Branco"
        public string Tipo { get; set; } = string.Empty;      // Ex: "PLA", "PETG", "ABS"
        public string CorHex { get; set; } = "#FFFFFF";      // Código da cor do filamento
        public decimal PrecoPorKg { get; set; }              // Valor por kg
        public double QuantidadeKg { get; set; } = 1.0;      // Quantidade disponível em kg

        // Propriedade formatada para exibir no DataGrid / ItemsControl do WPF
        public string PrecoFormatado => $"R$ {PrecoPorKg:N2}";

        // Cor de fundo visual da tag
        public Brush CorFundoTipo => new SolidColorBrush(Color.FromRgb(252, 235, 211));
    }
}