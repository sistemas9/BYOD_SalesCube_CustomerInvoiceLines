using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  //[JsonConverter(typeof(CustomerInvoiceResponseLinesConverter))]
  public class CustomerInvoiceTransResponse
  {
    public int count { get; set; }
    public String odata { get; set; }
    public List<CustomerInvoiceTransValues_> value { get; set; }
  }
  //[JsonConverter(typeof(CustomerInvoiceResponseLineValuesConverter))]
  public class CustomerInvoiceTransValues
  {
    public String etag { get; set; }
    public Double Qty { get; set; }
    public Double TaxAmountMST { get; set; }
    public Double InventQty { get; set; }
    public Double LineAmountMST { get; set; }
    public String dataAreaId { get; set; }
    public String StockedProduct { get; set; }
    public DateTime InvoiceDate { get; set; }
    public String CurrencyCode { get; set; }
    public String ItemId { get; set; }
    public String SalesUnit { get; set; }
    public String InventDimId { get; set; }
    public Double CostAmountPosted;
    public Double CostAmountAdjustment;
    public String InventLocationId {
      get 
      {
        return this._InventLocationId(this.InventDimId);
      }
      set 
      {
        this.__InventLocationId = value;
      }
    }
    private List<InventTransResponseValues> InvTransRespValues {
      get
      {
        this.GetInventTransValues(this.ItemId, this.InventLocationId);
        return null;
      }
    }
    public Double COGS
    {
      get
      {
        return this._COGS(this._InventTransReponseValues);
      }
    }
    public Double __COGS;
    public String _InventLocationId(String InventDimId)
    {
      String invLocId = null;
      ConsultaEntity entity = new ConsultaEntity();
      String invDimId = InventDimId.Replace("#", "%23");
      String url = "https://ayt.operations.dynamics.com/Data/AYT_InventDimV2?%24filter=inventDimId%20eq%20%27" + invDimId + "%27&%24select=InventLocationId";
      var result = entity.QueryEntity(url);
      try
      {
        var response = JsonConvert.DeserializeObject<dynamic>(result.Result.Content);
        invLocId = response.value[0].InventLocationId;
        invDimId = invLocId;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        invDimId = null;
      }
      this.__InventLocationId = invDimId;
      return invDimId;
    }
    public String __InventLocationId;
    public ParallelQuery<InventTransResponseValues> _InventTransReponseValues;
    public Double _COGS(ParallelQuery<InventTransResponseValues> _InventTransReponseValues)
    { 
      try
      {
        /*Double costAmountPosted = _InventTransReponseValues.Select(x => x.CostAmountPosted).Sum();
        Double costAmountAdjustment = _InventTransReponseValues.Select(x => x.CostAmountAdjustment).Sum();
        Double cogs = Math.Abs(costAmountPosted) - costAmountAdjustment;*/
        var costAmountPosted = (from val in _InventTransReponseValues.AsEnumerable() group val by val.CostAmountPosted into capGroup select new { sumaPosted = capGroup.Sum(x => x.CostAmountPosted) });//lista.Select(x => x.CostAmountPosted).Sum();
        var costAmountAdjustment = (from val in _InventTransReponseValues.AsEnumerable() group val by val.CostAmountAdjustment into caaGroup select new { sumaAdjust = caaGroup.Sum(x => x.CostAmountAdjustment) });// lista.Select(x => x.CostAmountAdjustment).Sum();
        var iteratorPosted = costAmountPosted.GetEnumerator();
        var iteratorAdjust = costAmountAdjustment.GetEnumerator();
        Double cap = 0;
        Double caa = 0;
        while (iteratorPosted.MoveNext())
        {
          cap = iteratorPosted.Current.sumaPosted;
        }
        while (iteratorAdjust.MoveNext())
        {
          caa = iteratorAdjust.Current.sumaAdjust;
        }
        Double cogs = Math.Abs(cap) - caa;
        this.__COGS = cogs;
        return cogs;
      }catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
        return 0.0;
      }
    }
    public void GetInventTransValues(String ItemId,String InventLocationId) {
      List<InventTransResponse> responseInvTrans = new List<InventTransResponse>();
      Task.Factory.StartNew(() =>
      {
        ConsultaEntity entity = new ConsultaEntity();
        String url = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=ItemId%20eq%20%27" + ItemId + "%27%20and%20DateFinancial%20eq%202020-01-02T00%3A00%3A00Z&%24select=CostAmountPosted%2CCostAmountAdjustment%2CInventDimId";
        var result = entity.QueryEntity(url);
        try
        {
          var response = JsonConvert.DeserializeObject<InventTransResponse>(result.Result.Content);
          responseInvTrans.Add(response);
          this._InventTransReponseValues = from val in response.value.AsParallel() where val.InventLocationId == InventLocationId select val;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          return;
        }
      }).Wait();
      //List<InventTransResponseValues> invTransResponseValues = responseInvTrans.SelectMany(x => x.value).ToList();
      /*List<InventTransResponseValues> invTransResponseValues = new List<InventTransResponseValues>();
      var iterResp = responseInvTrans.GetEnumerator();
      while (iterResp.MoveNext())
      {
        var iterValues = iterResp.Current.value.GetEnumerator();
        while (iterValues.MoveNext())
        {
          invTransResponseValues.Add(iterValues.Current);
        }
      }
      this._InventTransReponseValues = invTransResponseValues;*/
    }
  }
}
