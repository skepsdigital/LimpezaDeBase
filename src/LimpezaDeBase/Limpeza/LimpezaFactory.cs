using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Limpeza.Strategy;
using LimpezaDeBase.Services.Interfaces;

namespace LimpezaDeBase.Limpeza
{
    public class LimpezaFactory
    {
        private readonly Dictionary<string, ILimpezaStrategy> _strategies;

        public LimpezaFactory(ITelefoneRepository telefoneRepository, IOtimaAPI otimaAPI, Dictionary<string,string> credenciais, ILogger<LimparWhatsappStrategy> loggerLimparWhatsapp, ILogger<LimparOptOutStrategy> loggerOptout, IProcessamentoRepository processamentoRepository, IMongoService mongoService, IProcessamentoService processamentoService)
        {
            _strategies = new()
            {
                {"limpezaDeBase", new LimparWhatsappStrategy(telefoneRepository, processamentoRepository, otimaAPI, credenciais, mongoService, loggerLimparWhatsapp, processamentoService) },
                {"blocklist", new LimparOptOutStrategy(loggerOptout, mongoService, processamentoRepository, processamentoService) },
            };
        }

        public ILimpezaStrategy GetStrategy(string name)
        {
            return _strategies[name];
        }
    }
}
