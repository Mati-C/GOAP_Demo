using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    int currentWood;
    float tiredAmount;
    StateMachine myFSM;
    Vector3 startPosition;

    Astar astar;
    Grid grid;
    int currentIndex = 0;

    [HideInInspector]
    public List<Node> astarPath = new List<Node>();
    List<Node> nodes = new List<Node>();
    List<Item> items = new List<Item>();

    Rigidbody rb;
    public float speed;
    Item itemTarget;
    bool onDelay;
    bool isRunning;

    [Header("Objects:")]
    public GameObject axe;
    public GameObject playerAxe;
    public GameObject badAxe;
    public GameObject playerBadAxe;
    public GameObject house;
    public GameObject tent;

    [Header("UI:")]
    public Text woodText;
    public Text stateText;
    public Text reputationText;

    Action OnCollisionAction = delegate { };
    Action OnTriggerAction = delegate { };

    State getWood = new State();
    State sleep = new State();
    State buildHouse = new State();
    State getAxe = new State();
    State getBadAxe = new State();
    State getMedicine = new State();
    State sellWood = new State();
    State stealWood = new State();
    State none = new State();

    public enum PlayerActions
    {
        Get_Wood,
        Sleep,
        Build_House,
        Get_Axe,
        Get_Bad_Axe,
        Get_Medicine,
        Sell_Wood,
        Steal_Wood,
        None
    }

    void Awake()
    {
        items.AddRange(FindObjectsOfType<Item>());
        startPosition = transform.position;
        stateText.text = "State: Normal";
        reputationText.text = "Reputation: Neutral";
        astar = GetComponent<Astar>();

        getWood.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Tree));
            OnCollisionAction += GetWood;
        };

        getWood.OnUpdate += () =>
        {
            WalkToTarget();
        };

        getWood.OnExit += () =>
        {
            OnCollisionAction -= GetWood;
        };



        stealWood.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Shop));
            OnTriggerAction += StealWood;
        };

        stealWood.OnUpdate += () =>
        {
            WalkToTarget();
        };

        stealWood.OnExit += () =>
        {
            OnTriggerAction -= StealWood;
        };



        sellWood.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Shop));
            OnTriggerAction += SellWood;
        };

        sellWood.OnUpdate += () =>
        {
            WalkToTarget();
        };

        sellWood.OnExit += () =>
        {
            OnTriggerAction -= SellWood;
        };



        sleep.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Bed));
            OnCollisionAction += GoToSleep;
            OnTriggerAction += GoToSleep;
            stateText.text = "State: Tired";
        };

        sleep.OnUpdate += () =>
        {
            WalkToTarget();
        };

        sleep.OnExit += () =>
        {
            OnCollisionAction -= GoToSleep;
            OnTriggerAction -= GoToSleep;
            stateText.text = "State: Normal";
        };



        buildHouse.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.House));
            OnCollisionAction += BuildHouse;
            OnTriggerAction += BuildHouse;
        };

        buildHouse.OnUpdate += () =>
        {
            WalkToTarget();
        };

        buildHouse.OnExit += () =>
        {
            OnCollisionAction -= BuildHouse;
            OnTriggerAction -= BuildHouse;
        };



        getAxe.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Hax));
            OnCollisionAction += GetAxe;
        };

        getAxe.OnUpdate += () =>
        {
            WalkToTarget();
        };

        getAxe.OnExit += () =>
        {
            OnCollisionAction -= GetAxe;
        };



        getBadAxe.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.BadHax));
            OnCollisionAction += GetBadAxe;
        };

        getBadAxe.OnUpdate += () =>
        {
            WalkToTarget();
        };

        getBadAxe.OnExit += () =>
        {
            OnCollisionAction -= GetBadAxe;
        };



        getMedicine.OnEnter += () =>
        {
            MoveTo(FindNearObject(ItemType.Medicine));
            OnCollisionAction += GetWood;
        };

        getMedicine.OnUpdate += () =>
        {
            WalkToTarget();
        };

        getMedicine.OnExit += () =>
        {
            OnCollisionAction -= GetWood;
        };
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grid = FindObjectOfType<Grid>();
        nodes.AddRange(grid.listNodes.Where(x => x.walkable));
    }

    void Update()
    {
        if (isRunning) myFSM.Update();
        woodText.text = "Current Wood: " + currentWood;
    }

    public Vector3 FindNearObject(ItemType item)
    {
        var i = FindObjectsOfType<Item>().Where(x => x.type == item).OrderBy(x =>
        {
            var d = Vector3.Distance(transform.position, x.transform.position);

            return d;

        });

        itemTarget = i.First();

        return i.First().transform.position;
    }

    public void MoveTo(Vector3 endPos)
    {
        currentIndex = 0;
        astarPath.Clear();

        astar.StartCoroutine(astar.FindPath(FindNearNode(transform.position), FindNearNode(endPos)));
    }

    private Vector3 FindNearNode(Vector3 pos)
    {
        var n = nodes.OrderBy(x =>
        {
            var distance = Vector3.Distance(x.position, pos);
            return distance;
        });

        return n.First().position;
    }

    public void WalkToTarget()
    {
        if (currentIndex < astarPath.Count && !onDelay && isRunning)
        {
            float d = Vector3.Distance(astarPath[currentIndex].position, transform.position);

            var _dir = Vector3.zero;

            if (d >= 1)
            {
                Quaternion targetRotation;
                _dir = (astarPath[currentIndex].position - transform.position).normalized;
                _dir.y = 0;
                targetRotation = Quaternion.LookRotation(_dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 7 * Time.deltaTime);
                rb.MovePosition(rb.position + _dir * speed * Time.deltaTime);
            }
            else
                currentIndex++;
        }
    }

    public void AddStateList(List<PlayerActions> l)
    {
        var states = l.Select(x =>
        {
            if (x == PlayerActions.Get_Wood) return getWood;

            else if (x == PlayerActions.Sleep) return sleep;

            else if (x == PlayerActions.Build_House) return buildHouse;

            else if (x == PlayerActions.Get_Axe) return getAxe;

            else if (x == PlayerActions.Get_Bad_Axe) return getBadAxe;

            else if (x == PlayerActions.Get_Medicine) return getMedicine;

            else if (x == PlayerActions.Sell_Wood) return sellWood;

            else if (x == PlayerActions.Steal_Wood) return stealWood;

            else return none;

        }).ToList();

        myFSM = new StateMachine(states);
        isRunning = true;
    }

    IEnumerator DelayNextStep()
    {
        onDelay = true;
        itemTarget = null;
        yield return new WaitForSeconds(0.5f);

        myFSM.NextStep();
        yield return new WaitForSeconds(0.1f);
        onDelay = false;
    }

    public void OnCollisionStay(Collision c)
    {
        if (itemTarget != null && c.gameObject.GetComponent<Item>())
            if (c.gameObject.GetComponent<Item>().type == itemTarget.type && !onDelay)
                OnCollisionAction();
    }

    public void OnCollisionEnter(Collision c)
    {
        if (itemTarget != null && c.gameObject.GetComponent<Item>())
            if (c.gameObject.GetComponent<Item>().type == itemTarget.type && !onDelay)
                OnCollisionAction();
    }

    public void OnTriggerEnter(Collider c)
    {
        if (itemTarget != null && c.gameObject.GetComponent<Item>())
            if (c.gameObject.GetComponent<Item>().type == itemTarget.type && !onDelay)
                OnTriggerAction();
    }

    public void OnTriggerStay(Collider c)
    {
        if (itemTarget != null && c.gameObject.GetComponent<Item>())
            if (c.gameObject.GetComponent<Item>().type == itemTarget.type && !onDelay)
                OnTriggerAction();
    }

    public void StealWood()
    {
        reputationText.text = "Reputation: Villian";
        currentWood += 40;
        StartCoroutine(DelayNextStep());
    }

    public void SellWood()
    {
        reputationText.text = "Reputation: Honorable";
        currentWood -= 20;
        StartCoroutine(DelayNextStep());
    }

    public void GetWood()
    {
        if (itemTarget.type == ItemType.Tree) currentWood += 10;
        itemTarget.gameObject.SetActive(false);
        StartCoroutine(DelayNextStep());
    }

    public void GetAxe()
    {
        axe.SetActive(false);
        playerAxe.SetActive(true);
        StartCoroutine(DelayNextStep());
    }

    public void GetBadAxe()
    {
        badAxe.SetActive(false);
        playerBadAxe.SetActive(true);
        StartCoroutine(DelayNextStep());
    }

    public void BuildHouse()
    {
        tent.SetActive(false);
        house.SetActive(true);
        isRunning = false;
    }

    public void GoToSleep()
    {
        StartCoroutine(DelayNextStep());
    }
}
