using PrescriptionsDataAccess;
using RedPill_dotnet.Utils;
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

        public const string PILL_PREFIX = "FSPILLSEN@";
        //[BasicAuthentication]
        public IEnumerable<Prescription> Get()
        {
            //string docID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            using (RedPillEntities entities = new RedPillEntities())
            {
                return entities.Prescriptions.ToList();
                //return entities.Prescriptions.Where(doc => doc.docID.Equals(docID)).ToList();
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
            
            if (pre == null) {
                var message = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Prescription object could not be parsed, check request body");
                return message;
            }
            try
            {
                // create response
                var message = Request.CreateResponse(HttpStatusCode.Created, pre);
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

                // redirect to the uri of newly created object
                // does not compile, pres.id is auto generated on server
                //message.Headers.Location = new Uri(Request.RequestUri +  pre..ToString());

                using (RedPillEntities entities = new RedPillEntities())
                {
                    pre.timeAdded = DateTime.Now;
                    // add info to server
                    entities.Prescriptions.Add(pre);
                    entities.SaveChanges();
                }
                // create qr code
                QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
                QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(PILL_PREFIX + pre.info, QRCoder.QRCodeGenerator.ECCLevel.Q);
                QRCoder.Base64QRCode qrCode = new QRCoder.Base64QRCode(qrCodeData);
                string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
                message.Content = new StringContent(qrCodeImageAsBase64);
                return message;

                #region bitmap
                /*
                QRCoder.QRCode qrCode = new QRCoder.QRCode(qrCodeData);
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
                */
                #endregion
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            
        }

    }
}
