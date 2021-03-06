﻿using System.Collections.Generic;

namespace expense.manager.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public double? AmmountSpent { get; set; }

        public double? Budget { get; set; }


        public double RecurringBudget { get; set; }


        public ICollection<Category> ChildrenCategories { get; set; }

        public Category ParentCategory { get; set; }


        public override bool Equals(object obj)
        {

            if(obj is Category otherCateg)
            {
                return this.Id == otherCateg.Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}