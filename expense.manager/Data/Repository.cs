﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using expense.manager.Mapping;
using expense.manager.Models;
using Microsoft.EntityFrameworkCore;

namespace expense.manager.Data
{
    public class Repository : IRepository
    {

        private readonly ExpenseManagerContext _currentContext;

        public Repository(ExpenseManagerContext context)
        {
            _currentContext = context;
        }
        public async Task<IEnumerable<ExpenseData>> GetExpenses(Expression<Func<ExpenseData, bool>> predicate)
        {
            
                return await _currentContext.Expenses.Where(predicate).ToListAsync();

        }
        private async Task<List<int>> GetChildrenCategoriesId(int id)
        {
            
            
                var result = new List<int>();
                var subcategories = _currentContext.ExpenseCategories.Where(c => c.ParentId == id);
                result.AddRange(subcategories.Select(n => n.Id));
                foreach (var item in subcategories)
                {
                    result.AddRange(await GetChildrenCategoriesId(item.Id));

                }
                return result;

            

        }
        private async Task<List<int>> GetChildrenRecursive(int id, IEnumerable<CategoryData> list)
        {
            var result = new List<int>();
            var subcategories = list.Where(c => c.ParentId == id);
            result.AddRange(subcategories.Select(n => n.Id));
            foreach (var item in subcategories)
            {
                result.AddRange(await GetChildrenCategoriesId(item.Id));

            }
            return result;

        }
        public  Task<IEnumerable<CategoryData>> GetCategories(Expression<Func<CategoryData,bool>> predicate)
        {


            var result = _currentContext.ExpenseCategories.Where(predicate).ToList();

            return Task.FromResult(result.AsEnumerable());

        }
        public async Task<IEnumerable<ExpenseData>> GetExpenseFromTagAsync(int tagId)
        {
            
            
                var ids = _currentContext.ExpenseTagRelations.Where(n => n.TagId == tagId).Select(p => p.ExpenseId).ToList();
                return await _currentContext.Expenses.Where(n => ids.Contains(n.Id)).ToListAsync();
            



        }
        public Task DeleteExpense(ExpenseData expense)
        {
            
            
                _currentContext.Expenses.Remove(expense);

                var itemsToRemove = _currentContext.ExpenseTagRelations.Where(n => n.ExpenseId == expense.Id);
                foreach (var item in itemsToRemove)
                {
                    _currentContext.ExpenseTagRelations.Remove(item);
                }

                return Task.CompletedTask;
            
        }
        public async Task<int> AddExpense(ExpenseData expense)
        {
            
            
                if (expense.Id == 0)
                {

                    await _currentContext.Expenses.AddAsync(expense);

                }
                else
                {
                    _currentContext.Entry(expense).State = EntityState.Modified;
                }


                

                return expense.Id;
            


        }
        public async Task<IEnumerable<TagData>> GetAllTags()
        {
            
            {
                return await _currentContext.ExpenseTags.ToListAsync();

            }

        }
        public async Task<IEnumerable<TagData>> GetTagsByExpenseId(int expenseId)
        {
            
            {
                var tagsId = _currentContext.ExpenseTagRelations.Where(n => n.ExpenseId == expenseId).Select(p => p.TagId);
                return await _currentContext.ExpenseTags.Where(n => tagsId.Contains(n.Id)).ToListAsync();
            }


        }
        public Task DeleteTag(TagData tag)
        {
            
            {
                _currentContext.ExpenseTags.Remove(tag);
                var itemsToRemove = _currentContext.ExpenseTagRelations.Where(n => n.TagId == tag.Id);
                foreach (var item in itemsToRemove)
                {
                    _currentContext.ExpenseTagRelations.Remove(item);
                }
                return Task.CompletedTask;
            }

        }
        public async Task<int> AddTag(TagData tag)
        {
            
            {
                if (tag.Id == 0)
                {
                    await _currentContext.ExpenseTags.AddAsync(tag);
                }

                else
                {

                    _currentContext.Entry(tag).State = EntityState.Modified;
                }

                

                return tag.Id;
            }

        }
        public async Task<IEnumerable<CategoryData>> GetAllCategories()
        {
            
            {
                return await _currentContext.ExpenseCategories.ToListAsync();

            }
        }
        public async Task<CategoryData> GetCategory(int categoryId)
        {
            
            {
                return await _currentContext.ExpenseCategories.SingleAsync(t => t.Id == categoryId);

            }
        }
        public Task DeleteCategory(CategoryData category)
        {
            _currentContext.ExpenseCategories.Remove(category);

            return Task.CompletedTask;

            

        }
        public async Task<int> AddCategory(CategoryData category)
        {
            
            {
                if (category.Id == 0)
                {
                    await _currentContext.ExpenseCategories.AddAsync(category);
                }

                else
                {
                    _currentContext.ExpenseCategories.Update(category);
                }


                

                return category.Id;
            }
        }
        public Task<double?> GetSpecialBudget(int categoryId, string monthId)
        {
            
            {
                var result = _currentContext.Budgets.SingleOrDefault(b => b.CategoryId == categoryId && b.MonthId == monthId);

                return Task.FromResult(result?.BudgetAmmount);

            }
        }
        public Task AddTagsToExpense(int expenseId, IEnumerable<int> tagsIds)
        {
            
            {
                var itemsToRemove = _currentContext.ExpenseTagRelations.Where(n => n.ExpenseId == expenseId);

                itemsToRemove.ForEachAsync(i =>
                {
                    _currentContext.ExpenseTagRelations.Remove(i);
                });


                foreach (var tagId in tagsIds)
                {
                    _currentContext.ExpenseTagRelations.Add(new ExpenseTagRelationData { ExpenseId = expenseId, TagId = tagId });
                }

                return Task.CompletedTask;
            }

        }
        public async Task AddOrUpdateSpecialBuget(CategoryData category, string monthId, double newBudget)
        {

            
            {
                if (!_currentContext.Budgets.Any(n => n.CategoryId == category.Id && n.MonthId == monthId))
                {

                    await _currentContext.Budgets.AddAsync(new BudgetData
                    {
                        CategoryId = category.Id,
                        BudgetAmmount = newBudget,
                        MonthId = monthId
                    });

                }
                else
                {
                    var budgetToUpdate = _currentContext.Budgets.Single(n => n.CategoryId == category.Id && n.MonthId == monthId);
                    budgetToUpdate.BudgetAmmount = newBudget;
                    _currentContext.Budgets.Update(budgetToUpdate);

                }


                

            }

        }
        public async Task PersistPreviousMonthBudget(string monthId)
        {
            
            {
                foreach (var categ in _currentContext.ExpenseCategories)
                {
                    var specialbudget = _currentContext.Budgets.SingleOrDefault(n => n.CategoryId == categ.Id && n.MonthId == monthId);

                    if (specialbudget == null && categ.RecurringBudget!=0)
                    {
                        await _currentContext.Budgets.AddAsync(new BudgetData
                        {
                            CategoryId = categ.Id,
                            BudgetAmmount = categ.RecurringBudget,
                            MonthId = monthId
                        });
                    }

                }


                

            }

        }

        public Task SupressExpenseTagRelation(ExpenseData expense, TagData tag)
        {
            {
                var itemsToRemove = _currentContext.ExpenseTagRelations.Where(n => n.ExpenseId == expense.Id && n.TagId == tag.Id);

                foreach (var item in itemsToRemove)
                {
                    _currentContext.ExpenseTagRelations.Remove(item);
                }

                return Task.CompletedTask;
            }

        }



    }


}