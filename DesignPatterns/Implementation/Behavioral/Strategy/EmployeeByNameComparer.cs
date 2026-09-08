using System.Collections.Generic;

namespace Implementation.Behavioral.Strategy
{
	class EmployeeByNameComparer : IComparer<Employee>
	{
		public int Compare(Employee x, Employee y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}


}
