using LimpezaDeBase.Modelos;
using RestEase;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IOtimaAPI
    {
        [Post("v1/contact/")]
        Task<string> PostContact(
            [Header("Authorization")] string authorization,
            [Body(BodySerializationMethod.Serialized)] string contactRequest,
            [Header("Content-Type")] string type = "application/json"
        );

        [Post("token")]
        Task<TokenResponse> GetToken(
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> data,
        [Header("Content-Type")] string type = "application/x-www-form-urlencoded");
    }
}
