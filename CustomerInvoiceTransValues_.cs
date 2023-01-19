using System;
using System.Collections.Generic;
using System.Text;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class CustomerInvoiceTransValues_
  {
    public Double Qty { get; set; }
    public Double TaxAmountMST { get; set; }
    public Double InventQty { get; set; }
    public Double LineAmountMST { get; set; }
    public String dataAreaId { get; set; }
    public String StockedProduct { get; set; }
    public DateTime InvoiceDate { get; set; }
    public String InvoiceId { get; set; }
    public String CurrencyCode { get; set; }
    public String ItemId { get; set; }
    public String SalesUnit { get; set; }
    public String InventDimId { get; set; }
    public String InventLocationId { get; set; }
    public String SalesId { get; set; }
    public Double COGS { get; set; }
    public Double CostAmountPosted;
    public Double CostAmountAdjustment;
  }
}
