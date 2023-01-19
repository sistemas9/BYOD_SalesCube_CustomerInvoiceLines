using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class CustomerInvoiceResponseLinesConverter : JsonConverter
  {
    public delegate object Thunk(object target, object[] arguments);
    public override bool CanConvert(Type objectType)
    {
      if (objectType == typeof(CustomerInvoiceTransResponse))
      {
        return true;
      }

      return false;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      try
      {
        var model = new CustomerInvoiceTransResponse();
        model.value = new List<CustomerInvoiceTransValues_>();
        int count = 0;
        List<CustomerInvoiceTransValues> modelList = new List<CustomerInvoiceTransValues>();
        List<CustomerInvoiceTransValues_> listTemp = new List<CustomerInvoiceTransValues_>();
        while ( reader.Read() )
        {
          reader.Read();
          switch (reader.Path)
          {
            case "odata":
              model.odata = (string)serializer.Deserialize(reader);
              break;
            case "count":
              model.count = (int)serializer.Deserialize(reader);
              break;
            case "value":
              while(reader.TokenType != JsonToken.EndArray)
              {
                reader.Read();
                if (reader.Path != "value")
                {
                  JObject obj2 = JObject.Load(reader);
                  String json2 = JsonConvert.SerializeObject(obj2);
                  listTemp.Add((CustomerInvoiceTransValues_)obj2.ToObject(typeof(CustomerInvoiceTransValues_)));
                }
              }
              /*Parallel.ForEach(listTemp.Take(3), custInvValue => {
                String json = JsonConvert.SerializeObject(custInvValue);
                var cinvl = JsonConvert.DeserializeObject<CustomerInvoiceTransValues>(json, new CustomConverterCreationCINVL());
              });*/
              var s = new Stopwatch();
              
              var iter = listTemp.GetEnumerator();
              /*while (iter.MoveNext())
              {
                s.Start();
                var ilid = iter.Current._InventLocationId(iter.Current.InventDimId);
                var itid = iter.Current.ItemId;
                //iter.Current.GetInventTransValues(itid, ilid);
                var listInvTransValues = SetInvTransValues(itid, ilid);
                //var cogs = iter.Current._COGS(listInvTransValues);
                s.Stop();
                Console.WriteLine("tardo: {0} milisegundos", s.ElapsedMilliseconds);
                s.Reset();
              }*/
              
              model.value = listTemp;// JsonConvert.DeserializeObject<List<CustomerInvoiceTransValues>>(json, new CustomConverterCreationCINVL());// serializer.Deserialize<List<CustomerInvoiceTransValues>>(reader);
              break;
          }
        }
        /*
        DateTime inicio = DateTime.Now;
        Console.WriteLine("inicio: " + inicio.ToString());
        int[] listaIndexes = new int[model.value.Take(2).ToList().Count];
        for (int i = 0; i < model.value.Take(2).ToList().Count; i++)
        {
          listaIndexes[i] = i;
        }
        
        Parallel.ForEach(listaIndexes.AsEnumerable(), index => {
          Task.Factory.StartNew(() => 
          { 
            var mdl = model.value.Take(2).ToList();
            Type t = mdl[index].GetType();
            PropertyInfo propInfo2 = t.GetProperty("COGS");
            MethodInfo methInfos2 = propInfo2.GetGetMethod();
            //methInfos2.Invoke(mdl, new object[] { });
            var m = mdl[index].COGS;
            //var get = (Func<Double>)Delegate.CreateDelegate(typeof(Func<Double>), methInfos2);
            //Thread.Sleep(500);
            //get.Invoke();
          }).Wait();
          
          
        });
        DateTime fin = DateTime.Now;
        Console.WriteLine("tardo: "+(fin - inicio).TotalSeconds+" segundos");*/
        return model;
      }catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
        return null;
      }
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
    public void InvokeInventLocationId(CustomerInvoiceTransResponse model)
    {
      Parallel.ForEach(model.value.Take(10), mdl => {
        Type t = mdl.GetType();
        PropertyInfo propInfo = t.GetProperty("InventLocationId");
        //PropertyInfo propInfo2 = t.GetProperty("COGS");
        MethodInfo[] methInfos = propInfo.GetAccessors();
        //MethodInfo[] methInfos2 = propInfo2.GetAccessors();
        MethodInfo m = methInfos[0];
        //MethodInfo m2 = methInfos2[0];
        m.Invoke(mdl, new object[] { });
        //m2.Invoke(mdl, new object[] { });
      });
    }
    public List<CustomerInvoiceTransValues> GetListFromJson()
    {
      return new List<CustomerInvoiceTransValues>();
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
          listInvTrans = from val in response.value.AsParallel() where val.InventLocationId == InventLocationId select val;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }).Wait();
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
        while (iteratorPosted.MoveNext())
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
