using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class InventTransResponseValues
  {
    public String ItemId { get; set; }
    public String dataAreaId { get; set; }
    public Double Qty { get; set; }
    public String InventDimId { get; set; }
    public DateTime DateFinancial { get; set; }
    public Double CostAmountPosted { get; set; }
    public Double CostAmountAdjustment { get; set; }
    public String InventLocationId;
    public Double CostAmountOperations { get; set; }
    public Double QtySettled { get; set; }
    public String InvoiceReturned { get; set; }
    public String CurrencyCode { get; set; }
    public String StatusIssue { get; set; }
    public String Voucher { get; set; }
    public String InvoiceId { get; set; }
    public Double CostAmountPhysical { get; set; }
    public String StatusReceipt { get; set; }
  }
  public class InventTransResponse
  {
    public List<InventTransResponseValues> value { get; set; }
    public int count { get; set; }
  }
}
