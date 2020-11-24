using System.Collections.Generic;
using System.Linq;

public class StateMachine
{
    public State currentState;
    List<State> allStates = new List<State>();

    public void NextStep()
    {
        var a = new List<State>();
        a.AddRange(allStates.Skip(1));
        allStates.Clear();
        allStates.AddRange(a);
        ChangeState(allStates.First());
    }

    public StateMachine(List<State> list)
    {
        allStates.AddRange(list);
        currentState = allStates.First();
        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null) currentState.Update();
    }

    public void ChangeState(State state)
    {
        currentState.Exit();
        currentState = state;
        currentState.Enter();
    }
}
