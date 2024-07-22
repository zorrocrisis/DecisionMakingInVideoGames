using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.IAJ.Unity.DecisionMaking.BehaviorTree
{
    public class Selector : CompositeTask
    {
   
        public Selector(List<Task> tasks) : base(tasks)
        {
        }

        // A Selector will return immediately with a success status code when one of its children
        // runs successfully.As long as its children are failing, it will keep on trying. If it runs out of
        // children completely, it will return a failure status code
        public override Result Run()
        {
            // TODO implement
            if (children.Count > this.currentChild)
            {
                Result result = children[currentChild].Run();

                if (result == Result.Running)
                    return Result.Running;

                //If current child fails, go to the next
                else if (result == Result.Failure)
                {
                    currentChild++;
                    if (children.Count > this.currentChild)
                        return Result.Running;
                    else
                    {
                        currentChild = 0;
                        return Result.Failure;
                    }
                }
                //If the current child succeeds, we can stop
                else
                {
                    currentChild = 0;
                    return Result.Success;
                }
            }
            
            return Result.Failure;

        }
    }
}
