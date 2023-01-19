using System;
using System.Collections.Generic;
using System.Text;

namespace BYOD_SalesCube_CustomerInvoiceLines
{
  public class Clientes
  {
    public String RFCNumber { get; set; }
    public String OrganizationName { get; set; }
    public String InvoiceAddressZipCode { get; set; }
    public String AddressZipCode { get; set; }
  }
  public class ClientesResponse
  {
    public List<Clientes> value { get; set; }
  }
}
