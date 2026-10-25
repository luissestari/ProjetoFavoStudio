using System.Collections.Generic;

namespace FavoStudio3D.Models
{
    public class ResultadoCalculoModel
    {
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoCategoria { get; set; } = string.Empty;
        public string ProdutoDescricao { get; set; } = string.Empty;
        public string CustoTotalProducao { get; set; } = string.Empty;
        public string PrecoVendaSugerido { get; set; } = string.Empty;
        public string CaminhoImagemProduto { get; set; } = string.Empty;

        public List<ParametroInfo> Parametros { get; set; } = new List<ParametroInfo>();
        public List<CustoInfo> CustosDetalhados { get; set; } = new List<CustoInfo>();
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
        public string ValorFormatado { get; set; } = string.Empty;
    }
}