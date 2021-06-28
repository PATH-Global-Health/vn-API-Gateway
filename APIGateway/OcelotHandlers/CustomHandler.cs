using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APIGateway.OcelotHandlers
{
    public class CustomHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string VALIDATIONURL;

        public CustomHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            VALIDATIONURL = configuration["PermissionValidationUrl"];
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(VALIDATIONURL))
            {
                throw new Exception("Missing PermissionValidationUrl");
            }

            var httpClient = _httpClientFactory.CreateClient();

            #region Get Request body
            var permissionModel = new ResourcePermissionValidationModel
            {
                Method = request.Method.Method,
                ApiPath = request.RequestUri.AbsolutePath
            };
            var bodyContent = new StringContent(JsonConvert.SerializeObject(permissionModel), Encoding.UTF8, "application/json");
            #endregion

            #region Add Authorization 
            httpClient.DefaultRequestHeaders.Authorization = request.Headers.Authorization;
            #endregion

            var response = await httpClient.PostAsync(VALIDATIONURL, bodyContent, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                httpClient.Dispose();
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            httpClient.Dispose();
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class ResourcePermissionValidationModel
    {
        public string Method { get; set; }
        public string ApiPath { get; set; }
    }
}
