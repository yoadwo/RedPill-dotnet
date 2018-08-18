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
    }
}
