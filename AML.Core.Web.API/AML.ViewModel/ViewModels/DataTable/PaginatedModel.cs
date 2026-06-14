using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.DataTable
{
    public class DataTableModel
    {
        // properties are not capital due to json mapping
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public int? status { get; set; }
        public int? isdue { get; set; }
        public int? itemId { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<Order> order { get; set; }
        public int expiredquals { get; set; }
        public int notloggedin { get; set; }
        public int expiredindcs { get; set; }
        public int? investigated { get; set; }
        public int? reported { get; set; }
        public string CustomFilterOne { get; set; }
        public string CustomFilterTwo { get; set; }
        //public AuthModel AuthToken { get; set; }
        public int RoleID { get; set; }
        public int ItemTypeId { get; set; }
        public int? SiteID { get; set; }
        public int? PositionId { get; set; }
        public int? QualificationId { get; set; }
        public string Permission { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
}
