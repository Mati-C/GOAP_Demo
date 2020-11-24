using System;

public class State
{
    public event Action OnUpdate = delegate { };
    public event Action OnFixedUpdate = delegate { };
    public event Action OnLateUpdate = delegate { };
    public event Action OnEnter = delegate { };
    public event Action OnExit = delegate { };

    public State() { }

    public void Update() { OnUpdate(); }

    public void Enter() { OnEnter(); }

    public void Exit() { OnExit(); }
}
