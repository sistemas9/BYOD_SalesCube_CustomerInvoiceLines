using FastMember;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class Program
  {
    private static databaseAyt03 dbayt03 = new databaseAyt03();
    private static List<CustomerInvoiceTransValues_> totalTemp;
    private static List<InventTransResponseValues> totalInventTransValues;
    public static String totalTemp_;
    //public static List<CustomerInvoiceTransValues_> totalTemp_;
    public static void Main(string[] args)
    {
      DateTime inicio = DateTime.Now;
      Console.WriteLine("INICIO");

      totalInventTransValues = GetInventTransValues(); ///Este tambien!!!
      totalTemp = GetCustomerInvoiceTransValues(); ///este siii!!!

      InsertCustomerInvoiceLinesSalesCube(); ///este si!!!
      InsertInventTransValues();
      DateTime fin = DateTime.Now;
      Console.WriteLine("Total registro : " + totalInventTransValues.Count.ToString());
      Console.WriteLine("Total registro : " + totalTemp.Count.ToString());
      Console.WriteLine("Tiempo transcurrido: " + ((fin - inicio).TotalSeconds).ToString());
     }
    public static List<String> GetUrlsListInventTrans(int totalRecords)
    {
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=DateFinancial%20eq%202023-01-25T12%3A00%3A00Z&%24select=dataAreaId%2CDateFinancial%2CQty%2CCostAmountOperations%2CCostAmountAdjustment%2CInventDimId%2CQtySettled%2CInvoiceReturned%2CCostAmountPosted%2CCurrencyCode%2CStatusIssue%2CVoucher%2CItemId%2CInvoiceId%2CCostAmountPhysical%2CStatusReceipt";
      List<String> urlArray = new List<String>();
      for (int i = 0; i < totalRecords; i += 1000)
      {
        String urlComplemento = "&$top=1000&$skip=" + i.ToString();
        String urlCompleta = urlBase + urlComplemento;
        urlArray.Add(urlCompleta);
      }
      return urlArray;
    }
    public static int GetTotalInventTrans()
    {
      int TotalCustomerInvoice = 0;
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=DateFinancial%20eq%202023-01-25T12%3A00%3A00Z&%24select=dataAreaId&%24top=1&%24count=true";
      ConsultaEntity entity = new ConsultaEntity();
      var result = entity.QueryEntity(urlBase).Result;
      var resultProducts = JsonConvert.DeserializeObject<dynamic>(result.Content.Replace("@odata.count", "count"));
      TotalCustomerInvoice = resultProducts.count;
      return TotalCustomerInvoice;
    }
    public static List<InventTransResponseValues> GetInventTransValues()
    {
      List<InventTransResponse> invTransResponse = new List<InventTransResponse>();
      JsonTextModel jsonList = new JsonTextModel();
      jsonList.value = new JsonTextValueModel();
      JsonTextValueModel temp = new JsonTextValueModel();
      temp.jsonText = new List<String>();
      int totalInventTrans = GetTotalInventTrans();
      ConsultaEntity entity = new ConsultaEntity();
      if (totalInventTrans > 0)
      {
        List<String> urlArray = GetUrlsListInventTrans(totalInventTrans);
        //List<String> urlArray2 = new List<String>() { urlArray[urlArray.Count - 1] };        
        Parallel.ForEach(urlArray, (url) =>
        {
          try
          {
            
            var result = entity.QueryEntity(url);

            InventTransResponse InventTransProducts = JsonConvert.DeserializeObject<InventTransResponse>(result.Result.Content);

            if (InventTransProducts.value.Count > 0)
            {
              invTransResponse.Add(InventTransProducts);
              //temp.jsonText.Add(result.Result.Content);
            }
            
          } catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            Console.WriteLine("url: {0}", url);
          }
        });
      }

      var s = new Stopwatch();
      s.Start();
      /*var iterInvTransResposeValues = invTransResponse[0].value.GetEnumerator();
      while (iterInvTransResposeValues.MoveNext())
      {
        iterInvTransResposeValues.Current.InventLocationId = GetInventLocationId(iterInvTransResposeValues.Current.InventDimId);
      }*/
      Parallel.ForEach(invTransResponse, (invTransResp) => {
        Parallel.ForEach(invTransResp.value, (invTrans) => {
          invTrans.InventLocationId = GetInventLocationId(invTrans.InventDimId);
        });
      });
      s.Stop();
      Console.WriteLine("tardo en ilid: {0} milisegundos", s.ElapsedMilliseconds);


      List<InventTransResponseValues> InvTransProductsFinal = invTransResponse.SelectMany(x => x.value).ToList();
      //jsonList.value = temp;
      //Console.WriteLine("json {0}", jsonList.value.jsonText.Count);
      //InsertJsonSourceInvTrans(jsonList);
      return InvTransProductsFinal;
    }
    public static List<String> GetUrlsListCustomers()
    {
      int totalRecords = GetTotalCustomers();
      String urlBase = "https://ayt.operations.dynamics.com/Data/CustomersV3?%24filter=RFCNumber%20ne%20%27%27&%24select=RFCNumber%2COrganizationName%2CInvoiceAddressZipCode%2CAddressZipCode";
      List<String> urlArray = new List<String>();
      for (int i = 0; i < totalRecords; i += 5000)
      {
        String urlComplemento = "&$top=5000&$skip=" + i.ToString();
        String urlCompleta = urlBase + urlComplemento;
        urlArray.Add(urlCompleta);
      }
      return urlArray;
    }
    public static int GetTotalCustomers()
    {
      int TotalCustomers = 0;
      String urlBase = "https://ayt.operations.dynamics.com/Data/CustomersV3?%24select=RFCNumber&%24top=1&%24count=true";
      ConsultaEntity entity = new ConsultaEntity();
      var result = entity.QueryEntity(urlBase).Result;
      var ResultCustomers = JsonConvert.DeserializeObject<dynamic>(result.Content.Replace("@odata.count", "count"));
      TotalCustomers = ResultCustomers.count;
      return TotalCustomers;
    }
    public static List<ClientesResponse> GetCustomers()
    {
      List<ClientesResponse> totalClientes = new List<ClientesResponse>();
      int totalRecordsClientes = GetTotalCustomers();
      ConsultaEntity entity = new ConsultaEntity();
      if (totalRecordsClientes > 0)
      {
        List<String> urlArray = GetUrlsListCustomers();
        Parallel.ForEach(urlArray, (url) =>
        {
          var result = entity.QueryEntity(url);
          try
          {
            ClientesResponse InventTransProducts = JsonConvert.DeserializeObject<ClientesResponse>(result.Result.Content);
            totalClientes.Add(InventTransProducts);
          } catch (Exception ex)
          {
            Console.WriteLine("url: " + url);
            Console.WriteLine("exception: " + ex.Message);
          }
        });
      }
      List<Clientes> clientesFinal = totalClientes.SelectMany(x => x.value).ToList();
      return totalClientes;
    }    
    public static void GenerateCustomerFiles(List<ClientesResponse> clientesTotal)
    {
      int fileName = 1;
      foreach (var clientes in clientesTotal)
      {
        string path = @"C:\tmp\clietesDynamics_INN_" + fileName.ToString() + ".txt";
        fileName++;
        using (FileStream fs = File.Create(path))
        {
          String text = "";
          int count = 1;
          foreach (var cli in clientes.value)
          {
            String zipCode = (cli.InvoiceAddressZipCode == "") ? (cli.AddressZipCode == "") ? "0" : cli.AddressZipCode : cli.InvoiceAddressZipCode;
            text += count.ToString() + "|" + cli.RFCNumber.ToString() + "|" + cli.OrganizationName + "|" + zipCode + "\r\n";
            count++;
          }
          byte[] textArray = Encoding.UTF8.GetBytes(text);
          fs.Write(textArray, 0, textArray.Length);
        }
      }
    }
    public static int GetTotalInventDims()
    {
      int TotalInventDims = 0;
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_InventDimV2?%24top=1&%24count=true";
      ConsultaEntity entity = new ConsultaEntity();
      var result = entity.QueryEntity(urlBase).Result;
      var resultInventDims = JsonConvert.DeserializeObject<dynamic>(result.Content.Replace("@odata.count", "count"));
      TotalInventDims = resultInventDims.count;
      return TotalInventDims;
    }
    public static List<String> GetUrlsListInventDims()
    {
      int totalRecords = GetTotalInventDims();
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_InventDimV2?";
      List<String> urlArray = new List<String>();
      for (int i = 0; i < totalRecords; i += 1000)
      {
        String urlComplemento = "$top=1000&$skip=" + i.ToString();
        String urlCompleta = urlBase + urlComplemento;
        urlArray.Add(urlCompleta);
      }
      return urlArray;
    }
    public static List<InventDimsReponse> GetInventDims()
    {
      List<InventDimsReponse> totalInventDims = new List<InventDimsReponse>();
      int totalRecordsInventDims = GetTotalCustomers();
      ConsultaEntity entity = new ConsultaEntity();
      if (totalRecordsInventDims > 0)
      {
        List<String> urlArray = GetUrlsListInventDims();
        Parallel.ForEach(urlArray, (url) =>
        {
          var result = entity.QueryEntity(url);
          try
          {
            InventDimsReponse InventDims = JsonConvert.DeserializeObject<InventDimsReponse>(result.Result.Content);
            totalInventDims.Add(InventDims);
          }
          catch (Exception ex)
          {
            Console.WriteLine("url: " + url);
            Console.WriteLine("exception: " + ex.Message);
          }
        });
      }
      List<InventDimsValues> inventDimsFinal = totalInventDims.SelectMany(x => x.value).ToList();
      return totalInventDims;
    }
    public static int GetTotalCustomerInvoiveTrans()
    {
      int TotalCustomerInvoiceTrans = 0;
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_CustInvoiceTrans?%24select=dataAreaId&%24top=1&%24count=true&$filter=InvoiceDate%20eq%202023-01-25T12%3A00%3A00Z";
      ConsultaEntity entity = new ConsultaEntity();
      var result = entity.QueryEntity(urlBase).Result;
      var resultCustomerInvoiceTrans = JsonConvert.DeserializeObject<dynamic>(result.Content.Replace("@odata.count", "count"));
      TotalCustomerInvoiceTrans = resultCustomerInvoiceTrans.count;
      return TotalCustomerInvoiceTrans;
    }
    public static List<String> GetUrlsListCustomerInvoiceTrans()
    {
      int totalRecords = GetTotalCustomerInvoiveTrans();
      String urlBase = "https://ayt.operations.dynamics.com/Data/AYT_CustInvoiceTrans?%24filter=InvoiceDate%20eq%202023-01-25T12%3A00%3A00Z&%24select=Qty%2CTaxAmountMST%2CInventQty%2CLineAmountMST%2CdataAreaId%2CStockedProduct%2CInvoiceDate%2CCurrencyCode%2CItemId%2CSalesUnit%2CInventDimId%2CInvoiceId%2CSalesId";
      List<String> urlArray = new List<String>();
      for (int i = 0; i < totalRecords; i += 500)
      {
        String urlComplemento = "&$top=500&$skip=" + i.ToString();
        String urlCompleta = urlBase + urlComplemento;
        urlArray.Add(urlCompleta);
      }
      return urlArray;
    }
    public static List<CustomerInvoiceTransValues_> GetCustomerInvoiceTransValues()
    {
      List<CustomerInvoiceTransResponse> customerInvoiceTransResponse = new List<CustomerInvoiceTransResponse>();
      int totalcustomerInvoiceTrans = GetTotalCustomerInvoiveTrans();
      ConsultaEntity entity = new ConsultaEntity();
      if (totalcustomerInvoiceTrans > 0)
      {
        var s = new Stopwatch();
        s.Start();
        List<String> urlArray = GetUrlsListCustomerInvoiceTrans();
        Parallel.ForEach(urlArray, (url) =>
        {
          var result = entity.QueryEntity(url);
          String result2 = result.Result.Content.Replace("@odata.context", "odata").Replace("@odata.etag", "etag");
          CustomerInvoiceTransResponse customerInvoiceTransResponseTemp = JsonConvert.DeserializeObject<CustomerInvoiceTransResponse>(result2);

          try
          {

            var iterCustInvTransResp = customerInvoiceTransResponseTemp.value.GetEnumerator();
            while (iterCustInvTransResp.MoveNext())
            {
              var s2 = new Stopwatch();
              s2.Start();
              iterCustInvTransResp.Current.InventLocationId = GetInventLocationId(iterCustInvTransResp.Current.InventDimId);
              var itid = iterCustInvTransResp.Current.ItemId;
              var ilid = iterCustInvTransResp.Current.InventLocationId;
              var s = GetInventTransCosts(itid);
              //iterCustInvTransResp.Current.COGS = GetCOGS(itid, ilid);
              Console.WriteLine(iterCustInvTransResp.Current.InventLocationId);
              Console.WriteLine("tardo : {0} milisegundos", s2.ElapsedMilliseconds);
            }

            /*IEnumerable<CustomerInvoiceTransValues_> lista = null;
            lista = (IEnumerable<CustomerInvoiceTransValues_>)from val in customerInvoiceTransResponseTemp.value.AsEnumerable() select val;
            Parallel.ForEach(lista, (iter) =>
            {
              var s2 = new Stopwatch();
              s2.Start();
              iter.InventLocationId = GetInventLocationId(iter.InventDimId);
              var itid = iter.ItemId;
              var ilid = iter.InventLocationId;
              //iter.COGS = GetCOGS(itid, ilid);
              Console.WriteLine(iter.InventLocationId);
              Console.WriteLine("tardo : {0} milisegundos", s2.ElapsedMilliseconds);
            });*/
            customerInvoiceTransResponse.Add(customerInvoiceTransResponseTemp);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException);
            return;
          }
        });
        s.Stop();
        Console.WriteLine("tardo en ilid: {0} milisegundos", s.ElapsedMilliseconds);
      }
      List<CustomerInvoiceTransValues_> customerInvoiceTransTotal = customerInvoiceTransResponse.SelectMany(x => x.value).ToList();
      //var r = customerInvoiceTransResponse.SelectMany(x => x.value.Select(y => y.values_)).ToList();
      return customerInvoiceTransTotal;
    }
    public static void InsertCustomerInvoiceLinesSalesCube()
    {
      String truncate = "TRUNCATE TABLE AYT_CustomerInvoiceTransLines";
      int rows = dbayt03.queryInsert(truncate);
      var copyParameters = new[]
      {
        "idCustInvLines",
        "Qty",
        "TaxAmountMST",
        "InventQty",
        "LineAmountMST",
        "dataAreaId",
        "StockedProduct",
        "InvoiceDate",
        "InvoiceId",
        "CurrencyCode",
        "ItemId",
        "SalesUnit",
        "InventDimId",
        "InventLocationId",
        "SalesId",
      };
      var AppSettings = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();
      String connStr = AppSettings["DB_AYT03"];
      using (var sqlCopy = new SqlBulkCopy(connStr))
      {
        sqlCopy.DestinationTableName = "[AYT_CustomerInvoiceTransLines]";
        sqlCopy.BatchSize = 5000;
        sqlCopy.BulkCopyTimeout = 1000;
        var s1 = new Stopwatch();
        s1.Start();
        var s = totalTemp.Select(x => new
        {
          Qty = x.Qty,
          TaxAmountMST = x.TaxAmountMST,
          InventQty = x.InventQty,
          LineAmountMST = x.LineAmountMST,
          dataAreaId = x.dataAreaId,
          StockedProduct = x.StockedProduct,
          InvoiceDate = x.InvoiceDate,
          InvoiceId = x.InvoiceId,
          CurrencyCode = x.CurrencyCode,
          ItemId = x.ItemId,
          SalesUnit = x.SalesUnit,
          InventDimId = x.InventDimId,
          InventLocationId = x.InventLocationId,
          SalesId = x.SalesId,
        }).ToList();
        s1.Stop();
        Console.WriteLine("tardo object to insert: {0} milisegundos", s1.ElapsedMilliseconds);
        using (var readerProducts = ObjectReader.Create(s, copyParameters))
        {
          sqlCopy.WriteToServer(readerProducts);
        }
      }
    }
    public static String GetInventLocationId(String InventDimId)
    {
      String invLocId = null;
      ConsultaEntity entity = new ConsultaEntity();
      String invDimId = InventDimId.Replace("#", "%23");
      String url = "https://ayt.operations.dynamics.com/Data/AYT_InventDimV2?%24filter=inventDimId%20eq%20%27" + invDimId + "%27&%24select=InventLocationId";
      var result = entity.QueryEntity(url);
      try
      {
        if (result.Result.Content != null)
        {
          var response = JsonConvert.DeserializeObject<dynamic>(result.Result.Content);
          invLocId = response.value[0].InventLocationId;
        }
        else
        {
          return "-UNKNOWN";
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return "-UNKNOWN";
      }
      return (invLocId != "") ? invLocId : "-UNKNOWN";
    }
    public static Double GetCOGS(String ItemId, String InventLocationId)
    {
      try
      {

        List<InventTransResponse> responseInvTrans = new List<InventTransResponse>();
        //ParallelQuery<InventTransResponseValues> lista = null;
        IEnumerable<InventTransResponseValues> lista = null;
        IEnumerable<InventTransResponseValues> listaResponse = null;
        ConsultaEntity entity = new ConsultaEntity();
        String url = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=ItemId%20eq%20%27" + ItemId + "%27%20and%20DateFinancial%20eq%202020-01-02T00%3A00%3A00Z&%24select=CostAmountPosted%2CCostAmountAdjustment%2CInventDimId%2CItemId";
        var result = entity.QueryEntity(url);
        var response = JsonConvert.DeserializeObject<InventTransResponse>(result.Result.Content);
        listaResponse = (IEnumerable<InventTransResponseValues>)from val in response.value.AsEnumerable() /*where val.InventLocationId == InventLocationId*/ select val;

        Parallel.ForEach(listaResponse, (invTrans) => {
          String ilid = GetInventLocationId(invTrans.InventDimId);
          if (ilid != null)
          {
            invTrans.InventLocationId = ilid;
          }
        });


        lista = (IEnumerable<InventTransResponseValues>)from val in response.value.AsEnumerable() where val.InventLocationId == InventLocationId select val;
        /*var costAmountPosted = (from val in lista.AsEnumerable() group val by val.CostAmountPosted into capGroup select new { sumaPosted = capGroup.Sum(x => x.CostAmountPosted) });//lista.Select(x => x.CostAmountPosted).Sum();
        var costAmountAdjustment = (from val in lista.AsEnumerable() group val by val.CostAmountAdjustment into caaGroup select new { sumaAdjust = caaGroup.Sum(x => x.CostAmountAdjustment) });// lista.Select(x => x.CostAmountAdjustment).Sum();
        var cogsSum = from val in lista.AsEnumerable() select (val.CostAmountPosted, val.CostAmountAdjustment, val.InventLocationId) ;

        
        var iteratorPosted = costAmountPosted.GetEnumerator();
        var iteratorAdjust = costAmountAdjustment.GetEnumerator();
        var iteratorCogs = cogsSum.GetEnumerator();*/
        var iteratorLista = listaResponse.GetEnumerator();
        Double cap = 0;
        Double caa = 0;
        Double cogs = 0;


        /*while (iteratorLista.MoveNext())
        {
          caa += iteratorLista.Current.CostAmountAdjustment;
          cap += iteratorLista.Current.CostAmountPosted;
        }*/
        if (lista != null) {
          Parallel.ForEach(lista, (invTrans) => {
            caa += invTrans.CostAmountAdjustment;
            cap += invTrans.CostAmountPosted;
          });
        }

        cogs = Math.Abs(cap) - caa;


        return cogs;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return 0.0;
      }
    }
    public static InventTransResponseModel GetInventTransCosts(String ItemId)
    {
      InventTransResponseModel respuesta = new InventTransResponseModel();
      try
      {
        ConsultaEntity entity = new ConsultaEntity();
        String url = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=ItemId%20eq%20%27" + ItemId + "%27%20and%20DateFinancial%20eq%202020-01-02T00%3A00%3A00Z&%24select=CostAmountPosted%2CCostAmountAdjustment%2CInventDimId%2CItemId";
        var result = entity.QueryEntity(url);
        respuesta = JsonConvert.DeserializeObject<InventTransResponseModel>(result.Result.Content);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return respuesta;
      }
      return respuesta;
    }
    public static void InsertJsonSourceInvTrans(JsonTextModel source)
    {
      String truncate = "TRUNCATE TABLE AYT_JsonSourceInvTrans";
      int rows = dbayt03.queryInsert(truncate);
      var copyParameters = new[]
      {
        "idJsonText",
        "jsonText",
      };
      var AppSettings = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();
      String connStr = AppSettings["DB_AYT03"];
      using (var sqlCopy = new SqlBulkCopy(connStr))
      {
        sqlCopy.DestinationTableName = "[AYT_JsonSourceInvTrans]";
        sqlCopy.BatchSize = 5000;
        sqlCopy.BulkCopyTimeout = 1000;
        var s = source.value.jsonText.Select(x => new {
          jsonText = x
        });
        using (var readerProducts = ObjectReader.Create(s, copyParameters))
        {
          sqlCopy.WriteToServer(readerProducts);
        }
      }
    }
    public static void InsertInventTransValues()
    {
      String truncate = "TRUNCATE TABLE AYT_InventTransV2_BYOD";
      int rows = dbayt03.queryInsert(truncate);
      var copyParameters = new[]
      {
        "idInventTransV2",
        "dataAreaId",
        "DateFinancial",
        "Qty",
        "CostAmountOperations",
        "CostAmountAdjustment",
        "InventDimId",
        "InventLocationId",
        "QtySettled",
        "InvoiceReturned",
        "CostAmountPosted",
        "CurrencyCode",
        "StatusIssue",
        "Voucher",
        "ItemId",
        "InvoiceId",
        "CostAmountPhysical",
        "StatusReceipt",
      };
      var AppSettings = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .Build();
      String connStr = AppSettings["DB_AYT03"];
      using (var sqlCopy = new SqlBulkCopy(connStr))
      {
        sqlCopy.DestinationTableName = "[AYT_InventTransV2_BYOD]";
        sqlCopy.BatchSize = 5000;
        sqlCopy.BulkCopyTimeout = 1000;
        var s1 = new Stopwatch();
        s1.Start();
        var s = totalInventTransValues.Select(x => new
        {
          dataAreaId = x.dataAreaId,
          DateFinancial = x.DateFinancial,
          Qty = x.Qty,
          CostAmountOperations = x.CostAmountOperations,
          CostAmountAdjustment = x.CostAmountAdjustment,
          InventDimId = x.InventDimId,
          InventLocationId = x.InventLocationId,
          QtySettled = x.QtySettled,
          InvoiceReturned = x.InvoiceReturned,
          CostAmountPosted = x.CostAmountPosted,
          CurrencyCode = x.CurrencyCode,
          StatusIssue = x.StatusIssue,
          Voucher = x.Voucher,
          ItemId = x.ItemId,
          InvoiceId = x.InvoiceId,
          CostAmountPhysical = x.CostAmountPhysical,
          StatusReceipt = x.StatusReceipt,
        });
        s1.Stop();
        Console.WriteLine("tardo object to insert: {0} milisegundos", s1.ElapsedMilliseconds);
        using (var readerProducts = ObjectReader.Create(s, copyParameters))
        {
          sqlCopy.WriteToServer(readerProducts);
        }
      }
    }
  }
  public class InventTransValuesModel
  {
    public String ItemId { get; set; }
    public String InventDimId { get; set; }
    public Double CostAmountPosted { get; set; }
    public Double CostAmountAdjustment { get; set; }
  }
  public class InventTransResponseModel
  {
    public List<InventTransValuesModel> value { get; set; }
  }
  public class JsonTextModel
  {
    public JsonTextValueModel value { get; set; }
  }
  public class JsonTextValueModel
  {
    public List<String> jsonText { get; set; }
  }
}
