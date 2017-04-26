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
using System.Net.Mail;

namespace Contract.Controllers
{
    public class ContractQueryController : Controller
    {
        private ContractContext db = new ContractContext();
        string UserFileName;
        string systemfilename;
        string tmpUrl;
        string tmpName;

        public ActionResult Index()
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);
            List<Myfile> myfiles = new List<Myfile>();
            string userno = Session["userno"].ToString();
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int? no = int.Parse(userno);
            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
                ViewData["userno"] = no;
            }

            QueryMethod querymethod = new QueryMethod();
            vm = querymethod.QueryShow(userno);
            foreach (htmain item in vm.htmainModel)
            {

                 myfiles.AddRange(showfile(item.ht_id));
            }
            vm.filesModel = myfiles;
            return View("ContractQuery", vm);
        }

        [HttpPost]
        //查询 
        public ActionResult Index(string ht_no, string contract_price_total_smaller, string contract_price_total_bigger, string Party_A_name, string Party_B_name, DateTime? contract_begin_date, DateTime? contract_end_date)
        {
            string userno = Session["userno"].ToString();
            //string userno = TempData["userno"] as string;
            //TempData.Keep("userno");
            int? no = int.Parse(userno);
            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
                ViewData["userno"] = no;
            }
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);

            QueryMethod querymethod = new QueryMethod();
            vm = querymethod.queryquery(userno, ht_no, contract_price_total_smaller, contract_price_total_bigger, Party_A_name, Party_B_name, contract_begin_date, contract_end_date);

            foreach (htmain item in vm.htmainModel)
            {
                vm.filesModel = showfile(item.ht_id);
            }
            return View("ContractQuery", vm);
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

        public ActionResult Cancel(int queryhtid)
        {
            HT_Main tmp=db.HT_Mains.Find(queryhtid);
            tmp.status_flag = "C";
            db.Entry(tmp).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int queryhtid)
        {
            string userno = Session["userno"].ToString();
            //TempData.Keep("userno");
            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
            }

            HT_Main tmp = db.HT_Mains.Find(queryhtid);
            var vm = new viewModel(db.rs_groups.ToList(), null, db.HT_payment_types.ToList(),db.HT_Mains.ToList(), null, null, null,null, db.HT_main_filess.ToList(),null, db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null,null);
            //取出group_type为X、Z的往 视图的LIST传
            var contract1 = from a in db.rs_groups
                            select a;

            contract1 = contract1.Where(x => x.group_type == "X" || x.group_type == "Z");
            vm.rs_groupModel = contract1.ToList();

            //取出active_flag 为Y的往 视图的LIST传
            var contracts = from c in db.HT_types
                            select c;

            contracts = contracts.Where(x => x.active_flag == "Y");

            vm.HT_typeModel = contracts.ToList();

            //取出active_flag 为Y的往 视图的LIST传
            var contract = from b in db.HT_Business_types
                           select b;

            contract = contract.Where(x => x.active_flag == "Y");


            vm.HT_Business_typeModel = contract.ToList();
            vm.HT_MainModel.Clear();
            vm.HT_MainModel.Add(tmp);
            return View(vm);
        }

        
        public void email(int nextemployee,int htid)
        {
            string employeenum="";
            List<string> employees = new List<string>();
            if (nextemployee != 0)
            {
                employeenum = nextemployee.ToString();
                var emailto = from s in db.rs_users where employeenum == s.user_code select s.email;
                string to = emailto.FirstOrDefault();

                var auditperson = from i in db.rs_users where i.user_code == employeenum select i.user_name;
                string auditname = auditperson.FirstOrDefault();

                string auditpass = System.Configuration.ConfigurationManager.AppSettings["auditurlpass"].ToString();
                string auditurlcancel = System.Configuration.ConfigurationManager.AppSettings["auditurlcancel"].ToString();


                string from = System.Configuration.ConfigurationManager.AppSettings["emailfrom"].ToString();
                string key = System.Configuration.ConfigurationManager.AppSettings["emailkey"].ToString();

                var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
                string userno = Session["userno"].ToString();
                htmain emailmain = new htmain();
                QueryMethod querymethod = new QueryMethod();
                vm = querymethod.QueryShow(userno);
                foreach (htmain item in vm.htmainModel)
                {
                    if (htid == item.ht_id)
                    {
                        emailmain = item;
                    }
                }
                List<Myfile> myfiles = new List<Myfile>();
                myfiles.AddRange(showfile(emailmain.ht_id));
                string title = "合同跟催";
                string content = "<!DOCTYPE html><html><head><meta name='viewport' content='width=device-width' /><title>Index</title></head><body><div><div><h4></h4></div><div style='margin-left: 5px;margin-right: 0px;'><table border='1' id='to' cellspacing='0' cellpadding='8' align='center'><tr align='center'><td></td><td colspan='5' width='530' style='font-size:larger;font-weight:bold;text-align:center'>合同摘要</td></tr><tr><td width ='100'> 合同编号 </td><td colspan = '3' width = '225' id = 'ht_no'><input type = 'text' readonly class='ht_no' value=" + emailmain.ht_no + " style='width: 100%;border:none;outline: none;margin: 0px auto;text-align:center;' /></td><td width = '100' > 份数 </td><td width='205' id='no_of_contracts'><input type = 'text' readonly value=" + emailmain.no_of_contracts + " class='no_of_contracts' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td width = '100' > 合同名称 </td ><td colspan='5' width='530' id='ht_name'><input type = 'text' readonly value=" + emailmain.ht_name + " class='ht_name' style='width: 100%;border:none;height:100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td width = '100' > 承办部门 </td ><td colspan='3'><input value = " + emailmain.deptname + " readonly style='width:100%;border:none;outline: none;text-align: center;' type='text' class='dept_id_apply' /></td><td>合同类型</td><td><input type = 'text' readonly value=" + emailmain.httypename + " class='ht_type' style='border:none;outline: none;text-align: center;width:100%' /></td></tr><tr><td width = '100' > 合同期限 </td><td colspan='5' width='530'><input type = 'text' readonly value=" + emailmain.contract_begin_date + " class='contract_begin_date' style='width: 32%;border:none;outline: none;text-align:center;padding-left: 72px;margin-left: 9px;margin-right: -67px;' /><span style = 'margin-left: 143px;' > 至 </span><input type='text' readonly value=" + emailmain.contract_end_date + " class='contract_end_date' style='width: 40%;border:none;outline: none;margin-left: 69px;padding-left: 34px;text-align:center;' /></td></tr><tr><td width = '100' > 业务类型 </td><td colspan='3'><input type = 'text' readonly value=" + emailmain.businessname + " class='ht_business_type' style='border:none;outline: none;width: 100%;text-align: center;' /></td><td>送审日期</td><td id = 'date_time_create'><input type='text' readonly value=" + emailmain.date_time_create + " class='date_time_create' style='width: 100%;border:none;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>甲方</td><td colspan = '5' id='Party_A_name'><input type = 'text' value=" + emailmain.Party_A_name + " readonly class='Party_A_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center'/></td></tr><tr><td>甲方经办人</td><td width = '88' id='Party_A_apply_name'><input type = 'text' readonly value=" + emailmain.Party_A_apply_name + " class='Party_A_apply_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td width = '40' > 工号 </td ><td width='88' id='Party_A_apply_employee_no'><input type = 'text' readonly value=" + emailmain.Party_A_apply_employee_no + " class='Party_A_apply_employee_no' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td><td>联系方式</td><td id = 'Party_A_Contract' ><input type='text' readonly value=" + emailmain.Party_A_contract + " class='Party_A_contract' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>乙方</td><td colspan = '5' id='Party_B_name'><input type = 'text' readonly value=" + emailmain.Party_B_name + " class='Party_B_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td></tr><tr><td>乙方经办人</td><td id = 'Party_B_apply_name' ><input type='text' readonly value=" + emailmain.Party_B_apply_name + " class='Party_B_apply_name' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td><td>工号</td><td id = 'Party_B_apply_employee_no' ><input type='text' readonly value=" + emailmain.Party_B_apply_employee_no + " class='Party_B_apply_employee_no' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td>联系方式</td><td id = 'Party_B_contract'><input type='text' readonly value=" + emailmain.Party_B_contract + " class='Party_B_contract' style='width: 100%;border:none;height: 100%; outline: none; margin: 0px auto;text-align:center' /></td></tr><tr><td>标的单价</td><td colspan = '3' id='target_unit_price'><input type = 'text' readonly value=" + emailmain.target_unit_price + " class='target_unit_price' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td>合同总价</td><td id = 'contract_price_total' ><input type='text' readonly value=" + emailmain.contract_price_total + " class='contract_price_total' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>主要内容</td><td colspan = '5' style='word-wrap:break-word;' width='530' id='ht_comments'><input class='ht_comments' readonly value=" + emailmain.ht_comments + " style='border:none;outline: none;text-align:center' /></td></tr>";

                content = content + "</table><table border = '1' cellspacing='0' cellpadding='8' style='margin-left: 15.5%; margin-top: 4%; margin-right: 4%; ' align='center'><tbody align = 'center' ><tr><td width='100'>发票</td><td width = '548' colspan='3'>";
                if (emailmain.invoice_type == null)
                {
                    if (emailmain.contract_type == 2)
                    {
                        content = content + "<input type = 'text' name = 'contract_type' readonly style= 'text-align:center; outline: none; border: none; ' value = '增值税专用发票' />";
                    }
                    if (emailmain.contract_type == 3)
                    {
                        content = content + "<input type = 'text' name = 'contract_type' readonly style= 'text-align:center; outline: none; border: none; ' value = '增值税普通发票' />";
                    }
                }
                else
                {
                    content = content + "<input type = 'text' name='contract_type' value=" + emailmain.invoice_type + " readonly style='text-align:center; outline: none; border: none; '/>";

                }
                content = content + "</td></tr><tr><td width = '100' > 税率 </td ><td width='548' colspan='3'><input readonly type='text' name='tax_rate' id='tax_rate' value=" + emailmain.tax_rate + " style='text-align:center; outline: none; border: none; '></td></tr><tr><td width = '100' > 账期 </td ><td width = '274' style='border-right:none;outline:none'><input readonly type='text' name='payment_type' id='payment_type' value=" + emailmain.paymentname + " style='text-align:center; outline: none; border: none; '></td><td colspan='2' width = '274' style='border-left:none'><input readonly type='text' name='payment_type_content' id='payment_type_content' value=";
                if (string.IsNullOrEmpty(emailmain.payment_type_content))
                {
                    content = content + "' ' style='outline: none; border: none; text-align:center' /></td></tr><tr><td width = '100' > 开票内容</td><td colspan = '3' style='word-wrap:break-word; ' width='546'><input type = 'text' value=" + emailmain.invoice_content + " readonly style='outline: none; border: none; ' /></td></tr><tr><td> 合同附件 </td><td colspan = '3' style ='word-wrap:break-word;'>";
                }
                else
                {
                    content = content + emailmain.payment_type_content + " style='outline: none; border: none; text-align:center' /></td></tr><tr><td width = '100' > 开票内容</td><td colspan = '3' style='word-wrap:break-word; ' width='546'><input type = 'text' value=" + emailmain.invoice_content + " readonly style='outline: none; border: none; ' /></td></tr><tr><td> 合同附件 </td><td colspan = '3' style ='word-wrap:break-word;'>";
                }
                foreach (Myfile file in myfiles)
                {
                    if (file.ht_id == emailmain.ht_id)
                    {
                        content = content + "<a href ='" + file.filesurl + "' > " + file.filesname + "</a><br/>";
                    }
                }
                content = content + string.Format("</td></tr></tbody></table></div><table style='border = '1' cellspacing='0' cellpadding='8' style='margin-left: 15.5%; margin-top: 4%; margin-right: 4%; '><tr style='margin-left: 70%; margin-top: 4%; '><td><a  href ='" + auditpass + "?auditid={0}&audit={1}&auditcomment={2}&userno={3}' > 通过 </a >", htid, "通过", auditname + "于" + DateTime.Now + "审核通过", employeenum);
                content = content + string.Format("<a href='" + auditurlcancel + "?auditid={0}&audit={1}&auditcomment={2}&userno={3}' > 拒绝 </a>", htid, "拒绝", auditname + "于" + DateTime.Now + "审核拒绝", employeenum) + "</td></tr></table></div></body></html>";

                string username = from.Split('@').First();
                MailMessage newEmail = new MailMessage();
                #region 发送方邮件
                newEmail.From = new MailAddress(from, from);
                #endregion

                #region 发送对象，可群发

                newEmail.To.Add(new MailAddress(to));

                #endregion

                #region Subject
                newEmail.Subject = title;  //标题
                #endregion

                #region Body
                string strBody = content; //html格式，也可以是普通文本格式 
                newEmail.Body = strBody;  //内容
                #endregion

                #region 上传附件
                // Attachment MsgAttach = new Attachment(this.FileUpload1.PostedFile.FileName);//可通过一个FileUpload地址获取附件地址
                //newEmail.Attachments.Add(MsgAttach);
                #endregion

                #region Deployment
                newEmail.IsBodyHtml = true;                //是否支持html
                newEmail.Priority = MailPriority.High;  //优先级
                #endregion

                //发送方服务器信息
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(username, key);
                smtpClient.Host = "mail.1hai.cn"; //主机xiaodongyu@1hai.cn


                //smtpClient.Send(newEmail);   //同步发送,程序将被阻塞

                #region 发送
                smtpClient.Send(newEmail);
                #endregion
                //return Content("成功！！！！");
            }
            else
            {
                //List<string> emails = new List<string>();
                int auditgroupid;
                var auditgroup = from c in db.rs_groups
                                 where c.group_name == "非诉讼组"
                                 select c.group_id;
                auditgroupid = auditgroup.FirstOrDefault();
                var auditmem = from i in db.rs_users where i.group_id == auditgroupid && ((i.user_status == "Y") || (i.user_status == "S") || (i.user_status == "P")) select i.user_code;
                employees = auditmem.ToList();
                foreach (string item in employees)
                {
                    var emailto = from s in db.rs_users where item == s.user_code select s.email;
                    string totmp = emailto.FirstOrDefault();
                    var name = from i in db.rs_users where item == i.user_code select i.user_name;
                    string myname = name.FirstOrDefault();
                    //emails.Add(totmp);

                    //string auditname = "非诉讼组";
                    //var auditperson = from i in db.rs_users where i.user_code == employeenum select i.user_name;
                    //string auditname = auditperson.FirstOrDefault();

                    string auditpass = System.Configuration.ConfigurationManager.AppSettings["auditurlpass"].ToString();
                    string auditurlcancel = System.Configuration.ConfigurationManager.AppSettings["auditurlcancel"].ToString();

                    //string to = System.Configuration.ConfigurationManager.AppSettings["emailto"].ToString();//测试用

                    string from = System.Configuration.ConfigurationManager.AppSettings["emailfrom"].ToString();
                    string key = System.Configuration.ConfigurationManager.AppSettings["emailkey"].ToString();

                    var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
                    string userno = Session["userno"].ToString();
                    htmain emailmain = new htmain();
                    QueryMethod querymethod = new QueryMethod();
                    vm = querymethod.QueryShow(userno);
                    foreach (htmain item2 in vm.htmainModel)
                    {
                        if (htid == item2.ht_id)
                        {
                            emailmain = item2;
                        }
                    }
                    List<Myfile> myfiles = new List<Myfile>();
                    myfiles.AddRange(showfile(emailmain.ht_id));

                    string title = "合同跟催";
                    string content = "<!DOCTYPE html><html><head><meta name='viewport' content='width=device-width' /><title>Index</title></head><body><div><div><h4></h4></div><div style='margin-left: 5px;margin-right: 0px;'><table border='1' id='to' cellspacing='0' cellpadding='8' align='center'><tr align='center'><td></td><td colspan='5' width='530' style='font-size:larger;font-weight:bold;text-align:center'>合同摘要</td></tr><tr><td width ='100'> 合同编号 </td><td colspan = '3' width = '225' id = 'ht_no'><input type = 'text' readonly class='ht_no' value=" + emailmain.ht_no + " style='width: 100%;border:none;outline: none;margin: 0px auto;text-align:center;' /></td><td width = '100' > 份数 </td><td width='205' id='no_of_contracts'><input type = 'text' readonly value=" + emailmain.no_of_contracts + " class='no_of_contracts' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td width = '100' > 合同名称 </td ><td colspan='5' width='530' id='ht_name'><input type = 'text' readonly value=" + emailmain.ht_name + " class='ht_name' style='width: 100%;border:none;height:100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td width = '100' > 承办部门 </td ><td colspan='3'><input value = " + emailmain.deptname + " readonly style='width:100%;border:none;outline: none;text-align: center;' type='text' class='dept_id_apply' /></td><td>合同类型</td><td><input type = 'text' readonly value=" + emailmain.httypename + " class='ht_type' style='border:none;outline: none;text-align: center;width:100%' /></td></tr><tr><td width = '100' > 合同期限 </td><td colspan='5' width='530'><input type = 'text' readonly value=" + emailmain.contract_begin_date + " class='contract_begin_date' style='width: 32%;border:none;outline: none;text-align:center;padding-left: 72px;margin-left: 9px;margin-right: -67px;' /><span style = 'margin-left: 143px;' > 至 </span><input type='text' readonly value=" + emailmain.contract_end_date + " class='contract_end_date' style='width: 40%;border:none;outline: none;margin-left: 69px;padding-left: 34px;text-align:center;' /></td></tr><tr><td width = '100' > 业务类型 </td><td colspan='3'><input type = 'text' readonly value=" + emailmain.businessname + " class='ht_business_type' style='border:none;outline: none;width: 100%;text-align: center;' /></td><td>送审日期</td><td id = 'date_time_create'><input type='text' readonly value=" + emailmain.date_time_create + " class='date_time_create' style='width: 100%;border:none;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>甲方</td><td colspan = '5' id='Party_A_name'><input type = 'text' value=" + emailmain.Party_A_name + " readonly class='Party_A_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center'/></td></tr><tr><td>甲方经办人</td><td width = '88' id='Party_A_apply_name'><input type = 'text' readonly value=" + emailmain.Party_A_apply_name + " class='Party_A_apply_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td width = '40' > 工号 </td ><td width='88' id='Party_A_apply_employee_no'><input type = 'text' readonly value=" + emailmain.Party_A_apply_employee_no + " class='Party_A_apply_employee_no' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td><td>联系方式</td><td id = 'Party_A_Contract' ><input type='text' readonly value=" + emailmain.Party_A_contract + " class='Party_A_contract' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>乙方</td><td colspan = '5' id='Party_B_name'><input type = 'text' readonly value=" + emailmain.Party_B_name + " class='Party_B_name' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td></tr><tr><td>乙方经办人</td><td id = 'Party_B_apply_name' ><input type='text' readonly value=" + emailmain.Party_B_apply_name + " class='Party_B_apply_name' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td><td>工号</td><td id = 'Party_B_apply_employee_no' ><input type='text' readonly value=" + emailmain.Party_B_apply_employee_no + " class='Party_B_apply_employee_no' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td>联系方式</td><td id = 'Party_B_contract'><input type='text' readonly value=" + emailmain.Party_B_contract + " class='Party_B_contract' style='width: 100%;border:none;height: 100%; outline: none; margin: 0px auto;text-align:center' /></td></tr><tr><td>标的单价</td><td colspan = '3' id='target_unit_price'><input type = 'text' readonly value=" + emailmain.target_unit_price + " class='target_unit_price' style='width: 100%;border:none;height: 100%;outline: none; margin: 0px auto;text-align:center' /></td><td>合同总价</td><td id = 'contract_price_total' ><input type='text' readonly value=" + emailmain.contract_price_total + " class='contract_price_total' style='width: 100%;border:none;height: 100%;outline: none;margin: 0px auto;text-align:center' /></td></tr><tr><td>主要内容</td><td colspan = '5' style='word-wrap:break-word;' width='530' id='ht_comments'><input class='ht_comments' readonly value=" + emailmain.ht_comments + " style='border:none;outline: none;text-align:center' /></td></tr>";

                    content = content + "</table><table border = '1' cellspacing='0' cellpadding='8' style='margin-left: 15.5%; margin-top: 4%; margin-right: 4%; ' align='center'><tbody align = 'center' ><tr><td width='100'>发票</td><td width = '548' colspan='3'>";
                    if (emailmain.invoice_type == null)
                    {
                        if (emailmain.contract_type == 2)
                        {
                            content = content + "<input type = 'text' name = 'contract_type' readonly style= 'text-align:center; outline: none; border: none; ' value = '增值税专用发票' />";
                        }
                        if (emailmain.contract_type == 3)
                        {
                            content = content + "<input type = 'text' name = 'contract_type' readonly style= 'text-align:center; outline: none; border: none; ' value = '增值税普通发票' />";
                        }
                    }
                    else
                    {
                        content = content + "<input type = 'text' name='contract_type' value=" + emailmain.invoice_type + " readonly style='text-align:center; outline: none; border: none; '/>";

                    }
                    content = content + "</td></tr><tr><td width = '100' > 税率 </td ><td width='548' colspan='3'><input readonly type='text' name='tax_rate' id='tax_rate' value=" + emailmain.tax_rate + " style='text-align:center; outline: none; border: none; '></td></tr><tr><td width = '100' > 账期 </td ><td width = '274' style='border-right:none;outline:none'><input readonly type='text' name='payment_type' id='payment_type' value=" + emailmain.paymentname + " style='text-align:center; outline: none; border: none; '></td><td colspan='2' width = '274' style='border-left:none'><input readonly type='text' name='payment_type_content' id='payment_type_content' value=";
                    if (string.IsNullOrEmpty(emailmain.payment_type_content))
                    {
                        content = content + "' ' style='outline: none; border: none; text-align:center' /></td></tr><tr><td width = '100' > 开票内容</td><td colspan = '3' style='word-wrap:break-word; ' width='546'><input type = 'text' value=" + emailmain.invoice_content + " readonly style='outline: none; border: none; ' /></td></tr><tr><td> 合同附件 </td><td colspan = '3' style ='word-wrap:break-word;'>";
                    }
                    else
                    {
                        content = content + emailmain.payment_type_content + " style='outline: none; border: none; text-align:center' /></td></tr><tr><td width = '100' > 开票内容</td><td colspan = '3' style='word-wrap:break-word; ' width='546'><input type = 'text' value=" + emailmain.invoice_content + " readonly style='outline: none; border: none; ' /></td></tr><tr><td> 合同附件 </td><td colspan = '3' style ='word-wrap:break-word;'>";
                    }
                    foreach (Myfile file in myfiles)
                    {
                        if (file.ht_id == emailmain.ht_id)
                        {
                            content = content + "<a href ='" + file.filesurl + "' > " + file.filesname + "</a><br/>";
                        }
                    }
                    content = content + string.Format("</td></tr></tbody></table></div><table style='border = '1' cellspacing='0' cellpadding='8' style='margin-left: 15.5%; margin-top: 4%; margin-right: 4%; '><tr style='margin-left: 70%; margin-top: 4%; '><td><a  href ='" + auditpass + "?auditid={0}&audit={1}&auditcomment={2}&userno={3}' > 通过 </a >", htid, "通过", myname + "于" + DateTime.Now + "审核通过", item);
                    content = content + string.Format("<a href='" + auditurlcancel + "?auditid={0}&audit={1}&auditcomment={2}&userno={3}' > 拒绝 </a>", htid, "拒绝", myname + "于" + DateTime.Now + "审核拒绝", item) + "</td></tr></table></div></body></html>";

                    string username = from.Split('@').First();
                    MailMessage newEmail = new MailMessage();
                    #region 发送方邮件
                    newEmail.From = new MailAddress(from, from);
                    #endregion

                    #region 发送对象，可群发
                    
                    newEmail.To.Add(new MailAddress(totmp));
                    
                    #endregion

                    #region Subject
                    newEmail.Subject = title;  //标题
                    #endregion

                    #region Body
                    string strBody = content; //html格式，也可以是普通文本格式 
                    newEmail.Body = strBody;  //内容
                    #endregion

                    #region 上传附件
                    // Attachment MsgAttach = new Attachment(this.FileUpload1.PostedFile.FileName);//可通过一个FileUpload地址获取附件地址
                    //newEmail.Attachments.Add(MsgAttach);
                    #endregion

                    #region Deployment
                    newEmail.IsBodyHtml = true;                //是否支持html
                    newEmail.Priority = MailPriority.High;  //优先级
                    #endregion

                    //发送方服务器信息
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(username, key);
                    smtpClient.Host = "mail.1hai.cn"; //主机xiaodongyu@1hai.cn


                    //smtpClient.Send(newEmail);   //同步发送,程序将被阻塞

                    #region 发送
                    smtpClient.Send(newEmail);
                    #endregion
                    //return Content("成功！！！！");
                }
            }
        }
    }


}
