using System.Collections.Generic;
using System;

public class GOAP_Action
{
    public List<Tuple<GOAP_World.Variables, object>> preconditions = new List<Tuple<GOAP_World.Variables, object>>();
    public List<Tuple<GOAP_World.Variables, object>> effects = new List<Tuple<GOAP_World.Variables, object>>();

    Player.PlayerActions _myAction;

    public string name;

    public float cost;

    public GOAP_Action(string n, Player.PlayerActions a)
    {
        name = n;
        _myAction = a;
    }

    public Player.PlayerActions GetAction() { return _myAction; }

    public GOAP_Action PRE(GOAP_World.Variables v, object value)
    {
        preconditions.Add(new Tuple<GOAP_World.Variables, object>(v, value));
        return this;
    }

    public GOAP_Action EFFECT(GOAP_World.Variables v, object value)
    {
        effects.Add(new Tuple<GOAP_World.Variables, object>(v, value));
        return this;
    }

    public GOAP_Action COST(float c)
    {
        cost = c;
        return this;
    }
}
