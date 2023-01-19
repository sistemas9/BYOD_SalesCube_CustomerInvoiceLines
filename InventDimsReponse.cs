using System;
using System.Collections.Generic;
using System.Text;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class InventDimsReponse
  {
    public List<InventDimsValues> value { get; set; }
  }
  public class InventDimsValues
  {
    public String dataAreaId { get; set; }
    public String inventDimId { get; set; }
    public String wMSPalletId { get; set; }
    public String ModifiedDateTimeAYT { get; set; }
    public String InventProfileId_RU { get; set; }
    public String CreatedDateTimeAYT { get; set; }
    public String ModifiedByAYT { get; set; }
    public String InventColorId { get; set; }
    public String InventSizeId { get; set; }
    public String wMSLocationId { get; set; }
    public String InventLocationId { get; set; }
    public String InventOwnerId_RU { get; set; }
    public String InventStyleId { get; set; }
    public String InventSiteId { get; set; }
    public String LicensePlateId { get; set; }
    public String SHA1HashHex { get; set; }
    public String InventStatusId { get; set; }
    public String InventGtdId_RU { get; set; }
    public String inventSerialId { get; set; }
    public String configId { get; set; }
    public String inventBatchId { get; set; }
  }
}
