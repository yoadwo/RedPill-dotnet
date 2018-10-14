using PrescriptionsDataAccess;
using RedPill_dotnet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RedPill_dotnet.Controllers.api
{
    public class PrescriptionsController : ApiController
    {

        public const string PILL_PREFIX = "FSPILLSEN@";


        /// <summary>
        /// get a list of all existing prescriptions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Prescription> Get()
        {
            System.Diagnostics.Trace.WriteLine("In Precription Controller, GET");
            //string docID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            using (RedPillEntities entities = new RedPillEntities())
            {
                return entities.Prescriptions.ToList();
                //return entities.Prescriptions.Where(doc => doc.docID.Equals(docID)).ToList();
            }
        }

        /// <summary>
        /// get a list of all existing prescriptions that belong to a certain patient
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns></returns>
        public HttpResponseMessage GetPresByPatient(int id)
        {
            System.Diagnostics.Trace.WriteLine("In Precription Controller, GET /{id}");
            List<Prescription> pres;
            using (RedPillEntities entities = new RedPillEntities())
            {
                pres = entities.Prescriptions.Where(p => p.patientID.Equals(id.ToString())).ToList();
            }
            if (pres.Count > 0)
            {
                var message = Request.CreateResponse(HttpStatusCode.OK, pres);
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return message;
            }
            else
            {
                var message = Request.CreateResponse(HttpStatusCode.NoContent);
                return message;
            }
        }

        /// <summary>
        /// get a list of all existing prescriptions under a certain pill name
        /// </summary>
        /// <param name="pillName">name of prescribed pill</param>
        /// <returns></returns>
        public HttpResponseMessage GetPresByPillName(string pillName)
        {
            List<Prescription> pres;
            using (RedPillEntities entities = new RedPillEntities())
            {
                pres = entities.Prescriptions.Where(p => p.pillName.Equals(pillName)).ToList();
            }
            if (pres.Count > 0)
            {
                var message = Request.CreateResponse(HttpStatusCode.OK, pres);
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return message;
            }
            else
            {
                var message = Request.CreateResponse(HttpStatusCode.NoContent);
                return message;
            }
        }

        /// <summary>
        /// save prescription and receive its QR code
        /// </summary>
        /// <param name="pre">Prescription including doctor ID, patient ID, pill info and date given</param>
        /// <returns></returns>
        public HttpResponseMessage Post([FromBody] Prescription pre)
        {
            // System.Diagnostics.Trace.WriteLine("In Precription Controller, POST");
            var watchDB = System.Diagnostics.Stopwatch.StartNew();
            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here

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
                    watchDB.Stop();
                    
                }
                // create qr code
                QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
                QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(PILL_PREFIX + pre.info, QRCoder.QRCodeGenerator.ECCLevel.Q);
                QRCoder.Base64QRCode qrCode = new QRCoder.Base64QRCode(qrCodeData);
                string qrCodeImageAsBase64 = qrCode.GetGraphic(5);
                message.Content = new StringContent(qrCodeImageAsBase64);
                watchTotal.Stop();

                using (System.IO.StreamWriter w = System.IO.File.AppendText(System.Web.HttpContext.Current.Server.MapPath(@"~/Utils/log.txt")))
                {
                    Log("DB action took " + watchDB.ElapsedMilliseconds + "ms.", w);
                    Log("Total action took " + watchTotal.ElapsedMilliseconds + "ms.", w);
                }
                
                
                return message;

            }
            catch (Exception ex)
            {
                watchDB.Stop();
                watchTotal.Stop();
                using (System.IO.StreamWriter w = System.IO.File.AppendText(System.Web.HttpContext.Current.Server.MapPath(@"~/Utils/log.txt")))
                {
                    Log("Exception: " + ex.Message + ", DB action took " + watchDB.ElapsedMilliseconds + "ms.", w);
                    Log("Exception: " + ex.Message + ", Total action took " + watchTotal.ElapsedMilliseconds + "ms.", w);
                }
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            
        }

        public static void Log(string logMessage, System.IO.TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.Write("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

    }
}
