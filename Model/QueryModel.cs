using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DynamicQueryModel.Model
{
    public class QueryModel
    { 
        public OutsideLogic OutsideLogic { get; set; }

        public List<QueryData> QueryData { get; set; }
        [JsonIgnore]
        public int Index { get; set; }

    }

    public class QueryData
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string LeftValue { get; set; }
        public string RightValue { get; set; }
        public InsideLogic InsideLogic { get; set; }
        public InsideAction FilterAction { get; set; }
    }

    public enum InsideLogic
    {
        [Description("且")]
        And,
        [Description("或")]
        Or,
    }

    public enum OutsideLogic
    {
        [Description("且")]
        And,
        [Description("或")]
        Or,
    }
    public enum InsideAction
    {
        [Description("等于")]
        Equals,                 //==
        [Description("不等于")]
        NotEquals,              //!=
        [Description("包含")]
        Contains,               //like 
        [Description("不包含")]
        NotContains,
        [Description("小于")]
        LessThan,               //<
        [Description("不小于")]
        LessAndEqualThan,       //<=
        [Description("大于")]
        GreaterThan,            //>
        [Description("不大于")]
        GreaterAndEqualThan,    //>=
        [Description("介于")]
        Between,                //<= and >=
    }
}
