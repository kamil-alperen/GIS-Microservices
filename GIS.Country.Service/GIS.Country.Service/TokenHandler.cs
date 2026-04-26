using System.Net.Http.Headers;

namespace GIS.Country.Service
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Mevcut HTTP context'ten "Authorization" header'ını oku
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring(7); // "Bearer " kısmını ayıklıyoruz

                // 2. Giden isteğe (request) bu token'ı ekle
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 3. İsteğin yoluna devam etmesini sağla
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
