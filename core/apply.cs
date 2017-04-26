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


namespace Contract.core
{
    public class apply
    {
        private ContractContext db = new ContractContext();
        int k = 0;
        int? areaBoss = 0;
        int? areaBoss1 = 0;
        int? areaBoss2 = 0;
        int? auditgroupid;
        int? managerC = 0, managerD = 0, managerE = 0, managerC_group = 0, managerD_group = 0, managerE_group = 0;

        public viewModel ContractApplyShow()
        {
            var vm = new viewModel(db.rs_groups.ToList(), db.rs_levels.ToList(), db.HT_payment_types.ToList(), null, db.rs_users.ToList(), db.HT_audit_details.ToList(), db.ht_audit_conditions.ToList(), db.HT_user_lists.ToList(), db.HT_main_filess.ToList(), db.HT_Main_statuss.ToList(), db.HT_types.ToList(), db.HT_Business_types.ToList(), null, null, null);
            vm.rs_groupModel = db.rs_groups.ToList();
            vm.rs_levelModel = db.rs_levels.ToList();
            vm.HT_payment_typeModel = db.HT_payment_types.OrderByDescending(p => p.payment_type_name).ToList();
            vm.rs_userModel = db.rs_users.OrderByDescending(p => p.group_id).ToList();
            vm.HT_audit_datailModel = db.HT_audit_details.OrderByDescending(p => p.id).ToList();
            vm.ht_audit_conditionModel = db.ht_audit_conditions.OrderByDescending(p => p.id).ToList(); ;
            vm.HT_user_listModel = db.HT_user_lists.OrderByDescending(p => p.user_code).ToList(); ;
            vm.HT_main_filesModel = db.HT_main_filess.OrderByDescending(p => p.id).ToList();
            vm.HT_Main_statusModel = db.HT_Main_statuss.OrderByDescending(p => p.status_code).ToList();
            vm.HT_typeModel = db.HT_types.OrderByDescending(p => p.ht_type).ToList();
            vm.HT_Business_typeModel = db.HT_Business_types.OrderByDescending(p => p.business_type).ToList();

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
                    //if (items.ToString() == null)
                    //{
                    //    return (null);
                    //}
                    //else 
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

        public List<HT_audit_detail> detailstore(HT_Main hT_Main, HT_audit_detail hT_audit_detail, HT_audit_detail hT_audit_detail1, HT_audit_detail hT_audit_detail2, HT_audit_detail hT_audit_detail3, HT_audit_detail hT_audit_detail4, HT_audit_detail hT_audit_detail5,
             HT_audit_detail hT_audit_detail6, HT_audit_detail hT_audit_detail7,
            string upload, string business_type_name, string ht_type, string dept_id_apply, string contract_type, string payment_type /*string Party_A_apply_name, string Party_B_apply_name,*/, int? Party_A_apply_employee_no, int? Party_B_apply_employee_no)
        {
            int? apply_employee_no;
            int? apply_user_id;

            if (Party_A_apply_employee_no != null)
            {
                apply_employee_no = Party_A_apply_employee_no;
            }
            else apply_employee_no = Party_B_apply_employee_no;

            //var apply_group_ids = from a in db.rs_users where int.Parse(a.user_code) == apply_employee_no select a.group_id;
            //apply_group_ids.ToArray();
            //foreach (int? item in apply_group_ids)
            //{
            //    int? apply_group_id = item;
            int? apply_group_id = getgroup_id(apply_employee_no);
            auditgroupid = apply_group_id;

            //SearchLevel.
            apply_user_id = getuserid(apply_employee_no);
            var Manager = new SearchLevel();
            List<SearchManager> searchManager = new List<SearchManager>();
            searchManager = Manager.GetListPositive(apply_group_id, apply_user_id);
            List<HT_audit_detail> Audit_details = new List<HT_audit_detail>();
            //  HT_audit_detail hT_audit_detail1, hT_audit_detail2, hT_audit_detail3, hT_audit_detail4, hT_audit_detail5;

            foreach (SearchManager items in searchManager)
            {
                int? item_group_id = items.group_id_audit;
                int? item_id = items.manager_id_audit;

                //var levelModes = from b in db.rs_users where b.group_id == item_group_id select b.level_id;
                //levelModes.ToList();

                //    foreach (int? item in levelModes)
                //    {
                //var levelMode = getlevel_mode(int.Parse(level_Modes));
                //var levelModeDeep = getlevel_mode_deep(int.Parse(level_Modes));

                var auditgroup = from c in db.rs_groups
                                 where c.group_name == "非诉讼组"
                                 select c.group_id;
                auditgroupid = auditgroup.FirstOrDefault();


                if (items.level_main == "C")
                {
                    managerC = getname_bygroup(item_group_id, items.level_id);
                    managerC_group = item_group_id;
                }
                else if (items.level_main == "D")
                {
                    managerD = getname_bygroup(item_group_id, items.level_id);
                    managerD_group = item_group_id;
                }
                else if (items.level_main == "E")
                {
                    managerE = getname_bygroup(item_group_id, items.level_id);
                    managerE_group = item_group_id;
                }
                else if (items.level_main == "F")
                {
                    if (items.level_code == "F1")
                    {
                        areaBoss1 = getname_bygroup(item_group_id, items.level_id);
                    }

                    else if (items.level_code == "F2")
                    {
                        areaBoss2 = getname_bygroup(item_group_id, items.level_id);
                    }
                    //else if (items.level_main == "F3")
                    //{
                    //    areaBoss3 = getname_bygroup(item_group_id, items.level_id);
                    //}
                }
            }
            //search apply manager(C\D\E)
            if (managerE != 0)
            {
                hT_audit_detail.ht_id = hT_Main.ht_id;
                hT_audit_detail.employee_no_audit = managerE;
                hT_audit_detail.group_id = managerE_group;
                hT_audit_detail.priority_order = k++;
                hT_audit_detail.status_flag = "N";//XDY改了
                hT_audit_detail.additional_audit_flag = "F";//XDY改了
                hT_audit_detail.create_date_time = DateTime.Now;
                Audit_details.Add(hT_audit_detail);
            }
            else
            {
                if (managerD != 0)
                {
                    hT_audit_detail1.ht_id = hT_Main.ht_id;
                    hT_audit_detail1.employee_no_audit = managerD;
                    hT_audit_detail1.group_id = managerD_group;
                    hT_audit_detail1.priority_order = k++;
                    hT_audit_detail1.status_flag = "N";//XDY改了
                    hT_audit_detail1.additional_audit_flag = "F";//XDY改了
                    hT_audit_detail1.create_date_time = DateTime.Now;
                    Audit_details.Add(hT_audit_detail1);
                }
                else
                {
                    if (managerC != 0)
                    {
                        hT_audit_detail2.ht_id = hT_Main.ht_id;
                        hT_audit_detail2.employee_no_audit =managerC;
                        hT_audit_detail2.group_id = managerC_group;
                        hT_audit_detail2.priority_order = k++;
                        hT_audit_detail2.status_flag = "N";//XDY改了
                        hT_audit_detail2.additional_audit_flag = "F";//XDY改了
                        hT_audit_detail2.create_date_time = DateTime.Now;
                        Audit_details.Add(hT_audit_detail2);
                    }
                }
            }
            if (areaBoss1 != 0)
            {
                areaBoss = areaBoss1;
            }
            else
            {
                if (areaBoss2 != 0)
                {
                    areaBoss = areaBoss2;
                }
                //else
                //{
                //    if (areaBoss3 != null)
                //    {
                //        areaBoss = areaBoss3;
                //    }
                //}
            }
            if (hT_Main.is_standard == "1")
            {
                hT_audit_detail3.ht_id = hT_Main.ht_id;
                hT_audit_detail3.group_id = auditgroupid;//XDY改了
                hT_audit_detail3.priority_order = k++;
                hT_audit_detail3.status_flag = "N";//XDY改了
                hT_audit_detail3.additional_audit_flag = "F";//XDY改了
                hT_audit_detail3.create_date_time = DateTime.Now;
                Audit_details.Add(hT_audit_detail3);
            }
            if (getgroup_name(apply_group_id) == "企业销售部")
            {
                var CFO = from a in db.HT_user_lists where a.position_flag == "C" select a.user_code;
                string CFOS = CFO.ToString();
                int? cfo = int.Parse(CFO.FirstOrDefault());
                var CFO_group = from a in db.rs_users where a.user_code == CFOS select a.group_id;
                int? cfo_group = CFO_group.FirstOrDefault();
                hT_audit_detail4.ht_id = hT_Main.ht_id;
                hT_audit_detail4.employee_no_audit = cfo;
                hT_audit_detail4.group_id = cfo_group;
                hT_audit_detail4.priority_order = k++;
                hT_audit_detail4.status_flag = "N";//XDY¸ÄÁË
                hT_audit_detail4.additional_audit_flag = "F";//XDY¸ÄÁË
                hT_audit_detail4.create_date_time = DateTime.Now;
                Audit_details.Add(hT_audit_detail4);
            }


            else
            {
                //nextaudit=财务总监
                var CF = from a in db.HT_user_lists where a.position_flag == "F" select a.user_code;
                string cfs = CF.ToString();
                int? cf = int.Parse(CF.FirstOrDefault());
                var CF_group = from a in db.rs_users where a.user_code == cfs select a.group_id;
                int? cf_group = CF_group.FirstOrDefault();

                hT_audit_detail5.employee_no_audit = cf;
                hT_audit_detail5.ht_id = hT_Main.ht_id;
                hT_audit_detail5.group_id = cf_group;
                hT_audit_detail5.priority_order = k++;
                hT_audit_detail5.status_flag = "N";//XDY改了
                hT_audit_detail5.additional_audit_flag = "F";//XDY改了
                hT_audit_detail5.create_date_time = DateTime.Now;
                Audit_details.Add(hT_audit_detail5);

            }


            if (hT_Main.is_standard == "1")
            {
                //nextaudit=法务经理
                var Legal = from a in db.HT_user_lists where a.position_flag == "L" select a.user_code;
                string legals = Legal.ToString();
                int? legal = int.Parse(Legal.FirstOrDefault());
                var Legal_group = from a in db.rs_users where a.user_code == legals select a.group_id;
                int? legal_group = Legal_group.FirstOrDefault();
                hT_audit_detail6.ht_id = hT_Main.ht_id;
                hT_audit_detail6.employee_no_audit = legal;
                hT_audit_detail6.group_id = legal_group;//XDY改了
                hT_audit_detail6.priority_order = k++;
                hT_audit_detail6.status_flag = "N";//XDY改了
                hT_audit_detail6.additional_audit_flag = "F";//XDY改了
                hT_audit_detail6.create_date_time = DateTime.Now;
                Audit_details.Add(hT_audit_detail6);

            }
            //string prices = "SELECT contract_price_total FROM HT_Mains WHERE ht_no=ht_no";
            var maxprices = from b in db.ht_audit_conditions // where  *=auditgroupid 根据部门确定限制金额
                            select b.type_par;
            decimal? maxprice = maxprices.FirstOrDefault();
            var prices = from b in db.HT_Mains where b.ht_no == hT_Main.ht_no select b.contract_price_total;
            prices.ToArray();

            foreach (decimal item2 in prices)
            {
                decimal totalprices = item2;

                if (totalprices >= maxprice && areaBoss != 0)

                //if(totalprices >= addition.typebar)
                {
                    //nextaudit=所属大部副总裁
                    hT_audit_detail7.ht_id = hT_Main.ht_id;
                    hT_audit_detail7.employee_no_audit = areaBoss;
                    hT_audit_detail7.group_id = areaBoss;
                    hT_audit_detail7.priority_order = k++;
                    hT_audit_detail7.status_flag = "N";//XDY改了
                    hT_audit_detail7.additional_audit_flag = "F";//XDY改了
                    hT_audit_detail7.create_date_time = DateTime.Now;
                    Audit_details.Add(hT_audit_detail7);
                }

            }

            return Audit_details;
        }
    }
}