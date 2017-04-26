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
using Contract.ServiceReference;
using System.Configuration;
using System.Net.Http;
using Contract.core;
using System.Data.Entity;

namespace Contract.Controllers
{
    public class ContractAuditController : Controller
    {

        private ContractContext db = new ContractContext();

        Audit myaudit = new Audit();
        string tmpUrl;
        string tmpName;
        int? nextuser = null;
        int? order;
        int? addorder;

        DateTime? contract_begin_datetmp;
        DateTime? contract_end_datetmp;
        string ht_notmp;
        string user_nametmp;
        string contract_price_total_smallertmp;
        string contract_price_total_biggertmp;
        string Party_A_nametmp;
        string Party_B_nametmp;
        string Party_A_apply_nametmp;
        string Party_B_apply_nametmp;

        bool key = false;

        public ActionResult Index()
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            List<Myfile> myfiles = new List<Myfile>();
            string userno = Session["userno"].ToString();

            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int? no = int.Parse(userno);
            int usernum = int.Parse(userno);
            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
                ViewData["userno"] = no;
            }
            var auditgroup = from c in db.rs_groups
                             where c.group_name == "非诉讼组"
                             select c.group_id;
            int group = auditgroup.FirstOrDefault();
            ViewData["mygroup"]=getgroup_id(usernum);
            ViewData["group"] = group;

            AuditMethod auditmethod = new AuditMethod();

            vm = auditmethod.Auditshow(usernum);

