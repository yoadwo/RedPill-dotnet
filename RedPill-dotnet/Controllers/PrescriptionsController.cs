using PrescriptionsDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RedPill_dotnet.Controllers
{
    public class PrescriptionsController : ApiController
    {
        //<add name="RedPillEntities" connectionString="metadata=res://*/PrescriptionsDataModel.csdl|res://*/PrescriptionsDataModel.ssdl|res://*/PrescriptionsDataModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=YOAD\SQLEXPRESS;initial catalog=RedPill;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />
        public const string PillProtocolPrefix = "FSPILLSEN@";
        public IEnumerable<Prescription> Get()
        {
            using (RedPillEntities entities = new RedPillEntities())
            {
                return entities.Prescriptions.ToList();
            }
        }

        public Prescription Get(int id)
        {
            using (RedPillEntities entities = new RedPillEntities())
            {
                return entities.Prescriptions.FirstOrDefault(p => p.recordID == id);
            }
        }

        public HttpResponseMessage Post([FromBody] Prescription pre)
        {
            try
            {
                using (RedPillEntities entities = new RedPillEntities())
                {
                    // add info to server
                    entities.Prescriptions.Add(pre);
                    entities.SaveChanges();
                }
                // create qr code
                QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
                QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(pre.info, QRCoder.QRCodeGenerator.ECCLevel.Q);
                QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);

                // create http response
                var message = Request.CreateResponse(HttpStatusCode.Created, pre);
                // redirect to the uri of newly created object
                // does not compile, pres.id is auto generated on server
                //message.Headers.Location = new Uri(Request.RequestUri +  pre..ToString());

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(10))
                    {
                        qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    
                    // append to message the qr image
                    message.Content = new ByteArrayContent(ms.ToArray());
                    message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                }
                return message;    
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            
        }

        private string PrescriptiontoQR(Prescription pre)
        {
            string info = pre.info;
            int start = info.IndexOf(PillProtocolPrefix);
            int length = PillProtocolPrefix.Length;
            return info.Substring(start + length);
        }
    }
}
