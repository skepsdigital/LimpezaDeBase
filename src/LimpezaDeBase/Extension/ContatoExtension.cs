using LimpezaDeBase.Modelos;
using System.Text.RegularExpressions;

namespace LimpezaDeBase.Extension
{
    public static class ContatoExtension
    {
        private static readonly Regex telefoneRegex = new Regex(@"^\d{4}\d{9}$");

        public static List<Contato> FiltrarTelefonesInvalidos(this List<Contato> contatos)
        {
            // Separar os itens inválidos
            var itensRemovidos = contatos
                .Where(c => !telefoneRegex.IsMatch(c.Telefone))
                .ToList();

            // Remover os inválidos da lista original
            contatos.RemoveAll(itensRemovidos.Contains);

            return itensRemovidos;
        }


        public static string NormalizarTelefone(this string telefone)
        {
            var telefoneLimpo = Regex.Replace(telefone, @"[^0-9]", "");

            if (telefoneLimpo.StartsWith("0"))
            {
                telefoneLimpo = telefoneLimpo.Remove(0, 1);
            }

            if (!telefoneLimpo.StartsWith("55") && telefoneLimpo.Length < 13)
            {
                if (telefoneLimpo.Length < 13)
                {
                    telefoneLimpo = "55" + telefoneLimpo;
                }
            }

            return telefoneLimpo;  
        }

        public static void NormalizarListaContatos(this List<Contato> contatos)
        {
            for (int i = 0; i < contatos.Count; i++)
            {
                var contato = contatos[i];
                var telefoneLimpo = Regex.Replace(contato.Telefone, @"[^0-9]", "");

                if (telefoneLimpo.StartsWith("0"))
                {
                    telefoneLimpo = telefoneLimpo.Remove(0, 1);
                }


                if (!telefoneLimpo.StartsWith("55") && telefoneLimpo.Length < 13)
                {
                    if (telefoneLimpo.Length < 13)
                    {
                        telefoneLimpo = "55" + telefoneLimpo;
                    }
                }
                contatos[i] = new Contato
                {
                    CPF = string.IsNullOrWhiteSpace(contato.CPF) ? "01234567890" : contato.CPF,
                    Telefone = telefoneLimpo,
                    Extras = contato.Extras
                    
                };
            }
        }

        public static void NormalizarListaContatos(this List<ContatoCallbackResponse> contatos)
        {
            for (int i = 0; i < contatos.Count; i++)
            {
                var contato = contatos[i];
                var telefoneLimpo = Regex.Replace(contato.Telefone, @"[^0-9]", "");

                if (telefoneLimpo.StartsWith("0"))
                {
                    telefoneLimpo = telefoneLimpo.Remove(0, 1);
                }


                if (!telefoneLimpo.StartsWith("55") && telefoneLimpo.Length < 13)
                {
                    if (telefoneLimpo.Length < 13)
                    {
                        telefoneLimpo = "55" + telefoneLimpo;
                    }
                }
                contatos[i] = new ContatoCallbackResponse
                {
                    CPF = string.IsNullOrWhiteSpace(contato.CPF) ? "01234567890" : contato.CPF,
                    Telefone = telefoneLimpo,
                    RequestId = contato.RequestId,
                    TemWhatsapp = contato.TemWhatsapp
                };
            }
        }
    }
}