            foreach (htmain item in vm.htmainModel)
            {
                myfiles.AddRange(showfile(item.ht_id));
            }
            vm.filesModel = myfiles;
            return View("ContractAudit", vm);
        }

        [HttpPost]
        public ActionResult Index(string ht_no, string user_name, string contract_price_total_smaller, string contract_price_total_bigger, string Party_A_name, string Party_B_name, DateTime? contract_begin_date, DateTime? contract_end_date, string Party_A_apply_name, string Party_B_apply_name)
        {
            string userno = Session["userno"].ToString();

            Session["ht_no"] = ht_no;
            Session["user_name"] = user_name;
            Session["contract_price_total_smaller"] = contract_price_total_smaller;
            Session["contract_price_total_bigger"] = contract_price_total_bigger;
            Session["Party_A_name"] = Party_A_name;
            Session["Party_B_name"] = Party_B_name;
            Session["contract_begin_date"] = contract_begin_date;
            Session["contract_end_date"] = contract_end_date;
            Session["Party_A_apply_name"] = Party_A_apply_name;
            Session["Party_B_apply_name"] = Party_B_apply_name;


            viewModel vm = new viewModel(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int? no = int.Parse(userno);
            var contracto = from o in db.rs_users where o.user_code == userno select o.user_name;
            contracto.ToArray();
            int usernum = int.Parse(userno);
            foreach (string item in contracto)
            {
                ViewData["user"] = item;
                ViewData["userno"] = no;
            }
            var auditgroup = from c in db.rs_groups
                             where c.group_name == "非诉讼组"
                             select c.group_id;
            int group = auditgroup.FirstOrDefault();
            ViewData["mygroup"] = getgroup_id(usernum);
            ViewData["group"] = group;

            AuditMethod auditmethod = new AuditMethod();
            vm = auditmethod.AuditQuery(ht_no, user_name, contract_price_total_smaller, contract_price_total_bigger, Party_A_name, Party_B_name, contract_begin_date, contract_end_date, Party_A_apply_name, Party_B_apply_name, usernum);


            foreach (htmain item in vm.htmainModel)
            {
                vm.filesModel = showfile(item.ht_id);
            }
            return View("ContractAudit", vm);
        }

        public List<Myfile> showfile(int ht_id)
        {
            List<Myfile> myfiles = new List<Myfile>();
            List<string> fileid = new List<string>();
            var contractw = from w in db.HT_main_filess where w.ht_id == ht_id select w.file_id;
            contractw.ToList();
            foreach (string item in contractw)
            {
                fileid.Add(item);
            }
            try
            {
                foreach (string id in fileid)
                {
                    FileSearchMessage mg = new FileSearchMessage();
                    FileInfoResult result = new FileInfoResult();
                    UploadServiceClient service = new UploadServiceClient();
                    mg.FileId = id;
                    result = service.FileSearch(mg);
                    FileInfoData[] data;
                    if (result.Success)
                    {
                        data = result.FileInfoList;
                        foreach (FileInfoData item in data)
                        {
                            Myfile myfile = new Myfile();
                            myfile.ht_id = ht_id;
                            myfile.filesurl = data[0].FileUrl;
                            myfile.filesname = data[0].FileName;
                            myfiles.Add(myfile);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return (myfiles);
            }
            catch
            {
                return null;
            }
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

        public void Myaudit(int auditid, string audit, string auditcomment, string userno)
        {
            if (string.IsNullOrEmpty(userno))
            {
                userno = Session["userno"].ToString();
            }
            //
            if (userno == "0")
            {
                
            }
            //
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int usernum = int.Parse(userno);
            var htmain = db.HT_Mains.Where(D => D.ht_id == auditid);
            List<HT_Main> MyMain = htmain.ToList();
            var detail = db.HT_audit_details.Where(x => x.ht_id == auditid).OrderBy(d => d.priority_order);
            List<HT_audit_detail> Mydetails = detail.ToList();
            AuditMethod auditmethod = new AuditMethod();
            int? groupid = auditmethod.getgroup_id(usernum);
            var auditgroup = from c in db.rs_groups
                             where c.group_name == "非诉讼组"
                             select c.group_id;
            int auditgroupid = auditgroup.FirstOrDefault();

            var currentorder = from i in detail where i.employee_no_audit == usernum select i.priority_order;
            order = currentorder.FirstOrDefault();

            var orderlist = detail.Select(x => x.priority_order).OrderBy(x => x.Value);
            List<int?> orderlists = orderlist.ToList();
            int? biggest = orderlists.Max();

            if (groupid == auditgroupid)
            {
                var de = from s in detail where s.employee_no_audit == usernum select s.status_flag;
                string status = de.FirstOrDefault();
                if (string.IsNullOrEmpty(status))
                {
                    var currentorder2 = from i in detail where i.employee_no_audit == null select i.priority_order;
                    order = currentorder2.FirstOrDefault();
                }
                else
                {
                    currentorder = from i in detail where i.employee_no_audit == usernum select i.priority_order;
                    order = currentorder.FirstOrDefault();
                }

                var contractgroup = from s in detail where s.group_id == auditgroupid select s;
                List<HT_audit_detail> contractdetails = contractgroup.ToList();
                foreach (HT_audit_detail auditdetail in contractdetails)
                {
                    auditdetail.employee_no_audit = usernum;
                    auditdetail.audit_comments = auditcomment;
                    auditdetail.audit_date_time = DateTime.Now;
                    if (audit == "拒绝")
                    {
                        auditdetail.status_flag = "F";
                        foreach (HT_Main tmp2 in MyMain)
                        {
                            if (audit == "拒绝")
                            {
                                tmp2.status_flag = "F";
                            }
                            db.Entry(tmp2).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    if (audit == "通过")
                    {
                        auditdetail.status_flag = "T";
                    }
                    db.Entry(auditdetail).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                foreach (HT_audit_detail tmp in Mydetails)
                {
                    if ((tmp.employee_no_audit == usernum))
                    {
                        tmp.audit_comments = auditcomment;
                        if (audit == "拒绝")
                        {
                            tmp.status_flag = "F";
                            foreach (HT_Main tmp2 in MyMain)
                            {
                                if (audit == "拒绝")
                                {
                                    tmp2.status_flag = "F";
                                }
                                db.Entry(tmp2).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        if (audit == "通过")
                        {
                            tmp.status_flag = "T";
                        }
                        tmp.audit_date_time = DateTime.Now;
                        db.Entry(tmp).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            foreach (HT_audit_detail tmp3 in Mydetails)
            {
                if ((tmp3.status_flag == "N") && (tmp3.priority_order == (order + 1)))
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
                tmp2.employee_no_last_audit = usernum;
                tmp2.employee_no_next_audit = nextuser;
                db.Entry(tmp2).State = EntityState.Modified;
                db.SaveChanges();
            }


            foreach (HT_audit_detail item in Mydetails)
            {
                if ((item.status_flag != "N") && (item.status_flag != "C") && (order == biggest))
                {
                    foreach (HT_Main tmp2 in MyMain)
                    {
                        if (audit == "拒绝")
                        {
                            tmp2.status_flag = "F";
                        }
                        if (audit == "通过")
                        {
                            tmp2.status_flag = "T";
                        }
                        db.Entry(tmp2).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }
        public ActionResult emailpass(int auditid, string audit, string auditcomment, string userno)
        {
            Myaudit(auditid,audit,auditcomment,userno);
            return Content("审核通过！");
        }
        public ActionResult emailcancel(int auditid, string audit, string auditcomment, string userno)
        {
            Myaudit2(auditid, audit, auditcomment, userno);
            return Content("审核拒绝！");
        }
        public void Myaudit2(int auditid2, string audit, string auditcomment, string userno)
        {
            if (string.IsNullOrEmpty(userno))
            {
                userno = Session["userno"].ToString();
            }
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int usernum = int.Parse(userno);
            var htmain = db.HT_Mains.Where(D => D.ht_id == auditid2);
            List<HT_Main> MyMain = htmain.ToList();
            var detail = db.HT_audit_details.Where(x => x.ht_id == auditid2).OrderBy(d => d.priority_order);
            List<HT_audit_detail> Mydetails = detail.ToList();
            AuditMethod auditmethod = new AuditMethod();
            int? groupid = auditmethod.getgroup_id(usernum);
            var auditgroup = from c in db.rs_groups
                             where c.group_name == "非诉讼组"
                             select c.group_id;
            int auditgroupid = auditgroup.FirstOrDefault();

            var currentorder = from i in detail where i.employee_no_audit == usernum select i.priority_order;
            order = currentorder.FirstOrDefault();

            var orderlist = detail.Select(x => x.priority_order).OrderBy(x => x.Value);
            List<int?> orderlists = orderlist.ToList();
            int? biggest = orderlists.Max();

            if (groupid == auditgroupid)
            {
                var de = from s in detail where s.employee_no_audit == usernum select s.status_flag;
                string status = de.FirstOrDefault();
                if (string.IsNullOrEmpty(status))
                {
                    var currentorder2 = from i in detail where i.employee_no_audit == null select i.priority_order;
                    order = currentorder2.FirstOrDefault();
                }
                else
                {
                    currentorder = from i in detail where i.employee_no_audit == usernum select i.priority_order;
                    order = currentorder.FirstOrDefault();
                }

                var contractgroup = from s in detail where s.group_id == auditgroupid select s;
                List<HT_audit_detail> contractdetails = contractgroup.ToList();
                foreach (HT_audit_detail auditdetail in contractdetails)
                {
                    auditdetail.employee_no_audit = usernum;
                    auditdetail.audit_comments = auditcomment;
                    auditdetail.audit_date_time = DateTime.Now;
                    if (audit == "拒绝")
                    {
                        auditdetail.status_flag = "F";
                        foreach (HT_Main tmp2 in MyMain)
                        {
                            if (audit == "拒绝")
                            {
                                tmp2.status_flag = "F";
                            }
                            db.Entry(tmp2).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    if (audit == "通过")
                    {
                        auditdetail.status_flag = "T";
                    }
                    db.Entry(auditdetail).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                foreach (HT_audit_detail tmp in Mydetails)
                {
                    if ((tmp.employee_no_audit == usernum))
                    {
                        tmp.audit_comments = auditcomment;
                        if (audit == "拒绝")
                        {
                            tmp.status_flag = "F";
                            foreach (HT_Main tmp2 in MyMain)
                            {
                                if (audit == "拒绝")
                                {
                                    tmp2.status_flag = "F";
                                }
                                db.Entry(tmp2).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        if (audit == "通过")
                        {
                            tmp.status_flag = "T";
                        }
                        tmp.audit_date_time = DateTime.Now;
                        db.Entry(tmp).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            foreach (HT_audit_detail tmp3 in Mydetails)
            {
                if ((tmp3.status_flag == "N") && (tmp3.priority_order == (order + 1)))
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
                tmp2.employee_no_last_audit = usernum;
                tmp2.employee_no_next_audit = nextuser;
                db.Entry(tmp2).State = EntityState.Modified;
                db.SaveChanges();
            }


            foreach (HT_audit_detail item in Mydetails)
            {
                if ((item.status_flag != "N") && (item.status_flag != "C") && (order == biggest))
                {
                    foreach (HT_Main tmp2 in MyMain)
                    {
                        if (audit == "拒绝")
                        {
                            tmp2.status_flag = "F";
                        }
                        if (audit == "通过")
                        {
                            tmp2.status_flag = "T";
                        }
                        db.Entry(tmp2).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
        }

        public ActionResult reurl()
        {
            try
            {
                ht_notmp = Session["ht_no"].ToString();
            }
            catch
            {
                ht_notmp = null;
            }
            try
            {
                user_nametmp = Session["user_name"].ToString();
            }
            catch
            {
                user_nametmp = null;
            }
            try
            {
                contract_price_total_smallertmp = Session["contract_price_total_smaller"].ToString();
            }
            catch
            {
                contract_price_total_smallertmp = null;
            }
            try
            {
                contract_price_total_biggertmp = Session["contract_price_total_bigger"].ToString();
            }
            catch
            {
                contract_price_total_biggertmp = null;
            }
            try
            {
                Party_A_nametmp = Session["Party_A_name"].ToString();
            }
            catch
            {
                Party_A_nametmp = null;
            }
            try
            {
                Party_B_nametmp = Session["Party_B_name"].ToString();
            }
            catch
            {
                Party_B_nametmp = null;
            }
            try
            {
                contract_begin_datetmp = Convert.ToDateTime(Session["contract_begin_date"].ToString());
            }
            catch
            {
                contract_begin_datetmp = null;
            }
            try
            {
                contract_end_datetmp = Convert.ToDateTime(Session["contract_end_date"].ToString());
            }
            catch
            {
                contract_end_datetmp = null;
            }
            try
            {
                Party_A_apply_nametmp = Session["Party_A_apply_name"].ToString();
            }
            catch
            {
                Party_A_apply_nametmp = null;
            }
            try
            {
                Party_B_apply_nametmp = Session["Party_B_apply_name"].ToString();
            }
            catch
            {
                Party_B_apply_nametmp = null;
            }


            if (string.IsNullOrEmpty(ht_notmp) && string.IsNullOrEmpty(user_nametmp) && string.IsNullOrEmpty(contract_price_total_smallertmp) && string.IsNullOrEmpty(contract_price_total_biggertmp) && string.IsNullOrEmpty(Party_A_nametmp) && string.IsNullOrEmpty(Party_B_nametmp) && string.IsNullOrEmpty(Convert.ToString(contract_begin_datetmp)) && string.IsNullOrEmpty(Convert.ToString(contract_end_datetmp)) && string.IsNullOrEmpty(Party_A_apply_nametmp) && string.IsNullOrEmpty(Party_B_apply_nametmp))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index2");
            }
        }


        public ActionResult Index2()
        {
            try
            {
                ht_notmp = Session["ht_no"].ToString();
            }
            catch
            {
                ht_notmp = null;
            }
            try
            {
                user_nametmp = Session["user_name"].ToString();
            }
            catch
            {
                user_nametmp = null;
            }
            try
            {
                contract_price_total_smallertmp = Session["contract_price_total_smaller"].ToString();
            }
            catch
            {
                contract_price_total_smallertmp = null;
            }
            try
            {
                contract_price_total_biggertmp = Session["contract_price_total_bigger"].ToString();
            }
            catch
            {
                contract_price_total_biggertmp = null;
            }
            try
            {
                Party_A_nametmp = Session["Party_A_name"].ToString();
            }
            catch
            {
                Party_A_nametmp = null;
            }
            try
            {
                Party_B_nametmp = Session["Party_B_name"].ToString();
            }
            catch
            {
                Party_B_nametmp = null;
            }
            try
            {
                contract_begin_datetmp = Convert.ToDateTime(Session["contract_begin_date"].ToString());
            }
            catch
            {
                contract_begin_datetmp = null;
            }
            try
            {
                contract_end_datetmp = Convert.ToDateTime(Session["contract_end_date"].ToString());
            }
            catch
            {
                contract_end_datetmp = null;
            }
            try
            {
                Party_A_apply_nametmp = Session["Party_A_apply_name"].ToString();
            }
            catch
            {
                Party_A_apply_nametmp = null;
            }
            try
            {
                Party_B_apply_nametmp = Session["Party_B_apply_name"].ToString();
            }
            catch
            {
                Party_B_apply_nametmp = null;
            }

            viewModel vm = new viewModel(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
            string userno = Session["userno"].ToString();
            //TempData.Keep("userno");
            int? no = int.Parse(userno);
            var contracto = from o in db.rs_users where o.user_code == userno select o.user_name;
            contracto.ToArray();
            int usernum = int.Parse(userno);
            foreach (string item in contracto)
            {
                ViewData["user"] = item;
                ViewData["userno"] = no;
            }
            var auditgroup = from c in db.rs_groups
                             where c.group_name == "非诉讼组"
                             select c.group_id;
            int group = auditgroup.FirstOrDefault();
            ViewData["mygroup"] = getgroup_id(usernum);
            ViewData["group"] = group;
            AuditMethod auditmethod = new AuditMethod();
            vm = auditmethod.AuditQuery(ht_notmp, user_nametmp, contract_price_total_smallertmp, contract_price_total_biggertmp, Party_A_nametmp, Party_B_nametmp, contract_begin_datetmp, contract_end_datetmp, Party_A_apply_nametmp, Party_B_apply_nametmp, usernum);


            foreach (htmain item in vm.htmainModel)
            {
                vm.filesModel = showfile(item.ht_id);
            }
            return View("ContractAudit", vm);
        }
        public string Add(int addid, string addno, string addname)
        {
            string userno = Session["userno"].ToString();
            int usernum = int.Parse(userno);
            AuditMethod auditmethod = new AuditMethod();
            //List<HT_audit_detail> container = new List<HT_audit_detail>();
            //有权限？？
            var contracty = from x in db.rs_users where x.user_code == addno select x.user_name;
            string addusername = contracty.FirstOrDefault();
            if (addusername != addname)
            {
                return "C";//输入的工号与姓名不匹配
            }
            var contracti = from i in db.rs_users where i.user_code == addno && (i.user_status == "S" || i.user_status == "Y" || i.user_status == "P") select i.user_id;
            int? adduserid = contracti.FirstOrDefault();
            if (adduserid == null)
            {
                return "N";//追加人无审核权限
            }
            else
            {
                //var myaddmain = db.HT_Mains.Where(D => D.ht_id == addid);
                //List<HT_Main> addmain = myaddmain.ToList();
                //foreach (HT_Main main in addmain)
                //{
                //    main.employee_no_next_audit = int.Parse(addno);
                //    db.Entry(main).State = EntityState.Modified;
                //    db.SaveChanges();
                //}
                var detail = db.HT_audit_details.Where(x => x.ht_id == addid).OrderBy(d => d.priority_order);
                List<HT_audit_detail> Mydetails = detail.ToList();
                var htmain = db.HT_Mains.Where(D => D.ht_id == addid);
                List<HT_Main> MyMain = htmain.ToList();
                var currentorder = from i in detail where i.employee_no_audit == usernum select i.priority_order;
                order = currentorder.FirstOrDefault();
                if (order == null)
                {
                    var grouporder = from i in detail where i.employee_no_audit == null select i.priority_order;
                    order = grouporder.FirstOrDefault();
                }
                
                foreach (HT_audit_detail handle in Mydetails)
                {
                    if (handle.priority_order <= order)
                    {
                        continue;
                        //container.Add(handle);
                    }
                    else
                    {
                        handle.priority_order = handle.priority_order + 1;
                        //container.Add(handle);
                        db.Entry(handle).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                
                int? groupid = auditmethod.getgroup_id(int.Parse(addno));
                HT_audit_detail myadddetail = new HT_audit_detail();
                myadddetail.ht_id = addid;
                myadddetail.employee_no_audit = int.Parse(addno);
                myadddetail.group_id = groupid;
                myadddetail.status_flag = "N";
                myadddetail.priority_order = order + 1;
                myadddetail.additional_audit_flag = "Z";
                db.HT_audit_details.Add(myadddetail);
                db.SaveChanges();

                //foreach(HT_Main myitemadd in MyMain)
                //{
                //    if(myitemadd.ht_id== addid)
                //    {
                //        myitemadd.employee_no_next_audit = int.Parse(addno);
                //        myitemadd.employee_no_last_audit = usernum;
                //        db.Entry(myitemadd).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //}
                return "Y";//完成
            }
        }



    }
}
