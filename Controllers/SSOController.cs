using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contract.Models;
using System.ServiceModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Contract.DAL;
using Contract.core;


namespace Contract.Controllers
{
    public class SSOController : Controller
    {
        private ContractContext db = new ContractContext();
        bool partyA;
        bool partyB;
        bool key = false;
        // GET: SSO
        public ActionResult Index()
        {
            int user_id = 0;
            string userno = "";
            string UrlReferrer = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"];
            SSOLogin.SSOLoginSoapClient ecsso = new SSOLogin.SSOLoginSoapClient();
            if (ecsso.GetLoginInfo(UrlReferrer, System.Web.HttpContext.Current.Request.Form.Get("key"), ref user_id, ref userno) == "Y")//If ecsso.GetLoginInfo(UrlReferrer, Request.Form.Get("key"), user_id, user_code) = "Y" Then
            {
                Session["userno"] = userno;
                //TempData["userno"] = userno;
                //TempData.Keep();
                var contractu = from u in db.rs_users where u.user_code == userno select u.user_status;
                var contracto = from o in db.rs_users where o.user_code == userno select o.user_name;
                contracto.ToList();
                var contracti = from i in db.HT_user_lists select i.user_code;
                contracti.ToList();

                foreach (string item in contracti)
                {
                    if (userno == item)
                    {
                        key = true;
                        break;
                    }
                }


                string status = contractu.FirstOrDefault().ToString();

                if (((status == "Y") || (status == "S") || (status == "P")) && key)
                {
                    foreach (string item in contracto)
                    {
                        ViewData["user"] = item;
                    }
            
                    ViewData["Message"] = DateTime.Now;

                    return Redirect("~/ContractApply/Index");
                    
                }
                else
                {
                    return Redirect(System.Web.Configuration.WebConfigurationManager.AppSettings["LoginPage"].ToString());
                }
            }
            else
            {
                return Redirect(System.Web.Configuration.WebConfigurationManager.AppSettings["LoginPage"].ToString());
            }
        }

        public ActionResult loginin()
        {
            return Redirect(System.Web.Configuration.WebConfigurationManager.AppSettings["LoginPage"].ToString());
        }
    }
}