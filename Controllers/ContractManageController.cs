using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contract.Models;

namespace Contract.Controllers
{
    public class ContractManageController : Controller
    {
        // GET: ContractApply
        //private rs_groupDAL db = new rs_groupDAL();
        //private HT_payment_typeDAL db = new HT_payment_typeDAL();
        //private HT_MainDAL db = new HT_MainDAL();
        //private rs_userDAL db = new rs_userDAL();
        //private HT_audit_detailDAL db = new HT_audit_detailDAL();
        //private ht_audit_conditionDAL db = new ht_audit_conditionDAL();
        //private HT_user_listDAL db = new HT_user_listDAL();
        //private HT_main_filesDAL db = new HT_main_filesDAL();
        //private HT_Main_statusDAL db = new HT_Main_statusDAL();

        private ContractContext db = new ContractContext();

        public ActionResult Index()

        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList());





            string userno = TempData["userno"] as string;
            TempData.Keep("userno");

            var contractu = from u in db.rs_users where u.user_code == userno select u.user_name;
            contractu.ToArray();
            foreach (string item in contractu)
            {
                ViewData["user"] = item;
            }






            return View("ContractManage", vm);
        }


        public ActionResult minitable()
        {
            return View("minitable");
        }
        public string getno(string name)
        {
            var no = from a in db.rs_users where a.user_name == name select a.user_code;

            return (no.ToString());
        }
        public string getname(int num)
        {
            var name = from a in db.rs_users where int.Parse(a.user_code) == num select a.user_name;
            return (name.ToString());
        }
        [HttpPost]
        public ActionResult Index(HT_Main ht_main, string user_name,string last_audit_name, decimal contract_price_total_smaller, decimal contract_price_total_bigger)
        {

            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList());


            var contracts = from c in db.HT_Mains
                            select c;

            if (!String.IsNullOrEmpty(ht_main.ht_no))
            {
                contracts = contracts.Where(x => x.ht_no == ht_main.ht_no);

            }
            else
            {
                if (contract_price_total_smaller != 0)
                {
                    contracts = contracts.Where(x => x.contract_price_total >= contract_price_total_smaller);
                }
                if (contract_price_total_bigger != 0)
                {
                    contracts = contracts.Where(x => x.contract_price_total <= contract_price_total_bigger);
                }
                if (!String.IsNullOrEmpty(ht_main.Party_A_name))
                {
                    contracts = contracts.Where(x => x.Party_A_name == ht_main.Party_A_name);
                }
                if (!String.IsNullOrEmpty(ht_main.Party_A_apply_name))
                {
                    contracts = contracts.Where(x => x.Party_A_apply_name == ht_main.Party_A_apply_name);
                }

                if (!String.IsNullOrEmpty(ht_main.Party_B_name))
                {
                    contracts = contracts.Where(x => x.Party_B_name == ht_main.Party_B_name);
                }
                if (!String.IsNullOrEmpty(ht_main.Party_B_apply_name))
                {
                    contracts = contracts.Where(x => x.Party_B_apply_name == ht_main.Party_B_apply_name);
                }

                if (ht_main.contract_begin_date != DateTime.MinValue)
                {
                    contracts = contracts.Where(x => x.contract_begin_date >= ht_main.contract_begin_date);
                }
                if (ht_main.contract_end_date != DateTime.MinValue)
                {
                    contracts = contracts.Where(x => x.contract_end_date <= ht_main.contract_end_date);
                }
                if (ht_main.date_time_create != DateTime.MinValue)
                {
                    contracts = contracts.Where(x => x.date_time_create == ht_main.date_time_create);
                }
                if (ht_main.date_time_last_audit != DateTime.MinValue)
                {
                    contracts = contracts.Where(x => x.date_time_last_audit == ht_main.date_time_last_audit);
                }
                if (!String.IsNullOrEmpty(ht_main.status_flag))
                {
                    contracts = contracts.Where(x => x.status_flag == ht_main.status_flag);
                }
                if (!String.IsNullOrEmpty(getno(last_audit_name)))
                {
                    contracts = contracts.Where(x => x.employee_no_last_audit.ToString() == getno(last_audit_name));
                }
                if (!String.IsNullOrEmpty(user_name))
                {
                    contracts = contracts.Where(x => x.employee_no_opr.ToString() == getno(user_name));
                }
            }

            vm.HT_MainModel = contracts.ToList();

            //申请人
            //rs_user user = new rs_user();
            //user.user_name = "2";
            //user.user_code = "2";
            //user.user_status = "2";
            //vm.rs_userModel.Add(user);

            return View("ContractManage", vm);
        }
    }
}
