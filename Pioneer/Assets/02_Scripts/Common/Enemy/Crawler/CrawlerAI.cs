using UnityEngine.AI;

public class CrawlerAI : EnemyBase, IBegin
{
    // �׺� �޽� 
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();   
    }

    void Update()
    {
        
    }
}
