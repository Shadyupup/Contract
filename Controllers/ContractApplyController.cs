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
using System.Data.Entity;

namespace Contract.Controllers
{
    public class ContractApplyController : Controller
    {

        private ContractContext db = new ContractContext();
        bool partyA;
        bool partyB;
        int? Party_A_apply_employee_no2;
        int? Party_B_apply_employee_no2;
        bool key = false;
        int? nextuser = null;
        //int? areaBoss;
        //int k = 0;
        public ActionResult Index()
        {
            string userno = Session["userno"].ToString();
            var user_name = from u in db.rs_users where u.user_code == userno select u.user_name;
            string username=user_name.FirstOrDefault();
            ViewData["user"] = username;

            viewModel vm = new viewModel(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            apply applyshow = new apply();

            vm = applyshow.ContractApplyShow();
            ViewData["Message"] = DateTime.Now;

            return View("ContractApply", vm);
        }

        public int? getgroup_id(int? audit_employee_no)
        {
            string audit_no = audit_employee_no.ToString();
            var audit_group_ids = from a in db.rs_users where a.user_code == audit_no select a.group_id;
            audit_group_ids.ToArray();
            foreach (int item in audit_group_ids)
            {
                if (string.IsNullOrEmpty(item.ToString()))
                {
                    return (null);
                }
                else
                {
                    return (int.Parse(item.ToString()));
                }
            }
            return null;
        }
        public string getlevel_mode(int id_level)
        {
            var audit_level_mode = from a in db.rs_levels where a.level_id == id_level select a.level_main;
            return (audit_level_mode.ToString());
        }

        public string getlevel_mode_deep(int id_level_deep)
        {
            var audit_level_mode = from a in db.rs_levels where a.level_id == id_level_deep select a.level_code;
            return (audit_level_mode.ToString());
        }


        public int? getname_bygroup(int? bygroup_id, int? bylevel_id)
        {
            var bygroups = from a in db.rs_users
                           where a.group_id == bygroup_id && a.level_id == bylevel_id
                           select a.user_code;
            bygroups.ToArray();
            if (bygroups.Count() != 0)
            {
                foreach (string items in bygroups)
                {
                    return (int.Parse(items.ToString()));
                }
                return null;
            }
            return null;

        }

        public string getgroup_name(int? idgroup)
        {
            var name_groups = from a in db.rs_groups where a.group_id == idgroup select a.group_name;
            return (name_groups.ToString());
        }

        public int getuserid(int? empnum)
        {
            string code = Convert.ToString(empnum);
            var userno = from t in db.rs_users where t.user_code == code select t.user_id;
            userno.ToArray();
            if (userno.Count() != 0)
            {
                foreach (int item in userno)
                {
                    return item;
                }
                return 0;
            }
            else
            {
                return 0;
            }

        }

        public string checkht(string tmp1, string tmp3, string tmp6, string tmp4, string tmp9)
        {
            tmp1 = tmp1.Trim();
            tmp3 = tmp3.Trim();//Party_A_apply_employee_no
            tmp6 = tmp6.Trim();//Party_A_apply_name
            tmp4 = tmp4.Trim();//Party_B_apply_employee_no
            tmp9 = tmp9.Trim();//Party_B_apply_name

            partyA = false;
            partyB = false;
            var contractq = from q in db.HT_Mains where q.ht_no == tmp1 select q.ht_id;
            contractq.ToList();
            if (contractq.Count() != 0)
            {
                return "nosame";//合同编号重复，请重新输入！"
            }
            else
            {
                if (tmp1.Length < 10)
                {
                    return "noshort";//合同编号小于10个字符，请重新输入！
                }
                //return "noright";
            }
            if (tmp3 != null)
            {
                tmp6 = tmp6.Trim();
                string tmp_code = Convert.ToString(tmp3);
                var contractx = from x in db.rs_users where x.user_code == tmp_code select x.user_name;
                contractx.ToArray();
                foreach (string item in contractx)
                {
                    if (tmp6 != item)
                    {
                        partyA = false;
                    }
                    else
                    {
                        partyA = true;
                    }
                }
            }
            if (tmp4 != null)
            {
                tmp9 = tmp9.Trim();
                string tmp_code2 = Convert.ToString(tmp4);
                var contracty = from x in db.rs_users where x.user_code == tmp_code2 select x.user_name;
                contracty.ToArray();
                foreach (string item in contracty)
                {
                    if (tmp9 != item)
                    {
                        partyB = false;
                    }
                    else
                    {
                        partyB = true;
                    }
                }
            }
            if (!(partyA || partyB))
            {
                return "nameinvalid"; //甲方或乙方工号与姓名不符，请检查后重新输入！
            }
            else
            {
                string A_no = tmp3.ToString();
                string B_no = tmp4.ToString();
                var contracti = from i in db.rs_users where i.user_code == A_no && (i.user_status == "S" || i.user_status == "Y" || i.user_status == "P") select i.user_id;
                contracti.ToList();
                var contractz = from j in db.rs_users where j.user_code == B_no && (j.user_status == "S" || j.user_status == "Y" || j.user_status == "P") select j.user_id;
                contractz.ToList();
                if ((contracti.Count() == 0) && (contractz.Count() == 0))
                {
                    return "noright";//甲方或乙方没有申请权限，请检查后重新输入！
                }
                else
                {
                    return "T";
                }
            }
        }
        [HttpPost]
        public ActionResult Index(HT_Main hT_Main,HT_audit_detail hT_audit_detail, HT_audit_detail hT_audit_detail1, HT_audit_detail hT_audit_detail2, HT_audit_detail hT_audit_detail3, HT_audit_detail hT_audit_detail4, HT_audit_detail hT_audit_detail5,
             HT_audit_detail hT_audit_detail6, HT_audit_detail hT_audit_detail7,
            string upload, string business_type_name, string ht_type, string dept_id_apply, string contract_type, string payment_type,string invoice_type,string tax_rate1,string tax_rate2,string payment_type_content,string invoice_content,string Party_B_apply_employee_no,string Party_A_apply_employee_no)
        {
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            string userno = Session["userno"].ToString();

            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
            }
            
            var contracta = from u in db.rs_users where u.user_name ==hT_Main.Party_A_apply_name  select u.user_code;
            string useracode=contracta.FirstOrDefault();
            if (useracode != null)
            {
                Party_A_apply_employee_no2 = int.Parse(useracode);
            }
            
            var contracto = from u in db.rs_users where u.user_name == hT_Main.Party_B_apply_name select u.user_code;
            string userbcode = contracto.FirstOrDefault();
            if (userbcode != null)
            {
                Party_B_apply_employee_no2 = int.Parse(userbcode);
            }
            


            hT_Main.employee_no_opr = Convert.ToInt32(userno);


            hT_Main.is_standard = "1";//暂定

            if (hT_Main.is_standard == "1")
            {
                hT_Main.date_time_create= DateTime.Now;

                var contracts = from c in db.HT_Business_types where c.business_type_name == business_type_name select c.business_type;
                contracts.ToArray();
                foreach (Int16 item in contracts)
                {
                    hT_Main.ht_business_type = item;
                }

                var contractsa = from a in db.HT_types where a.ht_type_name == ht_type select a.ht_type;
                contractsa.ToArray();
                foreach (Int16 item in contractsa)
                {
                    hT_Main.ht_type = item;
                }

                if (contract_type == "other")
                {
                    hT_Main.invoice_type = invoice_type;
                }
                else
                {
                        if (contract_type == "增值税专用发票")
                        {
                            hT_Main.contract_type = 2;
                        }
                        else
                        {
                            if (contract_type == "增值税普通发票")
                            {
                                hT_Main.contract_type = 3;
                            }
                        }
                }

                if (payment_type == "")
                {
                    hT_Main.payment_type_content = invoice_content;
                }
                else
                {
                    var contractsd = from d in db.HT_payment_types where d.payment_type_name == payment_type select d.payment_type;
                    contractsd.ToArray();
                    foreach (Int16 item in contractsd)
                    {
                        hT_Main.payment_type = item;
                    }
                }

                if (tax_rate1 == "其他")
                {
                    hT_Main.tax_rate = double.Parse(tax_rate2) / 100;
                }
                else
                {
                    string[] sArray = tax_rate1.Split('%');
                    foreach (string i in sArray)
                    {
                        hT_Main.tax_rate = double.Parse(i) / 100;
                        break;
                    }
                }


                hT_Main.invoice_content = invoice_content;

                var contractsb = from b in db.rs_groups where b.group_name == dept_id_apply select b.group_id;
                contractsb.ToArray();
                foreach (Int16 item in contractsb)
                {
                    hT_Main.dept_id_apply = item;
                }

               

                hT_Main.status_flag = "N";
                db.HT_Mains.Add(hT_Main);
                db.SaveChanges();



                //create details table data
                apply applyshow = new apply();
                List<HT_audit_detail> Audit_details = new List<HT_audit_detail>();
                Audit_details = applyshow.detailstore(hT_Main, hT_audit_detail, hT_audit_detail1, hT_audit_detail2, hT_audit_detail3, hT_audit_detail4, hT_audit_detail5,
             hT_audit_detail6, hT_audit_detail7,
            upload, business_type_name, ht_type, dept_id_apply, contract_type, payment_type, Party_A_apply_employee_no2, Party_B_apply_employee_no2);
                foreach (HT_audit_detail ITEM in Audit_details)
                {
                    db.HT_audit_details.Add(ITEM);
                    db.SaveChanges();
                }
                //将审核流存入HT_Main
                var htmain = db.HT_Mains.Where(D => D.ht_id == hT_Main.ht_id);
                List<HT_Main> MyMain = htmain.ToList();
                var detail = db.HT_audit_details.Where(x => x.ht_id == hT_Main.ht_id);
                List<HT_audit_detail> Mydetails = detail.ToList();
                foreach (HT_audit_detail tmp3 in Mydetails)
                {
                    if (tmp3.status_flag == "N")
                    {
                        if (tmp3.employee_no_audit == null)
                        {
                            nextuser = 00000;
                            break;
                        }
                        else
                        {
                            nextuser = tmp3.employee_no_audit;
                            break;
                        }
                    }

                }

                foreach (HT_Main tmp2 in MyMain)
                {
                    tmp2.employee_no_next_audit = nextuser;
                    db.Entry(tmp2).State = EntityState.Modified;
                    db.SaveChanges();
                }



                HttpFileCollection Files = System.Web.HttpContext.Current.Request.Files;
                for (int i = 0; i < Files.Count; i++)
                {
                    HttpPostedFile FileImg = Files[i];
                    string strAddress = System.Configuration.ConfigurationManager.AppSettings["FileAddress"].ToString();
                    ChannelFactory<ServiceReference.IUploadService> factory = new ChannelFactory<ServiceReference.IUploadService>("BasicHttpBinding_IUploadService", new EndpointAddress(strAddress));

                    ServiceReference.IUploadService service = factory.CreateChannel();
                    ServiceReference.FileUploadMessage msg = new ServiceReference.FileUploadMessage();
                    ServiceReference.Result result = new ServiceReference.Result();


                    Stream fs = FileImg.InputStream;//stream;//FileImg.InputStream;
                    fs.Position = 0;
                    msg.FileId = "";
                    msg.CategoryId = 1001;//
                    msg.KeyNo = "12345";//
                    msg.FileName = FileImg.FileName;
                    msg.FileData = fs;
                    msg.OprNo = "12040";//
                    if (fs.Length > 0)
                    {
                        result = service.FileUpload(msg);//调用上传方法
                        if (result.Success)
                        {
                            HT_main_files hT_main_files = new HT_main_files();
                            hT_main_files.file_id = result.FileId;
                            hT_main_files.ht_id = hT_Main.ht_id;
                            hT_main_files.file_index = i;//改啊啊啊啊啊 
                            hT_main_files.user_filename = result.FileName;
                            hT_main_files.system_filename = hT_Main.ht_no;
                            db.HT_main_filess.Add(hT_main_files);
                            db.SaveChanges();
                            continue;
                        }
                        else
                        {
                            return Content(string.Format("<script type='text/javascript'>alert('失败原因: ' + result.ErrorMessage);window.location.href='{0}'</script>", Url.Action("Index", "ContractApply")));
                            //return Content("失败原因:" + result.ErrorMessage);

                        }
                    }
                    return Content("无上传文件");
                }
                return Content(string.Format("<script type='text/javascript'>alert('合同编号：" + hT_Main.ht_no + "已经提交成功');window.location.href='{0}'</script>", Url.Action("Index", "ContractApply")));
            }
            return RedirectToAction("Index");
        }
        

        [HttpPost]
        public ActionResult Index2(string userno)
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
                return Content(string.Format("<script type='text/javascript'>alert('此工号无进入权限，请重新登录！');window.location.href='{0}'</script>", Url.Action("Index", "Home")));
            }
        }

        public JsonResult GetBusinesstype(string depart_name)
        {
            List<string> businesslist = new List<string>(); 
            var contractsb = from b in db.rs_groups where b.group_name == depart_name select b.group_id;
            int groupdeptid=contractsb.FirstOrDefault();
            var business = from s in db.HT_Business_Type_Depts where s.group_dept_id == groupdeptid select s.business_type;
            List<short>businessid=business.ToList();
            foreach (short item in businessid)
            {
                string name;
                var businesstype = from c in db.HT_Business_types where c.business_type == item select c.business_type_name;
                name=businesstype.FirstOrDefault();
                businesslist.Add(name);
            }
            return Json(businesslist);
        }

    }
}
