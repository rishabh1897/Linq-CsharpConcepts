using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Expressions.Task3.E3SQueryProvider
{
    public class ExpressionToFtsRequestTranslator : ExpressionVisitor
    {
        readonly StringBuilder _resultStringBuilder;

        public ExpressionToFtsRequestTranslator()
        {
            _resultStringBuilder = new StringBuilder();
        }

        public string Translate(Expression exp)
        {
            Visit(exp);

            return _resultStringBuilder.ToString();
        }

        #region protected methods

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }
            switch (node.Method.Name)
            {
                case "StartsWith":
                    VisitCommonMethodFormatter(node);
                    return node;                    
                case "EndsWith":
                    VisitCommonMethodFormatter(node);
                    return node;
                case "Contains":
                    VisitCommonMethodFormatter(node);
                    return node;
                case "Equals":
                    VisitCommonMethodFormatter(node);
                    return node;
            }
            return base.VisitMethodCall(node);
        }
        
        protected Expression VisitCommonMethodFormatter(MethodCallExpression node)
        {
            var mainArg = node.Arguments[0];
            var lambda = Expression.Lambda<Func<string>>(mainArg);
            var arg = lambda.Compile()();
            var member = node.Object as MemberExpression;
            if (node.Method.Name == "EndsWith")
            {
                _resultStringBuilder.AppendFormat($"{member.Member.Name}:(*{arg})");
            }
            else if (node.Method.Name == "Contains")
            {
                _resultStringBuilder.AppendFormat("{0}:(*{1}*)", member.Member.Name, arg);
            }
            else if (node.Method.Name == "StartsWith")
            {
                _resultStringBuilder.AppendFormat("{0}:({1}*)", member.Member.Name, arg);
            }
            else if(node.Method.Name == "Equals")
            {
                _resultStringBuilder.AppendFormat("{0}:({1})", member.Member.Name, arg);
            }
            else
            {
                throw new NotSupportedException($"Method op not supported '{node.Method.Name}' is not supported");
            }
            return node;
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    if (node.Left.NodeType != ExpressionType.MemberAccess && node.Right.NodeType != ExpressionType.Constant)
                    {
                        Visit(node.Right);
                        _resultStringBuilder.Append("(");
                        Visit(node.Left);
                        _resultStringBuilder.Append(")");
                        break;
                    }
                    else
                    {
                        Visit(node.Left);
                        _resultStringBuilder.Append("(");
                        Visit(node.Right);
                        _resultStringBuilder.Append(")");
                        break;
                    }            

                default:
                    throw new NotSupportedException($"Operation '{node.NodeType}' is not supported");
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _resultStringBuilder.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _resultStringBuilder.Append(node.Value);

            return node;
        }

        #endregion
    }
}
