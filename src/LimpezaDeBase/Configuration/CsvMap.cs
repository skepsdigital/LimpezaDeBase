using CsvHelper.Configuration;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Configuration
{
    public class TelefoneEntityMap : ClassMap<TelefoneEntity>
    {
        public TelefoneEntityMap()
        {
            Map(c => c.Telefone).Name("Telefone");
            Map(c => c.PossuiWpp).Name("É valido?");
        }
    }

    public class ContatoMap : ClassMap<Contato>
    {
        public ContatoMap()
        {
            Map(m => m.Telefone).Name("Telefone","TELEFONE","telefone","Numero","NUMERO","numero","Celular","CELULAR","celular"); 
            Map(m => m.CPF).Optional(); 
            Map(m => m.Extras).Convert(args =>
            {
                var row = args.Row;
                var extras = new Dictionary<string, string>();

                foreach (var header in row.HeaderRecord)
                {
                    if (!string.Equals(header, "telefone", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(header, "celular", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(header, "numero", StringComparison.OrdinalIgnoreCase))
                    {
                        extras[header.ToLower()] = row.GetField(header);
                    }
                }

                return extras;
            });
        }
    }

    public class OptOutFullMap : ClassMap<OptOutResultado>
    {
        public OptOutFullMap()
        {
            Map(m => m.Telefone).Name("Telefone");
            Map(m => m.QuerReceberNotificao).Name("Quer receber notificação?");
            Map(m => m.NumeroEstaFormatado).Name("O Numero esta formatado?");
        }
    }


    public class OptOutLiteMap : ClassMap<OptOutResultado>
    {
        public OptOutLiteMap(IEnumerable<string> extraHeaders)
        {
            Map(m => m.Telefone).Name("Telefone");
            foreach (var header in extraHeaders)
            {
                Map().Name(header).Convert(record =>
                {
                    var extras = ((OptOutResultado)record.Value).Extras;
                    return extras.ContainsKey(header) ? extras[header] : null;
                });
            }
        }
    }
    

    public class RelatorioMap : ClassMap<RelatorioLimpeza>
    {
        public RelatorioMap()
        {
            Map(m => m.NumerosTotais).Name("Numeros Totais");
            Map(m => m.NumerosValidos).Name("Numeros Validos");
            Map(m => m.NumerosInvalidos).Name("Numeros Invalidos");
            Map(m => m.Economia).Name("Quanto você economizou");
            Map(m => m.DDD).Name("DDD");
        }
    }
}
