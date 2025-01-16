using LimpezaDeBase.Services.Interfaces;
using LimpezaDeBase.Modelos.Mongo;
using System.Text.RegularExpressions;

namespace LimpezaDeBase.Services
{
    public class OptOutService : IOptOutService
    {
        private readonly IMongoService _mongoService;
        private readonly ILogger<CatalogoService> _logger;


        public OptOutService(IMongoService mongoService, ILogger<CatalogoService> logger)
        {
            _mongoService = mongoService;
            _logger = logger;
        }

        public async Task<bool> InserirOptOut(string telefone, string contrato, string roteador)
        {
            try
            {
                var optouts = await _mongoService.ObterOptOutPorContrato(contrato);
                var optout = optouts.First(o => o.Roteador == roteador);

                var telefoneLimpo = Regex.Replace(telefone, @"[^0-9]", "");

                if (!telefoneLimpo.StartsWith("55") && telefoneLimpo.Length < 13)
                {
                    telefoneLimpo = "55" + telefoneLimpo;
                }

                if (optout is null)
                {
                    optout = new OptOutDB();

                    optout.Telefone.Add(telefoneLimpo);

                    await _mongoService.InserirOptOutAsync(optout);

                    return true;
                }

                if(optout.Telefone.Contains(telefoneLimpo))
                {
                    return false;
                }

                optout.Telefone.Add(telefoneLimpo);

                await _mongoService.AtualizarOptOutDBAsync(optout.Id, optout);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao tentar adiconar novo telefone {telefone} ao cliente {contrato}");

                return false;
            }
        }

        public async Task<bool> RemoverOptOut(string telefone, string contrato, string roteador)
        {
            try
            {
                var optouts = await _mongoService.ObterOptOutPorContrato(contrato);
                var optout = optouts.First(o => o.Roteador == roteador);

                var telefoneLimpo = Regex.Replace(telefone, @"[^0-9]", "");

                if (!telefoneLimpo.StartsWith("55") && telefoneLimpo.Length < 13)
                {
                    telefoneLimpo = "55" + telefoneLimpo;
                }

                if (optout != null && optout.Telefone.Contains(telefoneLimpo))
                {
                    if (optout.Telefone.Remove(telefoneLimpo))
                    {
                        await _mongoService.AtualizarOptOutDBAsync(optout.Id, optout);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false; 
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Erro ao tentar remover telefone {telefone} do cliente {contrato}");

                return false;
            }
        }
    }
}
