using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Conditions
{
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