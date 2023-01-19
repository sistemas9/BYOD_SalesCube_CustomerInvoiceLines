using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  class ConsultaEntity
  {

    public async Task<RestResponse> QueryEntity(String url)
    {
      var auth = new AuthenticationHeader();
      var token = await auth.getAuthenticationHeader();
      var client = new RestClient(url);
      var request = new RestRequest("/",Method.Get);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Accept", "application/json");
      request.AddHeader("Authorization", "Bearer " + token);
      request.Timeout = 600000;
      //request.AddCookie(client,"ApplicationGatewayAffinity", "e7fb295f94cb4b5e0cd1e2a4712e4a803fc926342cc4ecca988f29125dbd4b04","","");
      var response = await client.ExecuteAsync(request);
      return response;
    }

    public async Task<dynamic> QueryWebService(String url, String[] parametros)
    {
      var auth = new AuthenticationHeader();
      var token = await auth.getAuthenticationHeader();
      var client = new RestClient(url);
      var request = new RestRequest("/",Method.Post);
      request.AddHeader("Content-Type", "application/json");
      request.AddHeader("Accept", "application/json");
      request.AddHeader("Authorization", "Bearer "+token);
      //request.AddCookie("ApplicationGatewayAffinity", "e7fb295f94cb4b5e0cd1e2a4712e4a803fc926342cc4ecca988f29125dbd4b04");
      if (parametros.Count() > 0)
      {
        request.AddParameter("application/json", "{\n\"invoiceId\":\"" + parametros[0] + "\",\n\t\"transDate\":\"" + parametros[1] + "\",\n\t\"custInvoiceAccount\":\"" + parametros[2] + "\",\n\t\"company\":\"" + parametros[3] + "\",\n}", ParameterType.RequestBody);
      }
      var response = await client.ExecuteAsync(request);

      return response;
    }
  }
}
