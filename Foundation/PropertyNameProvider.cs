using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Foundation
{
    public class PropertyNameProvider
    {

        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            var unaryExpression = expression.Body as UnaryExpression;

            if (unaryExpression != null)
                memberExpression = unaryExpression.Operand as MemberExpression;

            if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Property)
                throw new Exception("Invalid lambda expression format.");

            return memberExpression.Member.Name;
        }
    }
}
