using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PrescriptionsDataAccess;

namespace RedPill_dotnet.Utils
{
    public class DoctorSecurity
    {
        public static bool Login(string docID, string password)
        {
            using (RedPillEntities entities = new RedPillEntities())
            {
                var docsExist = entities.Doctors.Any(doctor => doctor.docID.ToLower().Equals(docID)
                && doctor.password.Equals(password));
                //var docslist = docs.ToList();
                return docsExist;
            }
        }
    }
}