using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("✫ Blackboard")]
    public class CheckString : ConditionTask
    {

        [BlackboardOnly]
        public BBParameter<string> valueA;
        public BBParameter<string> valueB;

        protected override string info {
            get { return valueA + " == " + valueB; }
        }

        protected override bool OnCheck() {
            return valueA.value == valueB.value;
        }
    }
        
    [Category("✫ Blackboard")]
    public class CheckStringEmpty : ConditionTask
    {

        [BlackboardOnly]
        public BBParameter<string> value;

        protected override string info {
            get { return value + " == Empty"; }
        }

        protected override bool OnCheck() {
            return value.value == string.Empty;
        }
    }
    
}