using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CustomerCategory
{
    public class CustomerCategoryModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int IsActive { get; set; }
        public int Score { get; set; }
    }
}
