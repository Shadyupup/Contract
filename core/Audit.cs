using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Contract.Models;
using Contract.DAL;

using System.ServiceModel;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Contract.ServiceReference;
using System.Configuration;
using System.Net.Http;

namespace Contract.core
{
    public class AuditMethod
    {
        bool key = true;
        bool key2 = true;
        bool key3 = true;
        bool key4 = true;
        bool key5 = true;
        bool key6 = true;
        private ContractContext db = new ContractContext();
        int htid;
        List<int?> usernums = new List<int?>();
        List<string> name = new List<string>();
        int usernameno;
        List<string> audituser = new List<string>();
        public viewModel Auditshow(int usernum)
        {
            int? usergroup = getgroup_id(usernum);
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);

            var tmp = new SearchLevel();
            List<htmain> ht = new List<htmain>();
            ht = tmp.GetHTAU(usernum, usergroup);


            var tmp2 = new SearchLevel();
            List<Auditdetail> detail = new List<Auditdetail>();
            detail = tmp2.GetDetail().OrderBy(x => x.priority_order).ToList();
            foreach (htmain mymain in ht)
            {
                foreach (Auditdetail mydetail in detail)
                {
                    if (mymain.ht_id == mydetail.ht_id)
                    {
                        if ((mydetail.employee_no_audit == usernum) && (mydetail.status_flag == "N"))
                        {
                            var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                            details.ToList();
                            foreach (HT_audit_detail otherdetail in details)
                            {
                                if ((otherdetail.priority_order > mydetail.priority_order) && (otherdetail.status_flag != "N"))
                                {
                                    mymain.show_flag = "F";
                                    key = false;
                                    break;
                                }
                                key = true;
                            }
                            if (key)
                            {
                                mymain.show_flag = "T";
                            }
                        }
                        else
                        {
                            if ((mydetail.group_id == usergroup) && (mydetail.status_flag == "N"))
                            {
                                var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                                details.ToList();
                                foreach (HT_audit_detail otherdetail in details)
                                {
                                    if ((otherdetail.priority_order > mydetail.priority_order) && (otherdetail.status_flag != "N"))
                                    {
                                        mymain.show_flag = "F";
                                        key2 = false;
                                        break;
                                    }
                                    key2 = true;
                                }
                                if (key2)
                                {
                                    mymain.show_flag = "T";
                                }
                            }
                        }
                    }
                }
            }


            foreach (htmain mymain in ht)
            {
                foreach (Auditdetail mydetail in detail)
                {
                    if (mymain.ht_id == mydetail.ht_id)
                    {
                        if ((mydetail.employee_no_audit == usernum) && (mydetail.additional_audit_flag == "F"))
                        {
                            var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                            details.ToList();
                            foreach (HT_audit_detail otherdetail in details)
                            {
                                if ((otherdetail.priority_order > mydetail.priority_order) && (otherdetail.additional_audit_flag != "F"))
                                {
                                    mymain.show_add = "F";
                                    key5 = false;
                                    break;
                                }
                                key5 = true;
                            }
                            if (key5)
                            {
                                mymain.show_add = "T";
                            }
                        }
                        if ((mydetail.group_id == usergroup) && (mydetail.additional_audit_flag == "F"))
                        {
                            var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                            details.ToList();
                            foreach (HT_audit_detail otherdetail in details)
                            {
                                if ((otherdetail.priority_order > mydetail.priority_order) && (otherdetail.additional_audit_flag != "F"))
                                {
                                    mymain.show_add = "F";
                                    key6 = false;
                                    break;
                                }
                                key6 = true;
                            }
                            if (key6)
                            {
                                mymain.show_add = "T";
                            }
                        }
                        if ((mydetail.employee_no_audit == usernum) && (mydetail.additional_audit_flag == "Z"))
                        {
                            var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                            details.ToList();
                            foreach (HT_audit_detail otherdetail in details)
                            {
                                if ((otherdetail.priority_order > mydetail.priority_order) && (otherdetail.additional_audit_flag != "F"))
                                {
                                    mymain.show_add = "F";
                                    key6 = false;
                                    break;
                                }
                                key6 = true;
                            }
                            if (key6)
                            {
                                mymain.show_add = "T";
                            }
                        }
                    }
                }
            }


            vm.htmainModel = ht.OrderByDescending(p => p.date_time_create).ToList();
            vm.AuditdetailModel = detail;


            return vm;
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

        public int getno(string name)
        {
            var no = from a in db.rs_users where a.user_name == name select a.user_code;
            string notemp = no.FirstOrDefault();
            return (int.Parse(notemp));
        }
        public string getname(int num)
        {
            var name = from a in db.rs_users where int.Parse(a.user_code) == num select a.user_name;
            return (name.ToString());
        }
        public string getGroupName(int num)
        {

            var name = from a in db.rs_groups where a.group_id == num select a.group_name;
            return (name.ToString());
        }



        public viewModel AuditQuery(string ht_no, string user_name, string contract_price_total_smaller, string contract_price_total_bigger, string Party_A_name, string Party_B_name, DateTime? contract_begin_date, DateTime? contract_end_date, string Party_A_apply_name, string Party_B_apply_name, int usernum)
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            int? usergroup = getgroup_id(usernum);

            if (!string.IsNullOrEmpty(user_name))
            {
                usernameno = getno(user_name);
            }
            int user_name_no = usernameno;

            var tmp = new SearchLevel();
            List<htmain> queryHT = new List<htmain>();
            queryHT = tmp.GetHTAUquery(contract_begin_date, contract_end_date, Party_A_name, Party_B_name, ht_no, user_name_no, Party_A_apply_name, Party_B_apply_name, contract_price_total_smaller, contract_price_total_bigger, usernum, usergroup);


            var tmp2 = new SearchLevel();
            List<Auditdetail> detail = new List<Auditdetail>();
            detail = tmp2.GetDetail().OrderBy(x => x.priority_order).ToList();

            foreach (htmain mymain in queryHT)
            {
                foreach (Auditdetail mydetail in detail)
                {
                    if (mymain.ht_id == mydetail.ht_id)
                    {
                        if ((mydetail.employee_no_audit == usernum) && (mydetail.status_flag == "N"))
                        {
                            var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                            details.ToList();
                            foreach (HT_audit_detail otherdetail in details)
                            {
                                if ((otherdetail.priority_order >= mydetail.priority_order) && (otherdetail.status_flag != "N"))
                                {
                                    key3 = false;
                                    break;
                                }
                            }
                            if (key3)
                            {
                                mymain.show_flag = "T";
                            }
                        }
                        else
                        {
                            if ((mydetail.group_id == usergroup) && (mydetail.status_flag == "N"))
                            {
                                var details = from b in db.HT_audit_details where b.ht_id == mydetail.ht_id select b;
                                details.ToList();
                                foreach (HT_audit_detail otherdetail in details)
                                {
                                    if ((otherdetail.priority_order >= mydetail.priority_order) && (otherdetail.status_flag != "N"))
                                    {
                                        key4 = false;
                                        break;
                                    }
                                }
                                if (key4)
                                {
                                    mymain.show_flag = "T";
                                }
                            }
                        }
                    }
                }
            }
            vm.htmainModel = queryHT.OrderByDescending(p => p.date_time_create).ToList();
            vm.AuditdetailModel = detail;
            return vm;

        }


        public viewModel pass(string ht_no)
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            return vm;
        }

        public viewModel cancel(string ht_no)
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            return vm;
        }

        public viewModel add(string ht_no)
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), db.HT_Mains.ToList(), db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            return vm;
        }
    }
}

