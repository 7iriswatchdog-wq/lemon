using System;
using System.Linq;
using System.Linq.Expressions;

using System.Collections.Generic;

using AML.DTO.DTO.TransactionMonitor;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;

namespace AML.Web.Helper
{
    public class Builder
    {
        private static dynamic Evaluate<T>(T fact, string rule)
        {
            Console.WriteLine($"Eval: {rule}");

            if (string.IsNullOrEmpty(rule)) return null;

            var parameter = Expression.Parameter(typeof(T), "f");
            var lambdaExpression = DynamicExpressionParser.ParseLambda(new[] { parameter }, null, rule);

            var result = lambdaExpression.Compile().DynamicInvoke(fact);

            Console.WriteLine(result);

            return result;
        }

        private static bool Validate<T>(T fact, string rule)
        {
            return Evaluate(fact, rule);
        }

        public static (bool isRuleHit, TObjList returnList) CheckRule<TObjList>(TMSRulesDetailsDTO rule, TObjList objList, string compiledCheckerString = "", string compiledGetterString = "")
        {
            string ruleRuleType = rule.TMSRuleDetParamType;

            try
            {
                //(string compiledCheckerString, string compiledGetterString) = ("", "");

                if (ruleRuleType == "COMP") (compiledCheckerString, compiledGetterString) = CompileCompareRule(rule, objList);
                if (ruleRuleType == "MATCH") (compiledCheckerString, compiledGetterString) = CompileMatchRule(rule, objList);
                if (ruleRuleType == "TF") (compiledCheckerString, compiledGetterString) = CompileTimeframeRule(rule, objList);

                return (Validate(objList, compiledCheckerString), Evaluate(objList, compiledGetterString));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return (false, default(TObjList));
        }

        private static string buildFilter(bool sourceColumnIsString, string source, string aggregate, string operation, dynamic value)
        {
            if (value == null) { throw new Exception("'ruleValue' cannot be empty"); }
            if (operation == null) { throw new Exception("'ruleOperation' cannot be empty"); }
            if (aggregate == null) { throw new Exception("'ruleSourceAggregate' cannot be empty"); }

            string sourceColumn = source;

            var validAggregates = new List<string> { "None" };

            var validOperations = new List<string> { "GreaterThan", "LessThan", "EqualTo", "NotEqualTo", "GreaterThanEqual", "LessThanEqual" };
            var operationMapping = new List<string> { ">", "<", "==", "!=", ">=", "<=" };

            var validStringAggregations = new List<string> { "None" };
            var validStringOperations = new List<string> { "EqualTo", "NotEqualTo" };

            if (!validOperations.Contains(operation)) { throw new Exception($"'{operation}' is not a valid operation"); }
            if (!validAggregates.Contains(aggregate)) { throw new Exception($"'{aggregate}' is not a valid aggregate function"); }

            if (sourceColumnIsString)
            {
                if (!validStringOperations.Contains(operation)) { throw new Exception($"'{operation}' is not a valid operation for '{sourceColumn}'"); }
                if (!validStringAggregations.Contains(aggregate)) { throw new Exception($"'{aggregate}' is not a valid aggregation for '{sourceColumn}'"); }
            }

            var mappedOperation = operationMapping[validOperations.IndexOf(operation)];

            string selectString;

            if (value.GetType() == typeof(string) && value != "null") { value = $"\"{value}\""; }

            selectString = $".Where(item => item.{sourceColumn} {mappedOperation} {value})";

            return selectString;
        }

        private static (string, string) CompileTimeframeRule(TMSRulesDetailsDTO rule, dynamic fact)
        {
            string ruleOperation = rule.TMSRuleDetOperator;

            string ruleSource = rule.TMSRuleDetSource;
            string ruleSourceAggregate = rule.TMSRuleDetSourceAggr;

            string ruleSourceIfSource = rule.TMSRuleDetSourceIfSource;
            string ruleSourceIfSourceAggregate = rule.TMSRuleDetSourceIfSourceAggr;
            string ruleSourceIfOperator = rule.TMSRuleDetSourceIfOperator;
            string ruleSourceIfValue = rule.TMSRuleDetSourceIfValue;

            string ruleTimeFrameSource = rule.TMSRuleDetTimeFrameSource;
            int ruleTimeFrameValue = rule.TMSRuleDetTimeFrameVal;
            string ruleTimeFrameType = rule.TMSRuleDetTimeFrameType;

            string ruleCompareTo = rule.TMSRuleDetCompareTo;

            //dynamic ruleCompareValue = rule.TMSRuleDetCompareValue;
            dynamic ruleCompareValue = rule.TMSRuleDetDynamicCompareValue;

            bool ruleValueIsPercent = rule.TMSRuleDetCompareIsPercent;
            int rulePercentValue = rule.TMSRuleDetPercentValue;

            string ruleCompareField = rule.TMSRuleDetTarget;
            string ruleCompareFieldAggregate = rule.TMSRuleDetTargetAggr;

            string ruleCompareFieldIfSource = rule.TMSRuleDetCompareFieldIfSource;
            string ruleCompareFieldIfSourceAggregate = rule.TMSRuleDetCompareFieldIfSourceAggr;
            string ruleCompareFieldIfOperator = rule.TMSRuleDetCompareFieldIfOperator;
            string ruleCompareFieldIfValue = rule.TMSRuleDetCompareFieldIfValue;

            string ruleCompareTimeStartType = rule.TMSRuleDetCompareTimeStartType;
            int ruleCompareTimeStartValue = rule.TMSRuleDetCompareTimeStart;

            string ruleCompareTimeEndType = rule.TMSRuleDetCompareTimeEndType;
            int ruleCompareTimeEndValue = rule.TMSRuleDetCompareTimeEnd;

            string ruleForSame = rule.TMSRuleDetForSame;

            if (ruleOperation == null) { throw new Exception("'ruleOperation' cannot be empty"); }
            if (ruleTimeFrameType == null) { throw new Exception("'ruleTimeFrameType' cannot be empty"); }
            if (ruleSourceAggregate == null) { throw new Exception("'ruleSourceAggregate' cannot be empty"); }

            var validAggregates = new List<string> { "None", "Count", "CountDistinct", "Average", "Max", "Min", "Sum", "CountIf", "CountDistinctIf", "AverageIf", "MaxIf", "MinIf", "SumIf" };

            var sourceIsIfFilter = ruleSourceAggregate?[^2..] == "If";
            var targetIsIfFilter = ruleCompareFieldAggregate?[^2..] == "If";

            ruleSourceAggregate = sourceIsIfFilter ? ruleSourceAggregate[..^2] : ruleSourceAggregate;
            ruleCompareFieldAggregate = targetIsIfFilter ? ruleCompareFieldAggregate[..^2] : ruleCompareFieldAggregate;

            var validOperations = new List<string> { "GreaterThan", "LessThan", "EqualTo", "NotEqualTo", "GreaterThanEqual", "LessThanEqual" };
            var operationMapping = new List<string> { ">", "<", "==", "!=", ">=", "<=" };
            
            if (!validOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation"); }
            if (!validAggregates.Contains(ruleSourceAggregate)) { throw new Exception($"'{ruleSourceAggregate}' is not a valid aggregate function"); }

            string sourceColumn = ruleSource ?? "";

            string TimeframeSourceColumn = ruleTimeFrameSource ?? "";

            var validStringAggregates = new List<string> { "None", "Count", "CountDistinct" };
            var validStringOperations = new List<string> { "EqualTo", "NotEqualTo" };

            if (ruleSourceAggregate == "Count" || ruleSourceAggregate == "CountDistinct")
            {
                validStringOperations.AddRange(new List<string>() { "GreaterThan", "LessThan", "GreaterThanEqual", "LessThanEqual" });
            }

            if (ruleCompareFieldAggregate == "Count" || ruleCompareFieldAggregate == "CountDistinct")
            {
                validStringOperations.AddRange(new List<string>() { "GreaterThan", "LessThan", "GreaterThanEqual", "LessThanEqual" });
            }

            if (ruleCompareTo == "Value") { validStringAggregates.Add("None"); }

            var sourceColumnIsString = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(string);
            var sourceColumnIsDate = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(DateTime);

            if (sourceColumnIsDate) { sourceColumn = $"{sourceColumn}.Ticks"; }

            var sourceIfColumnIsString = ruleSourceIfSource == null ? "" : fact.GetType().GetProperty("Item").PropertyType.GetProperty(ruleSourceIfSource)?.PropertyType == typeof(string) ?? false;
            var targetIfColumnIsString = ruleCompareFieldIfSource == null ? "" : fact.GetType().GetProperty("Item").PropertyType.GetProperty(ruleCompareFieldIfSource)?.PropertyType == typeof(string) ?? false;

            if (sourceColumnIsString && !validStringAggregates.Contains(ruleSourceAggregate))
            {
                if (!validStringOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleSourceAggregate}' is not a valid aggregate for '{sourceColumn}'"); }
            }

            if (sourceColumnIsString && ruleSourceAggregate != "Count" && !validStringOperations.Contains(ruleSourceAggregate))
            {
                if (!validStringOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation for '{sourceColumn}'"); }
            }

            var operation = operationMapping[validOperations.IndexOf(ruleOperation)];

            long timeframeToTicks = 0;

            if (ruleTimeFrameType == "Seconds") { timeframeToTicks = DateTime.Now.Ticks - (10000000 * ruleTimeFrameValue); }
            if (ruleTimeFrameType == "Minutes") { timeframeToTicks = DateTime.Now.Ticks - (600000000 * ruleTimeFrameValue); }
            if (ruleTimeFrameType == "Hours") { timeframeToTicks = DateTime.Now.Ticks - (36000000000 * ruleTimeFrameValue); }
            if (ruleTimeFrameType == "Days") { timeframeToTicks = DateTime.Now.Ticks - (864000000000 * ruleTimeFrameValue); }

            string ruleString;
            string selectString;

            string sourceDistinct = "";
            string targetDistinct = "";

            string sourceIfFilter = sourceIsIfFilter
                ? buildFilter(sourceIfColumnIsString, ruleSourceIfSource, ruleSourceIfSourceAggregate, ruleSourceIfOperator, ruleSourceIfValue)
                : "";
            string targetIfFilter = targetIsIfFilter
                ? buildFilter(targetIfColumnIsString, ruleCompareFieldIfSource, ruleCompareFieldIfSourceAggregate, ruleCompareFieldIfOperator, ruleCompareFieldIfValue)
                : "";

            if (ruleSourceAggregate == "CountDistinct")
            {
                sourceDistinct = $".GroupBy(p => p.{sourceColumn})";
                ruleSourceAggregate = "Count";
            }

            if (ruleCompareTo != "Value")
            {
                string targetColumn = ruleCompareField ?? "";

                if (ruleCompareFieldAggregate == "CountDistinct")
                {
                    targetDistinct = $".GroupBy(p => p.{targetColumn})";
                    ruleCompareFieldAggregate = "Count";
                }

                var targetColumnIsString = fact.GetType().GetProperty(targetColumn)?.PropertyType == typeof(string);
                var targetColumnIsDate = fact.GetType().GetProperty(targetColumn)?.PropertyTyp == typeof(DateTime);

                if (targetColumnIsDate) { targetColumn = $"{targetColumn}.Ticks"; }

                string key = "";
                string groupBySelector = "";

                string timeFilter = $".Where(item => item.{TimeframeSourceColumn}.Ticks >= {timeframeToTicks})";

                if (!string.IsNullOrEmpty(ruleForSame))
                {
                    string[] andList = ruleForSame.Split(" && ").ToArray();

                    key = ".Key";

                    groupBySelector = $".GroupBy(item => new {{ {string.Join(", ", andList.Select(val => $"item.{val}"))} }})";
                }

                if (ruleCompareTo == "Field")
                {
                    if (ruleCompareFieldAggregate == "None")
                    {
                        if (ruleSourceAggregate == "None")
                        {
                            Console.WriteLine("TargetAggregate: 'None', SourceAggregate: 'None'");

                            ruleString = $"f" +
                            $"{timeFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $".Any(item => item{key}.{sourceColumn} / item{key}.{targetColumn} {operation} {rulePercentValue})"
                                : $".Any(item => item{key}.{sourceColumn} {operation} item{key}.{targetColumn})");

                            selectString = $"f" +
                            $"{timeFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $".Where(item => item{key}.{sourceColumn} / item{key}.{targetColumn} {operation} {rulePercentValue}).ToList()"
                                : $".Where(item => item{key}.{sourceColumn} {operation} item{key}.{targetColumn}).ToList()");

                            return (ruleString, selectString);
                        }

                        Console.WriteLine("TargetAggregate: 'None', SourceAggregate: 'Not None'");
                        ruleString = $"f" +
                            $"{timeFilter}" +
                            $".Any(obj => f" +
                            $"{timeFilter}" +
                            $"{sourceIfFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $"{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item{key}.{sourceColumn}") }) / obj.{targetColumn} {operation} {rulePercentValue})"
                                : $"{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item{key}.{sourceColumn}") }) {operation} obj.{targetColumn})");

                        selectString = $"f" +
                            $"{timeFilter}" +
                            $".Where(obj => f" +
                            $"{timeFilter}" +
                            $"{sourceIfFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $"{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item{key}.{sourceColumn}") }) / obj.{targetColumn} {operation} {rulePercentValue}).ToList()"
                                : $"{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item{key}.{sourceColumn}") }) {operation} obj.{targetColumn}).ToList()");

