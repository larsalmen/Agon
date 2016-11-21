using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Agon.Models
{
    public abstract class ViewModelBase
    {
        public string Title { get; set; }

        public ViewModelBase(string title)
        {
            Title = title;
        }
    }
}
