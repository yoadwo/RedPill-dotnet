using PrescriptionsDataAccess;
using RedPill_dotnet.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            List<Prescription> pres = null;

            // get results from server using entity framework by patient's id (and measure time) 
            var watchDB = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                pres = ListResultsFromEFbyPatientID(id);
            }
            // log error and respond to client
            catch (System.NotSupportedException nsex)
            {
                LogMessage(new string[] {
                "GET (READ PRESCRIPTIONS by PatientID), FAIL on exception: " + nsex.Message,
                "DB action took " + watchDB.ElapsedMilliseconds + "ms.",
                "Total action took " + watchTotal.ElapsedMilliseconds + "ms."
                });
                var errMessage = Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    nsex.Message);
                return errMessage;
            }
 
            watchDB.Stop();
            var message = CreateMessage(pres);

            watchTotal.Stop();

            LogMessage(new string[] {
                "GET (READ PRESCRIPTIONS by PatientID), SUCCESS",
                "DB action took " + watchDB.ElapsedMilliseconds + "ms.",
                "Total action took " + watchTotal.ElapsedMilliseconds + "ms."
            });
            
            return message;
        }

        /// <summary>
        /// get results from server using entity framework by patient's id (and measure time)
        /// </summary>
        /// <param name="patientID"></param>
        /// <returns></returns>
        private List<Prescription> ListResultsFromEFbyPatientID(int patientID)
        {
            List<Prescription> pres;
            using (RedPillEntities entities = new RedPillEntities())
            {
                pres = entities.Prescriptions.Where(p => p.patientID.Equals(patientID.ToString())).ToList();
            }
            return pres;
        }

        /// <summary>
        /// get a list of all existing prescriptions under a certain pill name
        /// </summary>
        /// <param name="pillName">name of prescribed pill</param>
        /// <returns></returns>
        public HttpResponseMessage GetPresByPillName(string pillName)
        {
            var watchTotal = System.Diagnostics.Stopwatch.StartNew();
            // get results from server using entity framework by pill's name (and measure time)
            var watchDB = System.Diagnostics.Stopwatch.StartNew();            
            List<Prescription> pres = ListResultsFromEFbyPillName(pillName);
            watchDB.Stop();
            // wrap the prescriptions list into HTTP message
            var message = CreateMessage(pres);
            
            // stop measure of total time
            watchTotal.Stop();

            // log
            LogMessage(new string[] {
                "GET (READ PRESCRIPTIONS by pillName), SUCCESS",
                "DB action took " + watchDB.ElapsedMilliseconds + "ms.",
                "Total action took " + watchTotal.ElapsedMilliseconds + "ms."
            });

            return message;
        }

        /// <summary>
        /// // get results from server using entity by pill's name framework (and measure time)
        /// </summary>
        /// <param name="pillName"></param>
        /// <returns></returns>
        private List<Prescription> ListResultsFromEFbyPillName(string pillName)
        {
            List<Prescription> pres;
            using (RedPillEntities entities = new RedPillEntities())
            {
                pres = entities.Prescriptions.Where(p => p.pillName.Equals(pillName)).ToList();
            }
            return pres;
        }

        /// <summary>
        /// wrap the prescriptions list into HTTP message
        /// </summary>
        /// <param name="pres"></param>
        /// <returns></returns>
        private HttpResponseMessage CreateMessage(List<Prescription> pres)
        {
            var message = Request.CreateResponse(pres);
            // if countent found sign with OK status and signal body as json type
            if (pres.Count > 0)
            {
                message.StatusCode = HttpStatusCode.OK;
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            }
            else
            {
                message.StatusCode = HttpStatusCode.NoContent;

            }
            return message;
        }

        /// <summary>
        /// save prescription and receive its QR code
        /// </summary>
        /// <param name="pre">Prescription including doctor ID, patient ID, pill info and date given</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync([FromBody] Prescription pre)
        {
            // System.Diagnostics.Trace.WriteLine("In Precription Controller, POST");
            var watchTotal = Stopwatch.StartNew();
            var watchDB = new Stopwatch();
            var watchQR = new Stopwatch();

            // validate object received from Post rquest
            if (pre == null) {
                var message = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Prescription object could not be parsed, check request body");
                return message;
            }
            try
            {
                // redirect to the uri of newly created object
                // does not compile, pres.id is auto generated on server
                //message.Headers.Location = new Uri(Request.RequestUri +  pre..ToString());

                // post results to server using entity framework (and measure time)
                watchDB.Start();
                //SaveResultsToEF(pre); // SYNC
                Task taskDB = SaveResultsToEFAsync(pre, watchDB); 

                // create response
                var message = Request.CreateResponse(HttpStatusCode.Created, pre);
                message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                
                // create qr code
                watchQR.Start();
                message.Content = GenerateQRImage(PILL_PREFIX + pre.info); // SYNC
                watchQR.Stop();

                await taskDB;
                watchTotal.Stop();

                LogMessage(new string[] {
                    "POST (WRITE PRESCRIPTIONS): SUCCESS",
                    "DB action took " + watchDB.ElapsedMilliseconds + "ms.",
                    "QR action took " + watchQR.ElapsedMilliseconds + "ms.",
                    "Total action took " + watchTotal.ElapsedMilliseconds + "ms."
                });
                
                return message;

            }
            catch (System.Data.DataException ex)
            {
                if (watchDB.IsRunning)
                    watchDB.Stop();
                if (watchQR.IsRunning)
                    watchQR.Stop();
                if (watchTotal.IsRunning)
                    watchTotal.Stop();
                watchTotal.Stop();

                LogMessage(new string[] {
                    "POST (WRITE PRESCRIPTIONS): FAIL on exception: " + ex.Message,
                    "DB action took " + watchDB.ElapsedMilliseconds + "ms.",
                    "QR action took " + watchQR.ElapsedMilliseconds + "ms.",
                    "Total action took " + watchTotal.ElapsedMilliseconds + "ms."
                });
                
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
            
        }

        /// <summary>
        /// save new prescription info to server
        /// </summary>
        /// <param name="pre"></param>
        private void SaveResultsToEF(Prescription pre)
        {
            using (RedPillEntities entities = new RedPillEntities())
            {
                pre.timeAdded = DateTime.Now;
                // add info to server
                entities.Prescriptions.Add(pre);
                entities.SaveChanges();
            }
        }

        /// <summary>
        /// save new prescription info to server - Async
        /// </summary>
        /// <param name="pre"></param>
        private async Task SaveResultsToEFAsync(Prescription pre, Stopwatch stopwatch)
        {
            using (RedPillEntities entities = new RedPillEntities())
            {
                pre.timeAdded = DateTime.Now;
                // add info to server
                entities.Prescriptions.Add(pre);
                await entities.SaveChangesAsync();
            }
            stopwatch.Stop();
        }


        /// <summary>
        /// generate QR Barcode (as image) with info received from the client
        /// </summary>
        /// <param name="codeInfo"></param>
        /// <returns></returns>
        private HttpContent GenerateQRImage(string codeInfo)
        {
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(codeInfo, QRCoder.QRCodeGenerator.ECCLevel.Q);
            QRCoder.Base64QRCode qrCode = new QRCoder.Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(5);
            return new StringContent(qrCodeImageAsBase64);
        }

        /// <summary>
        /// log an array of messages (using stringBuilder)
        /// </summary>
        /// <param name="messages"></param>
        private void LogMessage(string[] messages)
        {
            try
            {
                using (System.IO.StreamWriter w = System.IO.File.AppendText(System.Web.HttpContext.Current.Server.MapPath(@"~/Utils/log.txt")))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string message in messages)
                        sb.AppendLine(message);

                    Log(sb.ToString(), w);
                }
            }
            catch (System.IO.IOException ioex)
            {
                System.Diagnostics.Debug.WriteLine("io error while trying to log exception.", ioex);
            }
        }

        /// <summary>
        /// log a message into a file with date followed by a seperator
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="w">TextWriter</param>
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
