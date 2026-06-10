using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public interface ICategoryService
    {
        public List<Category> GetCategories();
    }
}
