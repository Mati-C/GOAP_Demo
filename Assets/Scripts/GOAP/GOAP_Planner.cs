using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class GOAP_Planner : MonoBehaviour
{
    [Header("Nodes Cost:")]

    public float pickBadHaxCost;
    public float pickHaxCost;
    public float pickMedCost;
    public float sleepCost;

    [Header("Stats Cost:")]

    public string reputationGoal;

    public int woodGoal;
    public int plusWood;

    public float tiredAmount;
    public float plusTired;

    public List<Player.PlayerActions> actionPath = new List<Player.PlayerActions>();

    public List<List<GOAP_Action>> possiblePaths = new List<List<GOAP_Action>>();

    public GOAP_World world;

    public Status status;

    public int watchDog = 100;

    public Player player;

    List<int> randomIndexes = new List<int>();

    IEnumerator GetBestPath()
    {
        yield return new WaitForSeconds(1);
        var paths = possiblePaths.Where(x => x.Count > 0).OrderBy(x =>
        {
            float cost = 0;

            foreach (var item in x) cost += item.cost;

            return cost;

        }).Where(x =>
        {
            foreach (var item in x)
                if (item.GetAction() == Player.PlayerActions.Build_House) return true;

            return false;

        }).ToList();

        if (paths.Any())
        {
            actionPath.AddRange(paths.First().Select(x => x.GetAction()));

            foreach (var item in actionPath)
                Debug.Log(item);

            StartCoroutine(DelayActivePlayer());
        }
        else Debug.Log("NO POSSIBLE PATH");
    }

    private void Awake()
    {
        player = FindObjectOfType<Player>();

        world = FindObjectOfType<GOAP_World>();

        status = world.GetWorldStatus();

        for (int i = 0; i < 50; i++)
            possiblePaths.Add(new List<GOAP_Action>());
    }

    public void Play_GOAP_Calculator()
    {
        StartCoroutine(CalculatePath(PossibleActions(), status, GOAP_World.Variables.House_Built, true, 0));
    }

    public void Play_GetBestPath()
    {
        StartCoroutine(GetBestPath());
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    IEnumerator DelayActivePlayer()
    {
        yield return new WaitForSeconds(2);
        player.AddStateList(actionPath);
    }

    public IEnumerator CalculatePath(List<GOAP_Action> listOfActions, Status ws, GOAP_World.Variables goal, object goalValue, int index)
    {
        if (watchDog > 0)
        {
            var actions = new List<GOAP_Action>();

            actions.AddRange(listOfActions.Where(x => MeetsPreconditions(x.preconditions, ws)));

            if (actions.Count > 0 && !world.GoalDone(goal, goalValue, ws))
            {
                var newIndexes = new List<int>();

                for (int i = 0; i < actions.Count; i++)
                {
                    if (index + i > index)
                    {
                        int r = UnityEngine.Random.Range(0, 50);

                        while (randomIndexes.Contains(r))
                        {
                            r = UnityEngine.Random.Range(0, 50);
                            yield return new WaitForEndOfFrame();
                        }

                        var p = possiblePaths[index].Take(possiblePaths[index].Count - 1);

                        possiblePaths[r].AddRange(p);
                        possiblePaths[r].Add(actions[i]);

                        randomIndexes.Add(r);
                        newIndexes.Add(r);
                    }
                    else
                    {
                        newIndexes.Add(index);
                        possiblePaths[index].Add(actions[i]);
                    }
                }

                for (int i = 0; i < actions.Count; i++)
                {
                    var auxWorld = world.GetWorldStatus();

                    auxWorld.wood = ws.wood;
                    auxWorld.tiredAmount = ws.tiredAmount;
                    auxWorld.hasAxe = ws.hasAxe;
                    auxWorld.pain = ws.pain;
                    auxWorld.houseBuilt = ws.houseBuilt;
                    auxWorld.reputation = ws.reputation;

                    auxWorld = ActivateEffects(actions[i].effects, auxWorld);

                    yield return new WaitForEndOfFrame();

                    StartCoroutine(CalculatePath(listOfActions, auxWorld, goal, goalValue, newIndexes[i]));
                }
            }
            watchDog--;
        }
    }

    public bool MeetsPreconditions(List<Tuple<GOAP_World.Variables, object>> pre, Status ws)
    {
        foreach (var p in pre)
            if (!world.CompareVars(p.Item1, p.Item2, ws)) return false;

        return true;
    }

    public Status ActivateEffects(List<Tuple<GOAP_World.Variables, object>> effects, Status ws)
    {
        foreach (var e in effects)
            ws = world.ApplyEffects(ws, e.Item1, e.Item2);

        return ws;
    }

    public List<GOAP_Action> PossibleActions()
    {
        return new List<GOAP_Action>()
        {
                new GOAP_Action("Pickup Axe", Player.PlayerActions.Get_Axe)
                .COST(pickHaxCost)

                .PRE(GOAP_World.Variables.Not_Enough_Wood, woodGoal)
                .PRE(GOAP_World.Variables.Axe, false)

                .EFFECT(GOAP_World.Variables.Axe, true)


                ,new GOAP_Action("Pickup Bad Axe", Player.PlayerActions.Get_Bad_Axe)
                .COST(pickBadHaxCost)

                .PRE(GOAP_World.Variables.Not_Enough_Wood, woodGoal)
                .PRE(GOAP_World.Variables.Axe, false)

                .EFFECT(GOAP_World.Variables.Axe, true)
                .EFFECT(GOAP_World.Variables.Pain, true)

                ,new GOAP_Action("Pickup Medicine", Player.PlayerActions.Get_Medicine)
                .COST(pickMedCost)

                .PRE(GOAP_World.Variables.Pain, true)

                .EFFECT(GOAP_World.Variables.Pain, false)

                , new GOAP_Action("PickUp Wood", Player.PlayerActions.Get_Wood)
                .COST(3)

                .PRE(GOAP_World.Variables.Not_Enough_Wood, woodGoal)
                .PRE(GOAP_World.Variables.Axe, true)
                .PRE(GOAP_World.Variables.Pain, false)
                .PRE(GOAP_World.Variables.Highly_Tired, tiredAmount)

                .EFFECT(GOAP_World.Variables.Highly_Tired, plusTired)
                .EFFECT(GOAP_World.Variables.Not_Enough_Wood, plusWood)

                , new GOAP_Action("Sell Wood", Player.PlayerActions.Sell_Wood)
                .COST(2)

                .PRE(GOAP_World.Variables.Enough_Wood, 20)
                .PRE(GOAP_World.Variables.Highly_Tired, tiredAmount)
                .PRE(GOAP_World.Variables.Same_Reputation, "Neutral")

                .EFFECT(GOAP_World.Variables.Sell_Wood, 20)
                .EFFECT(GOAP_World.Variables.Same_Reputation, "Honorable")


                , new GOAP_Action("Steal Wood", Player.PlayerActions.Steal_Wood)
                .COST(2)

                .PRE(GOAP_World.Variables.Same_Wood, 0)
                .PRE(GOAP_World.Variables.Highly_Tired, tiredAmount)
                .PRE(GOAP_World.Variables.Same_Reputation, "Neutral")

                .EFFECT(GOAP_World.Variables.Steal_Wood, 40)
                .EFFECT(GOAP_World.Variables.Same_Reputation, "Villian")


                ,new GOAP_Action("Sleep", Player.PlayerActions.Sleep)
                .COST(sleepCost)

                .PRE(GOAP_World.Variables.Slightly_Tired, tiredAmount)

                .EFFECT(GOAP_World.Variables.Slightly_Tired, 0f)


                 ,new GOAP_Action("Build House", Player.PlayerActions.Build_House)
                .COST(1)

                .PRE(GOAP_World.Variables.Enough_Wood, woodGoal)
                .PRE(GOAP_World.Variables.House_Built, false)
                .PRE(GOAP_World.Variables.Same_Reputation, reputationGoal)

                .EFFECT(GOAP_World.Variables.House_Built, true)
        };
    }
}