                        return (ruleString, selectString);
                    }

                    if (ruleSourceAggregate == "None")
                    {
                        Console.WriteLine("TargetAggregate: 'Not None', SourceAggregate: 'None'");
                        ruleString = $"f" +
                            $"{timeFilter}" +
                            $".Any(obj => f" +
                            $"{timeFilter}" +
                            $"{targetIfFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $"{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item{key}.{targetColumn}") }) / obj.{sourceColumn} {operation} {rulePercentValue})"
                                : $"{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item{key}.{targetColumn}") }) {operation} obj.{sourceColumn})");

                        selectString = $"f" +
                            $"{timeFilter}" +
                            $".Where(obj => f" +
                            $"{timeFilter}" +
                            $"{targetIfFilter}" +
                            $"{groupBySelector}" +
                            (ruleValueIsPercent
                                ? $"{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item{key}.{targetColumn}") }) / obj.{sourceColumn} {operation} {rulePercentValue}).ToList()"
                                : $"{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item{key}.{targetColumn}") }) {operation} obj.{sourceColumn}).ToList()");

                        return (ruleString, selectString);
                    }

                    Console.WriteLine("TargetAggregate: 'Not None', SourceAggregate: 'Not None'");

                    if (groupBySelector != "")
                    {
                        ruleString = $"f" +
                            $"{timeFilter}" +
                            $"{sourceIfFilter}" +
                            $"{groupBySelector}" +
                            $".Select(val => val{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item.{sourceColumn}") })).Max(val=>val)" +
                            (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                            $"f" +
                            $"{timeFilter}" +
                            $"{targetIfFilter}" +
                            $"{groupBySelector}" +
                            $".Select(val => val{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item.{targetColumn}") })).Max(val=>val) " +
                            (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "");

                        selectString = $"f{timeFilter}.ToList()";

                        return (ruleString, selectString);
                    }

                    ruleString = $"f" +
                        $"{timeFilter}" +
                        $"{sourceIfFilter}" +
                        $"{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item.{sourceColumn}") }) " +
                        (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                        $"f" +
                        $"{timeFilter}" +
                        $"{targetIfFilter}" +
                        $"{targetDistinct}.{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item.{targetColumn}") }) " +
                        (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "");

                    selectString = $"f{timeFilter}.ToList()";

                    return (ruleString, selectString);
                }

                if (ruleCompareTo == "Time")
                {
                    long timeframeStartTicks = 0;
                    long timeframeEndTicks = 0;

                    if (ruleCompareTimeStartType == "Seconds") { timeframeStartTicks = DateTime.Now.Ticks - (10000000 * ruleCompareTimeStartValue); }
                    if (ruleCompareTimeStartType == "Minutes") { timeframeStartTicks = DateTime.Now.Ticks - (600000000 * ruleCompareTimeStartValue); }
                    if (ruleCompareTimeStartType == "Hours") { timeframeStartTicks = DateTime.Now.Ticks - (36000000000 * ruleCompareTimeStartValue); }
                    if (ruleCompareTimeStartType == "Days") { timeframeStartTicks = DateTime.Now.Ticks - (864000000000 * ruleCompareTimeStartValue); }

                    if (ruleCompareTimeEndType == "Seconds") { timeframeEndTicks = DateTime.Now.Ticks - (10000000 * ruleCompareTimeEndValue); }
                    if (ruleCompareTimeEndType == "Minutes") { timeframeEndTicks = DateTime.Now.Ticks - (600000000 * ruleCompareTimeEndValue); }
                    if (ruleCompareTimeEndType == "Hours") { timeframeEndTicks = DateTime.Now.Ticks - (36000000000 * ruleCompareTimeEndValue); }
                    if (ruleCompareTimeEndType == "Days") { timeframeEndTicks = DateTime.Now.Ticks - (864000000000 * ruleCompareTimeEndValue); }

                    var sourceTimeSelector = $".Where(item => " +
                        $"{timeframeToTicks} <= item.{TimeframeSourceColumn}.Ticks)";

                    var timeSelector = $".Where(item => " +
                        $"{timeframeEndTicks} <= item.{TimeframeSourceColumn}.Ticks " +
                        $"&& " +
                        $"item.{TimeframeSourceColumn}.Ticks <= {timeframeStartTicks})";

                    if (ruleSourceAggregate == "None")
                    {
                        // TargetAggregate: "None", SourceAggregate: "None"
                        ruleString = "f" +
                            $"{sourceTimeSelector}" +
                            $"{groupBySelector}" +
                            $"{sourceDistinct}.Select(item => item.FirstOrDefault()) " +
                            $".Where(tranitem => {(ruleOperation == "EqualTo" ? "" : "!")}f.Where(tran => {string.Join(" && ", ruleForSame.Split(" && ").ToArray().Select(val => $"tran.{val} == tranitem.{val}"))})" +
                                $"{timeSelector}" +
                                $"{targetDistinct}.Select(item => item{(targetDistinct == "" ? "" : key)}.{targetColumn}).Contains(tranitem{(targetDistinct == "" ? "" : key)}.{targetColumn})).Count() > 0";

                        selectString = "f" +
                            $"{sourceTimeSelector}" +
                            $"{groupBySelector}" +
                            $"{sourceDistinct}.Select(item => item.FirstOrDefault()) " +
                            $".Where(tranitem => {(ruleOperation == "EqualTo" ? "" : "!")}f.Where(tran => {string.Join(" && ", ruleForSame.Split(" && ").ToArray().Select(val => $"tran.{val} == tranitem.{val}"))})" +
                                $"{timeSelector}" +
                                $"{targetDistinct}.Select(item => item{(targetDistinct == "" ? "" : key)}.{targetColumn}).Contains(tranitem{(targetDistinct == "" ? "" : key)}.{targetColumn})).ToList()";

                        return (ruleString, selectString);
                    }

                    // TargetAggregate: "Not none", SourceAggregate: "Not none"
                    ruleString = $"f" +
                    $"{sourceTimeSelector}" +
                    $"{sourceIfFilter}" +
                    $"{groupBySelector}" +
                    $"{sourceDistinct}.SelectMany(grp => grp).{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"item => item.{sourceColumn}") }) " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"f" +
                    $"{timeSelector}" +
                    $"{targetIfFilter}" +
                    $"{groupBySelector}" +
                    $"{targetDistinct}.SelectMany(grp => grp).{ruleCompareFieldAggregate}({ (targetColumnIsString || ruleCompareFieldAggregate == "Count" ? "" : $"item => item.{targetColumn}") })" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : ""); ;

                    selectString = $"f{sourceTimeSelector}.ToList()";

                    return (ruleString, selectString);
                }
            }

            if (ruleCompareTo == "Value")
            {
                if (ruleCompareValue is null) { throw new Exception("'CompareValue' cannot be null if 'CompareTo' is 'Value'"); }

                string key = "";
                string groupBySelector = "";

                string timeFilter = $".Where(item => item.{TimeframeSourceColumn}.Ticks >= {timeframeToTicks})";

                if (!string.IsNullOrEmpty(ruleForSame))
                {
                    string[] andList = ruleForSame.Split(" && ").ToArray();
                    string[] orList = ruleForSame.Split(" || ").ToArray();

                    key = ".Key";

                    groupBySelector = $".GroupBy(item => new {{ {string.Join(", ", andList.Select(val => $"item.{val}"))} }})";
                }

                dynamic ruleValue = ruleCompareValue;

                if (ruleValue.GetType() == typeof(string))
                {
                    var dateregex = @"^(\d)+\s+(days|minutes|seconds)$";
                    Regex r = new Regex(dateregex, RegexOptions.IgnoreCase);
                    if (r.IsMatch(ruleValue))
                    {
                        Match m = r.Match(ruleValue);
                        int date = int.Parse(m.Groups[0].Value);
                        string dateType = m.Groups[1].Value;

                        if (dateType.ToLower() == "seconds") { ruleValue = DateTime.Now.Ticks - (10000000 * date); }
                        if (dateType.ToLower() == "minutes") { ruleValue = DateTime.Now.Ticks - (600000000 * date); }
                        if (dateType.ToLower() == "hours") { ruleValue = DateTime.Now.Ticks - (36000000000 * date); }
                        if (dateType.ToLower() == "days") { ruleValue = DateTime.Now.Ticks - (864000000000 * date); }
                    }
                    else
                    {
                        ruleValue = $"\"{ruleCompareValue}\"";
                    }
                }

                if (ruleSourceAggregate == "None")
                {
                    ruleString = $"f" +
                        $"{timeFilter}" +
                        $"{groupBySelector}" +
                        (ruleValueIsPercent
                            ? $".Any(item => item{key}.{sourceColumn} / {ruleValue} {operation} {rulePercentValue})"
                            : $".Any(item => item{key}.{sourceColumn} {operation} {ruleCompareValue})");

                    selectString = $"f" +
                        $"{timeFilter}" +
                        $"{groupBySelector}" +
                        (ruleValueIsPercent
                            ? $".Where(item => item{key}.{sourceColumn} / {ruleValue} {operation} {rulePercentValue}).ToList()"
                            : $".Where(item => item{key}.{sourceColumn} {operation} {ruleCompareValue}).ToList()");

                    return (ruleString, selectString);
                }

                ruleString = $"f" +
                    $"{timeFilter}" +
                    $"{sourceIfFilter}" +
                    $"{groupBySelector}" +
                    (ruleValueIsPercent
                        ? $"{(groupBySelector == "" ? "" : ".Where(item => item")}{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) / {ruleValue} {operation} {rulePercentValue}{(groupBySelector == "" ? "" : ")")}"
                        : $"{(groupBySelector == "" ? "" : ".Where(item => item")}{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) {operation} {ruleValue}{(groupBySelector == "" ? "" : ")")}") +
                    (groupBySelector == "" ? "" : ".Count() > 0");

                selectString = $"f" +
                    $"{timeFilter}" +
                    $"{sourceIfFilter}" +
                    (groupBySelector == "" ? "" : $"{groupBySelector}" +
                    (ruleValueIsPercent
                        ? $"{(groupBySelector == "" ? "" : ".Where(item => item")}{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) / {ruleValue} {operation} {rulePercentValue}{(groupBySelector == "" ? "" : ")")}"
                        : $"{(groupBySelector == "" ? "" : ".Where(item => item")}{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) {operation} {ruleValue}{(groupBySelector == "" ? "" : ")")}") +
                    (groupBySelector == "" ? "" : ".SelectMany(gr => gr)")) +
                    ".ToList()";

                return (ruleString, selectString);
            }

            throw new Exception("Malformed rule");
        }

        private static (string, string) CompileMatchRule(TMSRulesDetailsDTO rule, dynamic fact)
        {
            string ruleOperation = rule.TMSRuleDetOperator;

            string ruleSource = rule.TMSRuleDetSource;
            string ruleSourceAggregate = rule.TMSRuleDetSourceAggr;

            string ruleTarget = rule.TMSRuleDetTarget;
            string ruleTargetAggregate = rule.TMSRuleDetTargetAggr;

            bool ruleValueIsPercent = rule.TMSRuleDetCompareIsPercent;
            int rulePercentValue = rule.TMSRuleDetPercentValue;

            string ruleSourceIfSource = rule.TMSRuleDetSourceIfSource;
            string ruleSourceIfSourceAggregate = rule.TMSRuleDetSourceIfSourceAggr;
            string ruleSourceIfOperator = rule.TMSRuleDetSourceIfOperator;
            string ruleSourceIfValue = rule.TMSRuleDetSourceIfValue;

            string ruleCompareFieldIfSource = rule.TMSRuleDetCompareFieldIfSource;
            string ruleCompareFieldIfSourceAggregate = rule.TMSRuleDetCompareFieldIfSourceAggr;
            string ruleCompareFieldIfOperator = rule.TMSRuleDetCompareFieldIfOperator;
            string ruleCompareFieldIfValue = rule.TMSRuleDetCompareFieldIfValue;

            if (ruleSource == null) { throw new Exception("'ruleSource' cannot be empty"); }
            if (ruleTarget == null) { throw new Exception("'ruleTarget' cannot be empty"); }
            if (ruleOperation == null) { throw new Exception("'ruleOperation' cannot be empty"); }
            if (ruleSourceAggregate == null) { throw new Exception("'ruleSourceAggregate' cannot be empty"); }
            if (ruleTargetAggregate == null) { throw new Exception("'ruleTargetAggregate' cannot be empty"); }

            string sourceColumn = ruleSource;
            string targetColumn = ruleTarget;

            var validAggregates = new List<string> { "None", "Count", "CountDistinct", "Average", "Max", "Min", "Sum", "CountIf", "CountDistinctIf", "AverageIf", "MaxIf", "MinIf", "SumIf" };

            var sourceIsIfFilter = ruleSourceAggregate?[^2..] == "If";
            var targetIsIfFilter = ruleTargetAggregate?[^2..] == "If";

            ruleSourceAggregate = sourceIsIfFilter ? ruleSourceAggregate[..^2] : ruleSourceAggregate;
            ruleTargetAggregate = targetIsIfFilter ? ruleTargetAggregate[..^2] : ruleTargetAggregate;

            var validOperations = new List<string> { "GreaterThan", "LessThan", "EqualTo", "NotEqualTo", "GreaterThanEqual", "LessThanEqual" };
            var operationMapping = new List<string> { ">", "<", "==", "!=", ">=", "<=" };

            var validStringAggregates = new List<string> { "None", "Count", "CountDistinct" };
            var validStringOperations = new List<string> { "EqualTo", "NotEqualTo" };

            if (ruleSourceAggregate == "Count" || ruleSourceAggregate == "CountDistinct")
            {
                validStringOperations.AddRange(new List<string>() { "GreaterThan", "LessThan", "GreaterThanEqual", "LessThanEqual" });
            }

            if (ruleTargetAggregate == "Count" || ruleTargetAggregate == "CountDistinct")
            {
                validOperations.AddRange(new List<string>() { "GreaterThan", "LessThan", "GreaterThanEqual", "LessThanEqual" });
            }

            var sourceColumnIsString = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(string);
            var targetColumnIsString = fact.GetType().GetProperty("Item").PropertyType.GetProperty(targetColumn)?.PropertyType == typeof(string);

            var sourceColumnIsDate = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(DateTime);
            var targetColumnIsDate = fact.GetType().GetProperty(targetColumn)?.PropertyTyp == typeof(DateTime);

            if (sourceColumnIsDate) { sourceColumn = $"{sourceColumn}.Ticks"; }
            if (targetColumnIsDate) { targetColumn = $"{targetColumn}.Ticks"; }

            var sourceIfColumnIsString = ruleSourceIfSource == null ? "" : fact.GetType().GetProperty("Item").PropertyType.GetProperty(ruleSourceIfSource)?.PropertyType == typeof(string) ?? false;
            var targetIfColumnIsString = ruleCompareFieldIfSource == null ? "" : fact.GetType().GetProperty("Item").PropertyType.GetProperty(ruleCompareFieldIfSource)?.PropertyType == typeof(string) ?? false;

            if (sourceColumnIsString && targetColumnIsString) { validStringAggregates.Add("None"); }

            if ((sourceColumnIsString && validStringAggregates.Contains(ruleSourceAggregate)) || (targetColumnIsString && validStringAggregates.Contains(ruleTargetAggregate)))
            {
                if (!validStringOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation for '{sourceColumn}'"); }
            }

            if (!validAggregates.Contains(ruleTargetAggregate)) { throw new Exception($"'{ruleTargetAggregate}' is not a valid aggregate function"); }
            if (!validOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation"); }

            if (sourceColumnIsString)
            {
                if (!validStringAggregates.Contains(ruleSourceAggregate)) { throw new Exception($"'{ruleSourceAggregate}' is not a valid aggregate for '{sourceColumn}'"); }
            }

            if (targetColumnIsString)
            {
                if (!validStringAggregates.Contains(ruleTargetAggregate)) { throw new Exception($"'{ruleTargetAggregate}' is not a valid aggregate for '{targetColumn}'"); }
            }

            var operation = operationMapping[validOperations.IndexOf(ruleOperation)];

            string ruleString;
            string selectString;

            string sourceDistinct = "";
            string targetDistinct = "";

            string sourceIfFilter = sourceIsIfFilter
                ? buildFilter(sourceIfColumnIsString, ruleSourceIfSource, ruleSourceIfSourceAggregate, ruleSourceIfOperator, ruleSourceIfValue)
                : "";
            string targetIfFilter = targetIsIfFilter
                ? buildFilter(targetIfColumnIsString, ruleCompareFieldIfSource, ruleCompareFieldIfSourceAggregate, ruleCompareFieldIfOperator, ruleCompareFieldIfValue)
                : "";

            if (ruleSourceAggregate == "CountDistinct")
            {
                sourceDistinct = $".GroupBy(p => p.{sourceColumn})";
                ruleSourceAggregate = "Count";
            }

            if (ruleTargetAggregate == "CountDistinct")
            {
                targetDistinct = $".GroupBy(p => p.{targetColumn})";
                ruleTargetAggregate = "Count";
            }

            if (ruleTargetAggregate == "None")
            {
                if (ruleSourceAggregate == "None")
                {
                    // TargetAggregate: "None", SourceAggregate: "None"
                    ruleString = $"f.Any(item => item.{sourceColumn} " +
                        (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                        $"item.{targetColumn}" +
                        (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                        ")";
                    selectString = $"f.Where(item => item.{sourceColumn} " +
                        (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                        $"item.{targetColumn}" +
                        (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                        $").ToList()";

                    return (ruleString, selectString);
                }

                // TargetAggregate: "None", SourceAggregate: "Not none"
                ruleString = $"f{sourceIfFilter}.Any(trn => f{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"trn.{targetColumn}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $")";
                selectString = $"f{sourceIfFilter}.Where(trn => f{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"trn.{targetColumn}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $").ToList()";

                return (ruleString, selectString);
            }

            if (ruleSourceAggregate == "None")
            {
                // TargetAggregate: "Not none", SourceAggregate: "None"
                ruleString = $"f{targetIfFilter}.Any(trn => f{targetDistinct}.{ruleTargetAggregate}({ (targetColumnIsString || ruleTargetAggregate == "Count" ? "" : $"val => val.{targetColumn}") }) " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"trn.{sourceColumn}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $")";
                selectString = $"f{targetIfFilter}.Where(trn => f{targetDistinct}.{ruleTargetAggregate}({ (targetColumnIsString || ruleTargetAggregate == "Count" ? "" : $"val => val.{targetColumn}") }) " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"trn.{sourceColumn}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $").ToList()";

                return (ruleString, selectString);
            }

            // TargetAggregate: "Not none", SourceAggregate: "Not none"
            ruleString = $"f{sourceDistinct}{sourceIfFilter}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) " +
                (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                $"f{targetDistinct}{targetIfFilter}.{ruleTargetAggregate}({ (targetColumnIsString || ruleTargetAggregate == "Count" ? "" : $"val => val.{targetColumn}") })" +
                (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "");
            selectString = "f";

            return (ruleString, selectString);
        }

        private static (string, string) CompileCompareRule(TMSRulesDetailsDTO rule, dynamic fact)
        {
            string ruleOperation = rule.TMSRuleDetOperator;

            string ruleSource = rule.TMSRuleDetSource;
            string ruleSourceAggregate = rule.TMSRuleDetSourceAggr;

            dynamic ruleValue = rule.TMSRuleDetValue;

            string ruleSourceIfSource = rule.TMSRuleDetSourceIfSource;
            string ruleSourceIfSourceAggregate = rule.TMSRuleDetSourceIfSourceAggr;
            string ruleSourceIfOperator = rule.TMSRuleDetSourceIfOperator;
            string ruleSourceIfValue = rule.TMSRuleDetSourceIfValue;

            bool ruleValueIsPercent = rule.TMSRuleDetCompareIsPercent;
            int rulePercentValue = rule.TMSRuleDetPercentValue;

            if (ruleValue == null) { throw new Exception("'ruleValue' cannot be empty"); }
            if (ruleOperation == null) { throw new Exception("'ruleOperation' cannot be empty"); }
            if (ruleSourceAggregate == null) { throw new Exception("'ruleSourceAggregate' cannot be empty"); }

            string sourceColumn = ruleSource;

            var validAggregates = new List<string> { "None", "Count", "CountDistinct", "Average", "Max", "Min", "Sum", "CountIf", "CountDistinctIf", "AverageIf", "MaxIf", "MinIf", "SumIf" };

            var validOperations = new List<string> { "GreaterThan", "LessThan", "EqualTo", "NotEqualTo", "GreaterThanEqual", "LessThanEqual" };
            var operationMapping = new List<string> { ">", "<", "==", "!=", ">=", "<=" };

            var validStringAggregations = new List<string> { "None", "Count", "CountDistinct" };
            var validStringOperations = new List<string> { "EqualTo", "NotEqualTo" };

            var sourceIsIfFilter = ruleSourceAggregate?[^2..] == "If";

            ruleSourceAggregate = sourceIsIfFilter ? ruleSourceAggregate[..^2] : ruleSourceAggregate;

            if (ruleSourceAggregate == "Count" || ruleSourceAggregate == "CountDistinct")
            {
                validStringOperations.AddRange(new List<string>() { "GreaterThan", "LessThan", "GreaterThanEqual", "LessThanEqual" });
            }

            var sourceColumnIsString = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(string);
            //Console.WriteLine(fact.GetType());
            //Console.WriteLine(fact.GetType().GetProperty("Item"));
            //Console.WriteLine(fact.GetType().GetProperty("Item").PropertyType);
            //Console.WriteLine(fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn));
            //Console.WriteLine(fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType);

            var sourceIfColumnIsString = ruleSourceIfSource == null ? "" : fact.GetType().GetProperty("Item").PropertyType.GetProperty(ruleSourceIfSource)?.PropertyType == typeof(string) ?? false;

            var sourceColumnIsDate = fact.GetType().GetProperty("Item").PropertyType.GetProperty(sourceColumn)?.PropertyType == typeof(DateTime);

            if (sourceColumnIsDate) { sourceColumn = $"{sourceColumn}.Ticks"; }

            if (!validOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation"); }
            if (!validAggregates.Contains(ruleSourceAggregate)) { throw new Exception($"'{ruleSourceAggregate}' is not a valid aggregate function"); }

            if (sourceColumnIsString)
            {
                if (!validStringOperations.Contains(ruleOperation)) { throw new Exception($"'{ruleOperation}' is not a valid operation for '{sourceColumn}'"); }
                if (!validStringAggregations.Contains(ruleSourceAggregate)) { throw new Exception($"'{ruleSourceAggregate}' is not a valid aggregation for '{sourceColumn}'"); }
            }

            var operation = operationMapping[validOperations.IndexOf(ruleOperation)];

            string ruleString;
            string selectString;

            string sourceDistinct = "";

            string sourceIfFilter = sourceIsIfFilter
                ? buildFilter(sourceIfColumnIsString, ruleSourceIfSource, ruleSourceIfSourceAggregate, ruleSourceIfOperator, ruleSourceIfValue)
                : "";

            if (ruleSourceAggregate == "CountDistinct")
            {
                sourceDistinct = $".GroupBy(p => p.{sourceColumn})";
                ruleSourceAggregate = "Count";
            }

            if (ruleValue.GetType() == typeof(string)) {
                var dateregex = @"^(\d+)\s+(days|minutes|seconds)$";
                Regex r = new Regex(dateregex, RegexOptions.IgnoreCase);
                if (r.IsMatch(ruleValue))
                {
                    Match m = r.Match(ruleValue);
                    int date = int.Parse(m.Groups[1].Value);
                    string dateType = m.Groups[2].Value;

                    if (dateType.ToLower() == "seconds") { ruleValue = DateTime.Now.Ticks - (10000000 * date); }
                    if (dateType.ToLower() == "minutes") { ruleValue = DateTime.Now.Ticks - (600000000 * date); }
                    if (dateType.ToLower() == "hours") { ruleValue = DateTime.Now.Ticks - (36000000000 * date); }
                    if (dateType.ToLower() == "days") { ruleValue = DateTime.Now.Ticks - (864000000000 * date); }
                }
                else
                {
                    ruleValue = $"\"{ruleValue}\"";
                }
            }

            if (ruleSourceAggregate == "None")
            {
                ruleString = $"f.Any(item => item.{sourceColumn} " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"{ruleValue}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $")";
                selectString = $"f.Where(item => item.{sourceColumn} " +
                    (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                    $"{ruleValue}" +
                    (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                    $").ToList()";

                return (ruleString, selectString);
            }

            ruleString = $"f{sourceIfFilter}.Any(item => f{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) " +
                (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                $"{ruleValue}" +
                (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                $")";
            selectString = $"f{sourceIfFilter}.Where(item => f{sourceDistinct}.{ruleSourceAggregate}({ (sourceColumnIsString || ruleSourceAggregate == "Count" ? "" : $"val => val.{sourceColumn}") }) " +
                (ruleValueIsPercent ? " * 100 / " : $" {operation} ") +
                $"{ruleValue}" +
                (ruleValueIsPercent ? $"{operation} {rulePercentValue}" : "") +
                $").ToList()";

            return (ruleString, selectString);
        }
    }
}
