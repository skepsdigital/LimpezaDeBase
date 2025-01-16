
using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos
{
    public class Contato
    {
        [JsonPropertyName("phone")]
        public string Telefone { get; set; }

        [JsonPropertyName("cpf")]
        public string CPF { get; set; }

        public Dictionary<string, string> Extras { get; set; } = new();
    }

    public class ContatoComparer : IEqualityComparer<Contato>
    {
        public bool Equals(Contato x, Contato y)
        {
            return x.Telefone == y.Telefone;
        }

        public int GetHashCode(Contato obj)
        {
            return obj.Telefone.GetHashCode();
        }
    }
}
