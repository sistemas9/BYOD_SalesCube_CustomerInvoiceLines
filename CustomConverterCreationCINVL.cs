using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class CustomConverterCreationCINVL : JsonCreationConverter<CustomerInvoiceTransValues>
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    protected override CustomerInvoiceTransValues Create(Type objectType, JObject jObject)
    {
      return new CustomerInvoiceTransValues();
    }
  }
  public abstract class JsonCreationConverter<T> : JsonConverter
  {
    /// <summary>
    /// Create an instance of objectType, based properties in the JSON object
    /// </summary>
    /// <param name="objectType">type of object expected</param>
    /// <param name="jObject">
    /// contents of JSON object that will be deserialized
    /// </param>
    /// <returns></returns>
    protected abstract T Create(Type objectType, JObject jObject);
    public List<InventTransResponse> invTransResponse = new List<InventTransResponse>();
    public List<InventTransResponseValues> invTransResponseValues = new List<InventTransResponseValues>();
    public override bool CanConvert(Type objectType)
    {
      return typeof(T).IsAssignableFrom(objectType);
    }

    public override bool CanWrite
    {
      get { return false; }
    }

    public override object ReadJson(JsonReader reader,
                                    Type objectType,
                                     object existingValue,
                                     JsonSerializer serializer)
    {
      // Load JObject from stream
      JObject jObject = JObject.Load(reader);

      // Create target object based on JObject
      T target = Create(objectType, jObject);

      // Populate the object properties
      serializer.Populate(jObject.CreateReader(), target);
      Type t = target.GetType();
      PropertyInfo propInfo2 = t.GetProperty("COGS");
      MethodInfo methInfos2 = propInfo2.GetGetMethod();   
      var cinvl = (target as CustomerInvoiceTransValues);
      var ilid = cinvl._InventLocationId(cinvl.InventDimId);      
      var itid = cinvl.ItemId;
      DateTime inicio = DateTime.Now;
      var listInvTransValues = SetInvTransValues(itid, ilid);
      cinvl.__COGS = GetCOGS(listInvTransValues);
      Console.WriteLine("tardo : {0} segundos", (DateTime.Now - inicio).TotalSeconds);
      if ((DateTime.Now-inicio).TotalSeconds > 20)
      {
        Console.WriteLine(cinvl.ItemId + " - " + cinvl.InventDimId + " - " + cinvl.InventLocationId);
      }
      return target;
    }
    public ParallelQuery<InventTransResponseValues> SetInvTransValues(String ItemId, String InventLocationId)
    {
      List<InventTransResponse> responseInvTrans = new List<InventTransResponse>();
      ParallelQuery<InventTransResponseValues> listInvTrans = null;
      Task.Factory.StartNew(() =>
      {
        ConsultaEntity entity = new ConsultaEntity();
        String url = "https://ayt.operations.dynamics.com/Data/AYT_InventTransV2?%24filter=ItemId%20eq%20%27" + ItemId + "%27%20and%20DateFinancial%20eq%202020-01-02T00%3A00%3A00Z&%24select=CostAmountPosted%2CCostAmountAdjustment%2CInventDimId";
        var result = entity.QueryEntity(url);
        try
        {
          var response = JsonConvert.DeserializeObject<InventTransResponse>(result.Result.Content);
          //int count = 0;
          //List<String> lista = new List<string>() { "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", "CHIHCONS", };
          /*Parallel.ForEach(response.value.AsEnumerable(), val => {
            var locationId = InventLocationId;
            if (val.InventLocationId == locationId)
            {
              invTransResponseValues.Add(val);
            }
          });*/
          listInvTrans = from val in response.value.AsParallel() where val.InventLocationId == InventLocationId select val;
          //invTransResponseValues = listInvTrans.ToList();
          /*var locationId = InventLocationId;
          foreach (var val in response.value)
          {
            if (val.InventLocationId == locationId)
            {
              invTransResponseValues.Add(val);
            }
          }*/
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }).Wait();
      //responseInvTrans.SelectMany(x => x.value).ToList().Where(x => x.InventLocationId == InventLocationId).ToList();
      return listInvTrans;
    }
    public Double GetCOGS(ParallelQuery<InventTransResponseValues> lista)
    {
      try
      {
        var costAmountPosted = (from val in lista.AsEnumerable() group val by val.CostAmountPosted into capGroup select new { sumaPosted = capGroup.Sum(x => x.CostAmountPosted) });//lista.Select(x => x.CostAmountPosted).Sum();
        var costAmountAdjustment = (from val in lista.AsEnumerable() group val by val.CostAmountAdjustment into caaGroup select new { sumaAdjust = caaGroup.Sum(x => x.CostAmountAdjustment) });// lista.Select(x => x.CostAmountAdjustment).Sum();
        var iteratorPosted = costAmountPosted.GetEnumerator();
        var iteratorAdjust = costAmountAdjustment.GetEnumerator();
        Double cap = 0;
        Double caa = 0;
        while(iteratorPosted.MoveNext())
        {
          cap = iteratorPosted.Current.sumaPosted;
        }
        while (iteratorAdjust.MoveNext())
        {
          caa = iteratorAdjust.Current.sumaAdjust;
        }
        Double cogs = Math.Abs(cap) - caa;
        return cogs;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return 0.0;
      }
    }
  }
}
