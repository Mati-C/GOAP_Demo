using UnityEngine;
using System;

public class GOAP_World : MonoBehaviour
{
    public enum Variables
    {
        Not_Enough_Wood,
        Enough_Wood,
        Same_Wood,
        Axe,
        Bad_Axe,
        Pain,
        Highly_Tired,
        Slightly_Tired,
        Diferent_Reputation,
        Same_Reputation,
        Sell_Wood,
        Steal_Wood,
        House_Built
    }

    public Func<Variables, object, Status, bool> CompareVars = (Variables v, object value, Status w) =>
    {
        switch (v)
        {
            case Variables.Not_Enough_Wood:
                return w.wood < (int)value;

            case Variables.Enough_Wood:
                return w.wood >= (int)value;

            case Variables.Same_Wood:
                return w.wood == (int)value;

            case Variables.Axe:
                return w.hasAxe == (bool)value;

            case Variables.Pain:
                return w.pain == (bool)value;

            case Variables.Highly_Tired:
                return w.tiredAmount < (float)value;

            case Variables.Slightly_Tired:
                return w.tiredAmount >= (float)value;

            case Variables.Diferent_Reputation:
                return w.reputation != (string)value;

            case Variables.Same_Reputation:
                return w.reputation == (string)value;

            case Variables.House_Built:
                return w.houseBuilt == (bool)value;

            default:
                return false;
        }
    };

    public Func<Status, Variables, object, Status> ApplyEffects =
        (Status w, Variables v, object value) =>
        {
            switch (v)
            {
                case Variables.Not_Enough_Wood:
                    w.wood += (int)value;
                    break;

                case Variables.Axe:
                    w.hasAxe = (bool)value;
                    break;

                case Variables.Pain:
                    w.pain = (bool)value;
                    break;

                case Variables.Highly_Tired:
                    w.tiredAmount += (float)value;
                    break;

                case Variables.Slightly_Tired:
                    w.tiredAmount = (float)value;
                    break;

                case Variables.Same_Reputation:
                    w.reputation = (string)value;
                    break;

                case Variables.Sell_Wood:
                    w.wood -= (int)value;
                    break;

                case Variables.Steal_Wood:
                    w.wood += (int)value;
                    break;

                case Variables.House_Built:
                    w.houseBuilt = (bool)value;
                    break;

                default:
                    break;
            }
            return w;
        };

    public Func<Variables, object, Status, bool> GoalDone = (Variables v, object value, Status w) =>
    {
        if (v == Variables.House_Built)
            return w.houseBuilt == (bool)value;
        else
            return false;
    };

    public Func<Status> GetWorldStatus = () =>
    {
        var w = new Status();

        w.wood = 0;
        w.tiredAmount = 0;
        w.hasAxe = false;
        w.pain = false;
        w.houseBuilt = false;
        w.reputation = "Neutral";

        return w;
    };
}
