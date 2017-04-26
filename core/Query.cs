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

namespace Contract.core
{
    public class QueryMethod
    {
        int htid;
        string byname;
        private ContractContext db = new ContractContext();


        public viewModel QueryShow(string userno)
        {
            int usernum = int.Parse(userno);
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);
            var tmp2 = new SearchLevel();

            List<htmain> ht = new List<htmain>();
            ht = tmp2.GetHTQU(usernum);
            vm.htmainModel=ht.OrderByDescending(p => p.date_time_create).ToList();

            List<Auditdetail> detail = new List<Auditdetail>();
            detail = tmp2.GetDetail().OrderBy(x => x.priority_order).ToList(); 
            vm.AuditdetailModel = detail;

            return vm;
        }

        public string getno(string name)
        {
            var no = from a in db.rs_users where a.user_name == name select a.user_code;
            return (no.ToString());
        }
        public string getname(int num)
        {
            var name = from a in db.rs_users where int.Parse(a.user_code) == num select a.user_name;
            name.ToArray();
            foreach (string item in name)
            {
                if (!string.IsNullOrEmpty(item))
                    byname = item;
                else byname = null;
            }
            return (byname);
        }
        public string getGroupName(int num)
        {
            var name = from a in db.rs_groups where a.group_id == num select a.group_name;
            return (name.ToString());
        }

        public viewModel queryquery(string userno, string ht_no, string contract_price_total_smaller, string contract_price_total_bigger, string Party_A_name, string Party_B_name, DateTime? contract_begin_date, DateTime? contract_end_date)
        {
            int usernum = int.Parse(userno);
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);

            var tmp = new SearchLevel();
            List<htmain> queryHT = new List<htmain>();
            queryHT = tmp.GetHTquery(contract_begin_date, contract_end_date, Party_A_name, Party_B_name, ht_no, contract_price_total_smaller, contract_price_total_bigger, usernum);
            vm.htmainModel = queryHT.OrderByDescending(p => p.date_time_create).ToList();
            
            var tmp2 = new SearchLevel();
            List<Auditdetail> detail = new List<Auditdetail>();
            detail = tmp2.GetDetail().OrderBy(x => x.priority_order).ToList();
            vm.AuditdetailModel = detail;

            return vm;
            
        }
        
        public viewModel add()
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);
            return vm;
        }

        public viewModel cancel()
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);
            return vm;
        }

        public viewModel edit(string ht_no)
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(),null,null,null);
            return vm;
        }
    }
}