using Google.Apis.Auth;
using SummerLoginServer.Models;

namespace SummerLoginServer.Services
{
    public class GoogleService
    {
        //구글 서비스에서 발급받은 클라이언트 Id들
        //클라에서 보내준 값이 같은지 확인해야함
        private readonly IReadOnlyCollection<string> _clientIds;

        public GoogleService(IConfiguration configuration)
        {
            _clientIds =
                configuration.GetSection("Google:ClientIds").Get<string[]>()
                ?? [];

            if (_clientIds.Count == 0)
                throw new InvalidOperationException("ClientIds 누락됨");
        }
        public async Task<GoogleUserInfo?> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
        {
            try
            {
                //구글쪽에 요청보냄 이 id가 맞는지 확인
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = _clientIds
                    });

                if (string.IsNullOrWhiteSpace(payload.Subject))
                    return null;
                return new GoogleUserInfo(
                    payload.Subject,
                    payload.Email,
                    payload.Name,
                    payload.Picture);

            }
            catch (InvalidJwtException)
            {
                return null;
            }

        }
    }
}
