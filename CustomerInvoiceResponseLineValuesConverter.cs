using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class CustomerInvoiceResponseLineValuesConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      if (objectType == typeof(List<CustomerInvoiceTransValues>))
      {
        return true;
      }

      return false;
    }
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      try
      {
        var model = new CustomerInvoiceTransValues();
        var modelList = new List<CustomerInvoiceTransValues>();
        string[] keys = new string[] { "etag", "Qty", "TaxAmountMST", "InventQty", "LineAmountMST", "dataAreaId", "StockedProduct", "InvoiceDate", "CurrencyCode", "ItemId", "SalesUnit", "InventDimId", };
        int count = 0;
        while (reader.Read())
        {
          reader.Read();
          string sKeyResult = keys.FirstOrDefault<String>(s => reader.Path.Contains(s));
          switch (sKeyResult)
          {
            case "etag":
             model.etag = (string)serializer.Deserialize(reader);
              break;
            case "Qty":
              model.Qty = Convert.ToDouble(serializer.Deserialize(reader));
              break;
            case "TaxAmountMST":
              model.TaxAmountMST = Convert.ToDouble(serializer.Deserialize(reader));
              break;
            case "InventQty":
              model.InventQty = Convert.ToDouble(serializer.Deserialize(reader));
              break;
            case "LineAmountMST":
              model.LineAmountMST = Convert.ToDouble(serializer.Deserialize(reader));
              break;
            case "dataAreaId":
              model.dataAreaId = (string)serializer.Deserialize(reader);
              break;
            case "StockedProduct":
              model.StockedProduct = (string)serializer.Deserialize(reader);
              break;
            case "InvoiceDate":
              model.InvoiceDate = Convert.ToDateTime(serializer.Deserialize(reader));
              break;
            case "CurrencyCode":
              model.CurrencyCode = (string)serializer.Deserialize(reader);
              break;
            case "ItemId":
              model.ItemId = (string)serializer.Deserialize(reader);
              break;
            case "SalesUnit":
              model.SalesUnit = (string)serializer.Deserialize(reader);
              break;
            case "InventDimId":
              model.InventDimId = (string)serializer.Deserialize(reader);
              break;
            default:
              Console.WriteLine("sKeyResult(count): " + count +" "+ sKeyResult);
              if (sKeyResult != null || sKeyResult != "")
              {
                modelList.Add(model);
              }
              count++;
              break;
          }
        }
        /*Parallel.ForEach(modelList, mdl => {
          Type t = mdl.GetType();
          PropertyInfo propInfo = t.GetProperty("InventLocationId");
          MethodInfo[] methInfos = propInfo.GetAccessors();
          MethodInfo m = methInfos[0];
          m.Invoke(mdl, new object[] { });
        });*/
        reader.Read();
        return modelList;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        return null;
      }
    }
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}
